using System.Reflection;
using System.Text.Json.Nodes;
using MultiAgentFramework.Skills;

namespace MultiAgentFramework;

// -------------------------------------------------------
// Sensitive field hints — used to sanitize outputs
// -------------------------------------------------------
file static class SanitizationPolicy
{
    public static readonly HashSet<string> SensitiveHints =
        new(StringComparer.OrdinalIgnoreCase)
        { "secret", "internal", "policy", "heuristic", "strategy", "ip" };
}

// -------------------------------------------------------
// Parameter normalisation maps (mirrors PARAMETER_MAP in Python)
// -------------------------------------------------------
file static class ParameterMap
{
    /// <summary>
    /// Maps (toolName → (wrongParamName → correctParamName))
    /// </summary>
    public static readonly Dictionary<string, Dictionary<string, string>> Map = new()
    {
        ["create_event"] = new() { ["name"]        = "title" },
        ["create_task"]  = new() { ["name"]        = "task",
                                   ["description"] = "task" },
        ["send_email"]   = new() { ["content"]     = "body",
                                   ["message"]     = "body" },
    };
}

// -------------------------------------------------------
// ToolMeta — holds reflected method info + parameter names
// -------------------------------------------------------
public class ToolMeta
{
    public MethodInfo  Method      { get; init; } = null!;
    public string      Description { get; init; } = "";
    public List<string> Parameters { get; init; } = new();
}

// -------------------------------------------------------
// Agent
// -------------------------------------------------------
public class Agent
{
    public  string Name { get; }
    private readonly OllamaLLMClient _llm;
    private readonly Dictionary<string, ToolMeta> _apiContract;

    public Agent(string name, Dictionary<string, ISkill> skills, OllamaLLMClient llmClient)
    {
        Name         = name;
        _llm         = llmClient;
        _apiContract = BuildApiContract(skills);
    }

    // -------------------------------------------------------
    // Build the explicit allowlist of tools exposed to the LLM
    // -------------------------------------------------------
    private static Dictionary<string, ToolMeta> BuildApiContract(Dictionary<string, ISkill> skills)
    {
        var contract = new Dictionary<string, ToolMeta>();

        foreach (var (toolName, skillObj) in skills)
        {
            var method = skillObj.GetType().GetMethod(toolName,
                BindingFlags.Public | BindingFlags.Instance);

            if (method == null) continue;

            var parameters = method.GetParameters()
                                   .Select(p => p.Name!)
                                   .ToList();

            contract[toolName] = new ToolMeta
            {
                Method      = method,
                Description = skillObj.Description,
                Parameters  = parameters,
                // Store the instance so we can invoke it later
            };

            // We also need the skill instance to invoke the method.
            // Store it via a closure approach — wrap the call.
            _skillInstances[toolName] = skillObj;
        }

        return contract;
    }

    // Companion dict to hold skill instances (needed for MethodInfo.Invoke)
    private static readonly Dictionary<string, ISkill> _skillInstances = new();

    // -------------------------------------------------------
    // Sanitize output — strip keys that look like internal IP
    // -------------------------------------------------------
    private static object SanitizeOutput(object payload)
    {
        if (payload is not Dictionary<string, object> dict) return payload;

        return dict
            .Where(kv => !SanitizationPolicy.SensitiveHints
                          .Any(hint => kv.Key.Contains(hint, StringComparison.OrdinalIgnoreCase)))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    // -------------------------------------------------------
    // Normalize LLM-generated parameters to match method signature
    // -------------------------------------------------------
    private static Dictionary<string, string> NormalizeParameters(
        string toolName,
        Dictionary<string, string> llmParams,
        MethodInfo method)
    {
        // Apply mapping corrections
        if (ParameterMap.Map.TryGetValue(toolName, out var mapping))
        {
            foreach (var (wrong, correct) in mapping)
            {
                if (llmParams.ContainsKey(wrong) && !llmParams.ContainsKey(correct))
                {
                    llmParams[correct] = llmParams[wrong];
                    llmParams.Remove(wrong);
                }
            }
        }

        // Strip parameters not in method signature
        var allowed = method.GetParameters()
                            .Select(p => p.Name!)
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return llmParams
            .Where(kv => allowed.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    // -------------------------------------------------------
    // Execute a user query
    // -------------------------------------------------------
    public object Execute(string userQuery)
    {
        var toolsDescription = _apiContract
            .Select(kv => $"{kv.Key} → {kv.Value.Description}")
            .ToList();

        var toolsSchema = _apiContract.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Parameters);

        var systemPrompt = $"""
            You are {Name}.

            You can use ONLY the following tool API surface:

            {string.Join("\n", toolsDescription)}

            Allowed JSON parameter keys per tool:

            {System.Text.Json.JsonSerializer.Serialize(toolsSchema)}

            Choose the BEST tool for the user request.

            Return ONLY JSON like this:

            {{
                "tool": "tool_name",
                "parameters": {{ }}
            }}

            Examples:

            User: Send an email
            Output:
            {{
                "tool": "send_email",
                "parameters": {{ "to": "...", "body": "..." }}
            }}

            User: Schedule a meeting
            Output:
            {{
                "tool": "create_event",
                "parameters": {{ "title": "...", "date": "...", "time": "..." }}
            }}
            """;

        var messages = new List<ChatMessage>
        {
            new("system", systemPrompt),
            new("user",   userQuery),
        };

        var response = _llm.Chat(messages);

        // -------------------------------------------------------
        // Parse LLM response
        // -------------------------------------------------------
        var toolName   = response["tool"]?.GetValue<string>();
        var paramsNode = response["parameters"] as JsonObject;

        if (string.IsNullOrWhiteSpace(toolName))
            return response["response"]?.GetValue<string>() ?? "LLM did not return a tool";

        if (!_apiContract.TryGetValue(toolName, out var meta))
            return new Dictionary<string, string>
            {
                ["error"] = $"Tool '{toolName}' is not exposed by {Name}. " +
                            "Only explicitly allowlisted APIs are callable."
            };

        // -------------------------------------------------------
        // Build parameter dictionary from JSON
        // -------------------------------------------------------
        var llmParams = paramsNode?
            .ToDictionary(kv => kv.Key, kv => kv.Value?.GetValue<string>() ?? "")
            ?? new Dictionary<string, string>();

        var cleanedParams = NormalizeParameters(toolName, llmParams, meta.Method);

        // Fallback: if LLM forgot required fields
        if (toolName == "create_task"  && !cleanedParams.ContainsKey("task"))
            cleanedParams["task"]  = userQuery;

        if (toolName == "create_event" && !cleanedParams.ContainsKey("title"))
            cleanedParams["title"] = userQuery;

        // -------------------------------------------------------
        // Invoke skill method via reflection
        // -------------------------------------------------------
        try
        {
            var skillInstance = _skillInstances[toolName];
            var methodParams  = meta.Method.GetParameters();

            var args = methodParams.Select(p =>
            {
                if (cleanedParams.TryGetValue(p.Name!, out var val))
                    return (object)val;

                if (p.HasDefaultValue)
                    return p.DefaultValue!;

                return (object)"";
            }).ToArray();

            var rawResult = meta.Method.Invoke(skillInstance, args)!;
            return SanitizeOutput(rawResult);
        }
        catch (Exception ex)
        {
            return new Dictionary<string, string> { ["error"] = ex.Message };
        }
    }
}

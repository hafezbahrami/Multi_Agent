using System.Reflection;
using System.Text.Json.Nodes;
using MultiAgentFramework.Skills;

namespace MultiAgentFramework.Core;

file static class ParameterMap
{
    public static readonly Dictionary<string, Dictionary<string, string>> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["create_event"] = new(StringComparer.OrdinalIgnoreCase) { ["name"] = "title" },
        ["create_task"] = new(StringComparer.OrdinalIgnoreCase) { ["name"] = "task", ["description"] = "task" },
        ["send_email"] = new(StringComparer.OrdinalIgnoreCase) { ["content"] = "body", ["message"] = "body" }
    };
}

public sealed class ToolMeta
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required MethodInfo Method { get; init; }
    public required ISkill SkillInstance { get; init; }
    public required IReadOnlyList<string> Parameters { get; init; }
}

public sealed class Agent
{
    public string Name { get; }

    private readonly OllamaLLMClient _llm;
    private readonly Dictionary<string, ToolMeta> _apiContract;

    public Agent(string name, Dictionary<string, ISkill> skills, OllamaLLMClient llmClient)
    {
        Name = name;
        _llm = llmClient;
        _apiContract = BuildApiContract(skills);
    }

    private static Dictionary<string, ToolMeta> BuildApiContract(Dictionary<string, ISkill> skills)
    {
        var contract = new Dictionary<string, ToolMeta>(StringComparer.OrdinalIgnoreCase);

        foreach (var (toolName, skillObj) in skills)
        {
            var method = skillObj.GetType().GetMethod(toolName, BindingFlags.Public | BindingFlags.Instance);
            if (method is null)
            {
                throw new InvalidOperationException($"Tool '{toolName}' not found on skill '{skillObj.GetType().Name}'.");
            }

            contract[toolName] = new ToolMeta
            {
                Name = toolName,
                Description = skillObj.Description,
                Method = method,
                SkillInstance = skillObj,
                Parameters = method.GetParameters().Select(p => p.Name ?? string.Empty).ToArray()
            };
        }

        return contract;
    }

    public object Execute(string userQuery)
    {
        var toolsDescription = _apiContract
            .Select(kv => $"{kv.Key} -> {kv.Value.Description}")
            .ToList();

        var toolsSchema = _apiContract.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Parameters,
            StringComparer.OrdinalIgnoreCase);

        var systemPrompt = $$"""
            You are {{Name}}.

            You can use ONLY these tools:
            {{string.Join("\n", toolsDescription)}}

            Allowed JSON parameter keys per tool:
            {{System.Text.Json.JsonSerializer.Serialize(toolsSchema)}}

            Return ONLY JSON:
            {
              "tool": "tool_name",
              "parameters": { }
            }
            """;

        var response = _llm.Chat(new List<ChatMessage>
        {
            new("system", systemPrompt),
            new("user", userQuery)
        });

        var toolName = response["tool"]?.GetValue<string>();
        var paramsNode = response["parameters"] as JsonObject;

        if (string.IsNullOrWhiteSpace(toolName))
        {
            return response["response"]?.GetValue<string>() ?? "LLM did not return a tool";
        }

        if (!_apiContract.TryGetValue(toolName, out var meta))
        {
            return new Dictionary<string, string>
            {
                ["error"] = $"Tool '{toolName}' is not exposed by {Name}."
            };
        }

        var llmParams = paramsNode?.ToDictionary(kv => kv.Key, kv => kv.Value?.GetValue<string>() ?? string.Empty)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var cleanedParams = NormalizeParameters(toolName, llmParams, meta.Method);

        if (toolName.Equals("create_task", StringComparison.OrdinalIgnoreCase) && !cleanedParams.ContainsKey("task"))
        {
            cleanedParams["task"] = userQuery;
        }

        if (toolName.Equals("create_event", StringComparison.OrdinalIgnoreCase) && !cleanedParams.ContainsKey("title"))
        {
            cleanedParams["title"] = userQuery;
        }

        try
        {
            var args = meta.Method.GetParameters().Select(p =>
            {
                if (cleanedParams.TryGetValue(p.Name ?? string.Empty, out var value))
                {
                    return (object)value;
                }

                if (p.HasDefaultValue)
                {
                    return p.DefaultValue!;
                }

                return string.Empty;
            }).ToArray();

            var raw = meta.Method.Invoke(meta.SkillInstance, args);
            return raw ?? new Dictionary<string, string> { ["error"] = "Tool returned null" };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, string> { ["error"] = ex.Message };
        }
    }

    private static Dictionary<string, string> NormalizeParameters(
        string toolName,
        Dictionary<string, string> llmParams,
        MethodInfo method)
    {
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

        var allowed = method.GetParameters()
            .Select(p => p.Name ?? string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return llmParams
            .Where(kv => allowed.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
    }
}

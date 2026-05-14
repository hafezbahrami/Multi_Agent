using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MultiAgentFramework;

public class OllamaLLMClient
{
    private readonly string _url;
    private readonly string _model;
    private readonly HttpClient _http;

    public OllamaLLMClient(string? url = null, string? model = null)
    {
        _url   = url   ?? Environment.GetEnvironmentVariable("OLLAMA_URL")   ?? "http://localhost:11434";
        _model = model ?? Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.2:3b";
        _http  = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    /// <summary>
    /// Send a list of chat messages to Ollama.
    /// Returns a parsed JsonObject if the response contains JSON, otherwise a plain text wrapper.
    /// </summary>
    public JsonObject Chat(List<ChatMessage> messages, List<ToolDefinition>? tools = null)
    {
        var prompt = BuildPrompt(messages, tools);

        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                model       = _model,
                prompt      = prompt,
                stream      = false,
                temperature = 0.7,
            });

            var content  = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = _http.PostAsync($"{_url}/api/generate", content).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var doc  = JsonDocument.Parse(json);
            var text = doc.RootElement.GetProperty("response").GetString() ?? "";

            return ParseResponse(text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ollama error: {ex.Message}");
            return MockResponse(messages);
        }
    }

    // -------------------------------------------------------
    private string BuildPrompt(List<ChatMessage> messages, List<ToolDefinition>? tools)
    {
        var sb = new StringBuilder();

        foreach (var msg in messages)
            sb.AppendLine($"{msg.Role.ToUpper()}: {msg.Content}\n");

        if (tools != null && tools.Count > 0)
        {
            sb.AppendLine("\nAvailable tools:");
            foreach (var t in tools)
                sb.AppendLine($"- {t.Name}: {t.Description}");
        }

        sb.Append("\nASSISTANT: ");
        return sb.ToString();
    }

    private JsonObject ParseResponse(string text)
    {
        var start = text.IndexOf('{');
        var end   = text.LastIndexOf('}') + 1;

        if (start >= 0 && end > start)
        {
            try
            {
                var slice = text[start..end];
                var node  = JsonNode.Parse(slice);
                if (node is JsonObject obj) return obj;
            }
            catch { /* fall through */ }
        }

        return new JsonObject { ["response"] = text };
    }

    private JsonObject MockResponse(List<ChatMessage> messages)
    {
        var last = messages.LastOrDefault()?.Content ?? "";
        return new JsonObject { ["response"] = $"LLM received: {last}" };
    }
}

public record ChatMessage(string Role, string Content);
public record ToolDefinition(string Name, string Description);

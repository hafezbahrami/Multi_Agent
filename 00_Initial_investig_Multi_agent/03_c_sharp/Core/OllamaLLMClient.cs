using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MultiAgentFramework.Core;

public class OllamaLLMClient
{
    private readonly string _url;
    private readonly string _model;
    private readonly HttpClient _http;

    public OllamaLLMClient(string? url = null, string? model = null)
    {
        _url = url ?? Environment.GetEnvironmentVariable("OLLAMA_URL") ?? "http://localhost:11434";
        _model = model ?? Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.2:3b";
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public JsonObject Chat(List<ChatMessage> messages)
    {
        var prompt = BuildPrompt(messages);

        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                model = _model,
                prompt,
                stream = false,
                temperature = 0.7
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = _http.PostAsync($"{_url}/api/generate", content).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var doc = JsonDocument.Parse(json);
            var text = doc.RootElement.GetProperty("response").GetString() ?? "";
            return ParseResponse(text);
        }
        catch
        {
            var last = messages.LastOrDefault()?.Content ?? "";
            return new JsonObject { ["response"] = $"LLM unavailable. Echo: {last}" };
        }
    }

    private static string BuildPrompt(IEnumerable<ChatMessage> messages)
    {
        var sb = new StringBuilder();
        foreach (var message in messages)
        {
            sb.AppendLine($"{message.Role.ToUpperInvariant()}: {message.Content}\n");
        }

        sb.Append("\nASSISTANT: ");
        return sb.ToString();
    }

    private static JsonObject ParseResponse(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}') + 1;

        if (start >= 0 && end > start)
        {
            try
            {
                var node = JsonNode.Parse(text[start..end]);
                if (node is JsonObject obj)
                {
                    return obj;
                }
            }
            catch
            {
            }
        }

        return new JsonObject { ["response"] = text };
    }
}

public sealed record ChatMessage(string Role, string Content);

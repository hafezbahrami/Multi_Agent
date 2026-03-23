using McpTaskExample.Contracts;

namespace McpTaskExample.Execution;

public sealed class EntitlementPolicy
{
    private readonly Dictionary<string, HashSet<string>> _allowedByCaller = new(StringComparer.OrdinalIgnoreCase)
    {
        ["basic-client"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "create_task", "search" },
        ["pro-client"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "send_email", "create_event", "create_task", "search" }
    };

    public bool IsAllowed(string callerId, string toolName)
    {
        return _allowedByCaller.TryGetValue(callerId, out var tools) && tools.Contains(toolName);
    }
}

public sealed class MeteringSink
{
    private readonly List<MeterEvent> _events = new();

    public void Emit(MeterEvent meterEvent)
    {
        _events.Add(meterEvent);
    }

    public IReadOnlyList<MeterEvent> ReadAll() => _events;
}

public static class OutboundRedactor
{
    private static readonly string[] SensitiveHints =
    {
        "secret", "internal", "policy", "heuristic", "strategy", "ip"
    };

    public static Dictionary<string, object> Redact(Dictionary<string, object> payload)
    {
        return payload
            .Where(kv => !SensitiveHints.Any(h => kv.Key.Contains(h, StringComparison.OrdinalIgnoreCase)))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}

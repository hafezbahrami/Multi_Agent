using MultiAgentFramework.Contracts;

namespace MultiAgentFramework.Execution;

public sealed class UsageMeter
{
    private readonly List<UsageEvent> _events = new();

    public void Emit(UsageEvent usageEvent)
    {
        _events.Add(usageEvent);
    }

    public IReadOnlyList<UsageEvent> ReadAll() => _events;
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
            .Where(kv => !SensitiveHints.Any(hint => kv.Key.Contains(hint, StringComparison.OrdinalIgnoreCase)))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}

namespace MultiAgentFramework.Execution;

public sealed class EntitlementPolicy
{
    private readonly Dictionary<string, HashSet<string>> _rules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["basic-client"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "create_task", "search"
        },
        ["pro-client"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "send_email", "create_event", "create_task", "search"
        }
    };

    public bool IsAllowed(string callerId, string tool)
    {
        return _rules.TryGetValue(callerId, out var allowed) && allowed.Contains(tool);
    }
}

namespace MultiAgentFramework.Contracts;

public sealed record ToolRequest(
    string RequestId,
    string CallerId,
    string Tool,
    Dictionary<string, string> Parameters);

public sealed record ToolResponse(
    string RequestId,
    bool Success,
    string ErrorCategory,
    string Message,
    Dictionary<string, object>? Data);

public sealed record UsageEvent(
    DateTimeOffset Timestamp,
    string RequestId,
    string CallerId,
    string Tool,
    string ToolVersion,
    bool Success,
    long LatencyMs,
    string ErrorCategory,
    int BilledUnits);

namespace McpTaskExample.Contracts;

public sealed record ToolRequestDto(
    string RequestId,
    string CallerId,
    string Tool,
    Dictionary<string, string> Parameters);

public sealed record ToolResponseDto(
    string RequestId,
    bool Success,
    string ErrorCategory,
    string Message,
    Dictionary<string, object>? Data);

public sealed record MeterEvent(
    DateTimeOffset Timestamp,
    string RequestId,
    string CallerId,
    string ToolName,
    string ToolVersion,
    bool Success,
    long LatencyMs,
    string ErrorCategory,
    int BilledUnits);

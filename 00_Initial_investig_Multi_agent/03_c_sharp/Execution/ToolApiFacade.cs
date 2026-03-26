using System.Diagnostics;
using MultiAgentFramework.Contracts;

namespace MultiAgentFramework.Execution;

public sealed class ToolApiFacade
{
    private readonly IReadOnlyDictionary<string, ToolCatalogItem> _catalog;
    private readonly EntitlementPolicy _policy;
    private readonly UsageMeter _meter;

    public ToolApiFacade(
        IReadOnlyDictionary<string, ToolCatalogItem> catalog,
        EntitlementPolicy policy,
        UsageMeter meter)
    {
        _catalog = catalog;
        _policy = policy;
        _meter = meter;
    }

    public ToolResponse Execute(ToolRequest request)
    {
        var sw = Stopwatch.StartNew();
        var success = false;
        var errorCategory = "none";
        var message = "ok";
        Dictionary<string, object>? data = null;
        var version = "unknown";

        try
        {
            if (!_catalog.TryGetValue(request.Tool, out var tool))
            {
                errorCategory = "validation_error";
                message = $"Unknown tool '{request.Tool}'.";
                return BuildResponse();
            }

            version = tool.Version;

            if (!_policy.IsAllowed(request.CallerId, request.Tool))
            {
                errorCategory = "entitlement_error";
                message = $"Caller '{request.CallerId}' is not entitled to '{request.Tool}'.";
                return BuildResponse();
            }

            var args = tool.Method.GetParameters().Select(p =>
            {
                var name = p.Name ?? string.Empty;
                if (request.Parameters.TryGetValue(name, out var value))
                {
                    return (object)value;
                }

                if (p.HasDefaultValue)
                {
                    return p.DefaultValue!;
                }

                return string.Empty;
            }).ToArray();

            var raw = tool.Method.Invoke(tool.SkillInstance, args);
            if (raw is not Dictionary<string, object> payload)
            {
                errorCategory = "internal_error";
                message = "Tool returned unexpected payload.";
                return BuildResponse();
            }

            data = OutboundRedactor.Redact(payload);
            success = true;
            return BuildResponse();
        }
        catch
        {
            errorCategory = "internal_error";
            message = "Internal execution failure.";
            return BuildResponse();
        }
        finally
        {
            sw.Stop();
            _meter.Emit(new UsageEvent(
                Timestamp: DateTimeOffset.UtcNow,
                RequestId: request.RequestId,
                CallerId: request.CallerId,
                Tool: request.Tool,
                ToolVersion: version,
                Success: success,
                LatencyMs: sw.ElapsedMilliseconds,
                ErrorCategory: errorCategory,
                BilledUnits: 1));
        }

        ToolResponse BuildResponse() => new(
            RequestId: request.RequestId,
            Success: success,
            ErrorCategory: success ? "none" : errorCategory,
            Message: message,
            Data: data);
    }
}

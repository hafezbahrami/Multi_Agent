using System.Diagnostics;
using McpTaskExample.Contracts;

namespace McpTaskExample.Execution;

public sealed class ToolApiFacade
{
    private readonly IReadOnlyDictionary<string, ToolMetadata> _catalog;
    private readonly EntitlementPolicy _policy;
    private readonly MeteringSink _meter;

    public ToolApiFacade(
        IReadOnlyDictionary<string, ToolMetadata> catalog,
        EntitlementPolicy policy,
        MeteringSink meter)
    {
        _catalog = catalog;
        _policy = policy;
        _meter = meter;
    }

    public ToolResponseDto Execute(ToolRequestDto request)
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
                return BuildResponse(false);
            }

            version = tool.Version;

            if (!_policy.IsAllowed(request.CallerId, request.Tool))
            {
                errorCategory = "entitlement_error";
                message = $"Caller '{request.CallerId}' is not entitled for tool '{request.Tool}'.";
                return BuildResponse(false);
            }

            var args = tool.Method.GetParameters().Select(p =>
            {
                if (request.Parameters.TryGetValue(p.Name ?? string.Empty, out var val))
                {
                    return (object)val;
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
                message = "Tool returned unexpected payload type.";
                return BuildResponse(false);
            }

            data = OutboundRedactor.Redact(payload);
            success = true;
            return BuildResponse(true);
        }
        catch
        {
            errorCategory = "internal_error";
            message = "Internal execution failure.";
            return BuildResponse(false);
        }
        finally
        {
            sw.Stop();
            _meter.Emit(new MeterEvent(
                DateTimeOffset.UtcNow,
                request.RequestId,
                request.CallerId,
                request.Tool,
                version,
                success,
                sw.ElapsedMilliseconds,
                errorCategory,
                BilledUnits: 1));
        }

        ToolResponseDto BuildResponse(bool ok)
        {
            return new ToolResponseDto(
                RequestId: request.RequestId,
                Success: ok,
                ErrorCategory: ok ? "none" : errorCategory,
                Message: message,
                Data: data);
        }
    }
}

using System.Diagnostics;
using System.Text.Json;

namespace MQ.Office.MCP.Server.IntegrationTests;

/// <summary>
/// Minimal Model Context Protocol JSON-RPC client over stdio. Implements the
/// MCP handshake (initialize + notifications/initialized) and the request/
/// response pairs needed by these integration tests.
/// </summary>
internal sealed class McpJsonRpcClient(Process server)
{
    private int _id;

    public async Task InitializeAsync()
    {
        await CallAsync("initialize", new
        {
            protocolVersion = "2024-11-05",
            capabilities = new { },
            clientInfo = new { name = "MQ.IntegrationTests", version = "1.0.0" }
        });
        await SendNotificationAsync("notifications/initialized", new { });
    }

    public Task<JsonElement> ListToolsAsync() => CallAsync("tools/list", new { });

    public Task<JsonElement> CallToolAsync(string toolName, object? arguments = null) =>
        CallAsync("tools/call", new { name = toolName, arguments = arguments ?? new { } });

    private async Task SendNotificationAsync(string method, object @params)
    {
        var notification = new { jsonrpc = "2.0", method, @params };
        await server.StandardInput.WriteLineAsync(JsonSerializer.Serialize(notification));
        await server.StandardInput.FlushAsync();
    }

    private async Task<JsonElement> CallAsync(string method, object @params)
    {
        int id = Interlocked.Increment(ref _id);
        var request = new { jsonrpc = "2.0", id, method, @params };
        await server.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request));
        await server.StandardInput.FlushAsync();

        var readTask = server.StandardOutput.ReadLineAsync();
        var completed = await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromSeconds(15)));
        if (completed != readTask)
            throw new TimeoutException($"No response received for method '{method}' (id={id}).");

        string? line = await readTask
            ?? throw new InvalidOperationException("Server stdout closed unexpectedly.");

        using var doc = JsonDocument.Parse(line);
        return doc.RootElement.Clone();
    }
}

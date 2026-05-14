namespace MQ.Office.MCP.Server.IntegrationTests;

/// <summary>
/// Smoke test: verifies the MQ.Office.MCP.Server process starts and is alive.
/// Real MCP JSON-RPC handshake tests can be added here once basic plumbing is verified.
/// </summary>
public class McpServerSmokeTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;

    public McpServerSmokeTests(McpServerFixture fixture) => _fixture = fixture;

    [Fact]
    public void Server_Starts_AndStaysAlive()
    {
        // Give the host a moment to spin up.
        Thread.Sleep(500);
        // Note: do NOT eagerly read StandardError here - it blocks while the
        // server is alive. Just check HasExited.
        Assert.False(_fixture.Server.HasExited,
            "MQ.Office.MCP.Server exited unexpectedly.");
    }
}

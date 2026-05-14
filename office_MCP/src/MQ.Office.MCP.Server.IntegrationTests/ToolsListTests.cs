using System.Text.Json;

namespace MQ.Office.MCP.Server.IntegrationTests;

/// <summary>
/// End-to-end test: verifies the server advertises every expected tool.
/// Exercises the full server boot path (DI, MCP host, stdio transport,
/// tool discovery) without requiring Word or Excel to be installed.
/// </summary>
public class ToolsListTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;

    public ToolsListTests(McpServerFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ToolsList_ContainsAllExpectedTools()
    {
        var client = new McpJsonRpcClient(_fixture.Server);
        await client.InitializeAsync();
        var response = await client.ListToolsAsync();

        var toolNames = response
            .GetProperty("result")
            .GetProperty("tools")
            .EnumerateArray()
            .Select(t => t.GetProperty("name").GetString()!)
            .ToHashSet();

        var expected = new[]
        {
            "word_open_document",
            "word_save_document",
            "word_close_document",
            "excel_open_workbook",
            "excel_save_workbook",
            "excel_close_workbook",
        };

        foreach (var name in expected)
            Assert.Contains(name, toolNames);
    }
}

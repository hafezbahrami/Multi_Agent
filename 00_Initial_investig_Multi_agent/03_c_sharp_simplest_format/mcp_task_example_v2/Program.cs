using McpTaskExample.Contracts;
using McpTaskExample.Discovery;
using McpTaskExample.Execution;
using McpTaskExample.Skills;

var skills = new object[]
{
    new WorkflowSkill(),
    new ResearchSkill()
};

// Task 2: discoverability + generated catalog
var generator = new ToolCatalogGenerator();
var catalog = generator.BuildCatalog(skills);
Console.WriteLine("=== Tool Catalog (Task 2) ===");
Console.WriteLine(generator.ToJson(catalog));

// Task 3 + 5: thin API facade with policy, metering, redaction
var facade = new ToolApiFacade(
    catalog,
    new EntitlementPolicy(),
    new MeteringSink());

var requests = new[]
{
    new ToolRequestDto(
        RequestId: "req-1",
        CallerId: "pro-client",
        Tool: "send_email",
        Parameters: new Dictionary<string, string>
        {
            ["to"] = "hafezbahrami@gmail.com",
            ["body"] = "This is a test from the clean example.",
            ["subject"] = "Task Example"
        }),
    new ToolRequestDto(
        RequestId: "req-2",
        CallerId: "basic-client",
        Tool: "send_email",
        Parameters: new Dictionary<string, string>
        {
            ["to"] = "hafezbahrami@gmail.com",
            ["body"] = "This should be blocked by entitlement."
        }),
    new ToolRequestDto(
        RequestId: "req-3",
        CallerId: "basic-client",
        Tool: "create_task",
        Parameters: new Dictionary<string, string>
        {
            ["task"] = "Review quarterly report",
            ["priority"] = "high"
        })
};

Console.WriteLine("\n=== Tool API Responses (Task 3 + 5) ===");
foreach (var request in requests)
{
    var response = facade.Execute(request);
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(response));
}

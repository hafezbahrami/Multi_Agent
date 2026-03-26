using MultiAgentFramework.Contracts;
using MultiAgentFramework.Core;
using MultiAgentFramework.Discovery;
using MultiAgentFramework.Execution;
using MultiAgentFramework.Skills;

var llmClient = new OllamaLLMClient();

// Legacy multi-agent style (from original codebase)
var workflowAgent = new Agent(
    name: "WorkflowAgent",
    skills: new Dictionary<string, ISkill>
    {
        ["send_email"] = new EmailSkill(),
        ["create_event"] = new CalendarSkill(),
        ["create_task"] = new TaskSkill()
    },
    llmClient: llmClient);

var researchAgent = new Agent(
    name: "ResearchAgent",
    skills: new Dictionary<string, ISkill>
    {
        ["search"] = new WebSearchSkill()
    },
    llmClient: llmClient);

var planner = new MultiAgentPlanner(
    new List<Agent> { workflowAgent, researchAgent },
    llmClient);

Console.WriteLine("=== Planner Demo (legacy flow) ===");
var plannerQueries = new[]
{
    "Send an email to hafezbahrami@gmail.com saying this is a test",
    "Create a high priority task to review the quarterly report",
    "Search for the latest AI trends in 2026"
};

foreach (var query in plannerQueries)
{
    var response = planner.RouteQuery(query);
    Console.WriteLine($"Query: {query}");
    Console.WriteLine($"Response: {FormatResponse(response)}\n");
}

// New MCP-style features (from mcp_task_example_v2)
Console.WriteLine("=== Discoverability Catalog (Task 2) ===");
var generator = new ToolCatalogGenerator();
var skillObjects = new object[]
{
    new EmailSkill(),
    new CalendarSkill(),
    new TaskSkill(),
    new WebSearchSkill()
};
var catalog = generator.BuildCatalog(skillObjects);
Console.WriteLine(generator.ToJson(catalog));

Console.WriteLine("\n=== API Facade + Entitlement + Metering (Tasks 3 & 5) ===");
var meter = new UsageMeter();
var facade = new ToolApiFacade(catalog, new EntitlementPolicy(), meter);

var requests = new[]
{
    new ToolRequest(
        RequestId: "req-1",
        CallerId: "pro-client",
        Tool: "send_email",
        Parameters: new Dictionary<string, string>
        {
            ["to"] = "hafezbahrami@gmail.com",
            ["subject"] = "Merged Demo",
            ["body"] = "This is a clean merged codebase demo."
        }),
    new ToolRequest(
        RequestId: "req-2",
        CallerId: "basic-client",
        Tool: "send_email",
        Parameters: new Dictionary<string, string>
        {
            ["to"] = "hafezbahrami@gmail.com",
            ["body"] = "This should be blocked by entitlement."
        }),
    new ToolRequest(
        RequestId: "req-3",
        CallerId: "basic-client",
        Tool: "search",
        Parameters: new Dictionary<string, string>
        {
            ["query"] = "MCP servers in engineering workflows"
        })
};

foreach (var request in requests)
{
    var response = facade.Execute(request);
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(response));
}

Console.WriteLine("\n=== Meter Events ===");
foreach (var usage in meter.ReadAll())
{
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(usage));
}

static string FormatResponse(object response)
{
    return response switch
    {
        Dictionary<string, object> dict => string.Join(", ", dict.Select(kv => $"{kv.Key}={kv.Value}")),
        Dictionary<string, string> dict => string.Join(", ", dict.Select(kv => $"{kv.Key}={kv.Value}")),
        _ => response?.ToString() ?? "(null)"
    };
}

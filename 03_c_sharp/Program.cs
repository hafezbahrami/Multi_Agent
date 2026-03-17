using MultiAgentFramework;
using MultiAgentFramework.Skills;

var llmClient = new OllamaLLMClient();

// -------------------------------------------------------
// Define agents with their skills
// -------------------------------------------------------
var workflowAgent = new Agent(
    name: "WorkflowAgent",
    skills: new Dictionary<string, ISkill>
    {
        ["send_email"]   = new EmailSkill(),
        ["create_event"] = new CalendarSkill(),
        ["create_task"]  = new TaskSkill(),
    },
    llmClient: llmClient
);

var researchAgent = new Agent(
    name: "ResearchAgent",
    skills: new Dictionary<string, ISkill>
    {
        ["search"] = new WebSearchSkill(),
    },
    llmClient: llmClient
);

// -------------------------------------------------------
// Orchestrator
// -------------------------------------------------------
var planner = new MultiAgentPlanner(
    agents: new List<Agent> { workflowAgent, researchAgent },
    llmClient: llmClient
);

// -------------------------------------------------------
// Demo queries
// -------------------------------------------------------
var demoQueries = new[]
{
    """Send an email to "hafezbahrami@gmail.com" saying "This is a test" """,
    "Schedule a meeting called 'Team Sync' for tomorrow at 2 PM",
    "Create a high priority task to review the quarterly report",
    "Search for the latest AI trends in 2026",
};

foreach (var query in demoQueries)
{
    Console.WriteLine($"\nQuery: {query}");
    var response = planner.RouteQuery(query);
    Console.WriteLine($"Response: {FormatResponse(response)}");
}

// -------------------------------------------------------
// Interactive mode (opt-in)
// -------------------------------------------------------
const bool doInteractive = false;

if (doInteractive)
{
    Console.WriteLine("\nInteractive Mode. Type 'exit' to quit.");
    while (true)
    {
        Console.Write("You: ");
        var input = Console.ReadLine()?.Trim() ?? "";
        if (input.ToLower() is "exit" or "quit") break;
        var response = planner.RouteQuery(input);
        Console.WriteLine($"Planner: {FormatResponse(response)}");
    }
}

// -------------------------------------------------------
// Helper: pretty-print response
// -------------------------------------------------------
static string FormatResponse(object response)
{
    return response switch
    {
        Dictionary<string, object> dict =>
            string.Join(", ", dict.Select(kv => $"{kv.Key}={kv.Value}")),
        Dictionary<string, string> dict =>
            string.Join(", ", dict.Select(kv => $"{kv.Key}={kv.Value}")),
        _ => response?.ToString() ?? "(null)"
    };
}

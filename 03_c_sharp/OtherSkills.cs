namespace MultiAgentFramework.Skills;

public class TaskSkill : ISkill
{
    public string Description => "Create and manage tasks";

    private readonly Dictionary<string, string> _priorityPolicy = new()
    {
        ["high"]   = "executive/escalation",
        ["medium"] = "team-backlog",
        ["low"]    = "icebox",
    };

    private string NormalizePriority(string? priority)
    {
        var p = (priority ?? "medium").ToLower().Trim();
        return new[] { "high", "medium", "low" }.Contains(p) ? p : "medium";
    }

    public Dictionary<string, object> create_task(string task, string priority = "medium")
    {
        var normalizedPriority = NormalizePriority(priority);
        Console.WriteLine($"\nTASK CREATED!\nTask: {task}\nPriority: {normalizedPriority}");

        return new Dictionary<string, object>
        {
            ["status"]   = "success",
            ["task"]     = task,
            ["priority"] = normalizedPriority,
        };
    }
}

public class WebSearchSkill : ISkill
{
    public string Description => "Search the web for information";

    private readonly string _rankingProfile = "research-ranking-v2";

    public Dictionary<string, object> search(string query)
    {
        Console.WriteLine($"\nWEB SEARCH EXECUTED!\nQuery: {query}");

        return new Dictionary<string, object>
        {
            ["status"]  = "success",
            ["results"] = new List<string>
            {
                $"Result 1 for {query}",
                $"Result 2 for {query}",
            },
        };
    }
}

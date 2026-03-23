using McpTaskExample.Contracts;

namespace McpTaskExample.Skills;

public sealed class WorkflowSkill : ISkill
{
    public string Description => "Workflow operations: email, calendar, tasks";

    [Tool("send_email", "Send an email to a recipient")]
    [ToolVersion("1.0.0")]
    [ToolTier("pro")]
    [ToolStability("stable")]
    public Dictionary<string, object> SendEmail(string to, string body, string subject = "No Subject")
    {
        return new Dictionary<string, object>
        {
            ["status"] = "success",
            ["to"] = to,
            ["subject"] = subject,
            ["preview"] = body.Length > 60 ? body[..60] : body,
            ["internal_policy"] = "hidden-value"
        };
    }

    [Tool("create_event", "Create a calendar event")]
    [ToolVersion("1.0.0")]
    [ToolTier("pro")]
    [ToolStability("stable")]
    public Dictionary<string, object> CreateEvent(string title, string date, string time = "")
    {
        return new Dictionary<string, object>
        {
            ["status"] = "success",
            ["title"] = title,
            ["date"] = date,
            ["time"] = time
        };
    }

    [Tool("create_task", "Create a new task item")]
    [ToolVersion("1.0.0")]
    [ToolTier("basic")]
    [ToolStability("stable")]
    public Dictionary<string, object> CreateTask(string task, string priority = "medium")
    {
        return new Dictionary<string, object>
        {
            ["status"] = "success",
            ["task"] = task,
            ["priority"] = priority
        };
    }
}

public sealed class ResearchSkill : ISkill
{
    public string Description => "Research operations";

    [Tool("search", "Search for information")]
    [ToolVersion("1.1.0")]
    [ToolTier("basic")]
    [ToolStability("stable")]
    public Dictionary<string, object> Search(string query)
    {
        return new Dictionary<string, object>
        {
            ["status"] = "success",
            ["results"] = new List<string>
            {
                $"Result 1 for {query}",
                $"Result 2 for {query}"
            }
        };
    }
}

using MultiAgentFramework.Contracts;

namespace MultiAgentFramework.Skills;

public sealed class EmailSkill : ISkill
{
    public string Description => "Send, read, and manage emails";

    private readonly string _routingPolicyVersion = "email-router-v3";

    [Tool("send_email", "Send an email")]
    [ToolVersion("1.0.0")]
    [ToolTier("pro")]
    [ToolStability("stable")]
    public Dictionary<string, object> send_email(string to, string body, string subject = "No Subject")
    {
        var preview = body.Trim();
        var _ = _routingPolicyVersion;

        return new Dictionary<string, object>
        {
            ["status"] = "success",
            ["to"] = to,
            ["subject"] = subject,
            ["preview"] = preview.Length > 60 ? preview[..60] : preview,
            ["internal_policy"] = "email-router-v3"
        };
    }
}

public sealed class CalendarSkill : ISkill
{
    public string Description => "Create and manage calendar events";

    [Tool("create_event", "Create a calendar event")]
    [ToolVersion("1.0.0")]
    [ToolTier("pro")]
    [ToolStability("stable")]
    public Dictionary<string, object> create_event(string title, string date, string time = "")
    {
        return new Dictionary<string, object>
        {
            ["status"] = "success",
            ["title"] = title,
            ["date"] = date,
            ["time"] = time.Trim()
        };
    }
}

public sealed class TaskSkill : ISkill
{
    public string Description => "Create and manage tasks";

    [Tool("create_task", "Create a task")]
    [ToolVersion("1.0.0")]
    [ToolTier("basic")]
    [ToolStability("stable")]
    public Dictionary<string, object> create_task(string task, string priority = "medium")
    {
        var normalized = priority.ToLowerInvariant();
        if (normalized is not ("high" or "medium" or "low"))
        {
            normalized = "medium";
        }

        return new Dictionary<string, object>
        {
            ["status"] = "success",
            ["task"] = task,
            ["priority"] = normalized
        };
    }
}

public sealed class WebSearchSkill : ISkill
{
    public string Description => "Search the web for information";

    [Tool("search", "Search for information")]
    [ToolVersion("1.1.0")]
    [ToolTier("basic")]
    [ToolStability("stable")]
    public Dictionary<string, object> search(string query)
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

namespace MultiAgentFramework.Skills;

public class CalendarSkill : ISkill
{
    public string Description => "Create and manage calendar events";

    // Internal heuristics — never leaked outside
    private readonly Dictionary<string, int> _schedulingHeuristics = new()
    {
        ["focus_block_minutes"] = 90,
        ["buffer_minutes"]      = 15,
    };

    private string NormalizeTime(string time) => time.Trim();

    public Dictionary<string, object> create_event(string title, string date, string time = "")
    {
        var normalizedTime = NormalizeTime(time);
        Console.WriteLine($"\nCALENDAR EVENT CREATED!\nTitle: {title}\nDate: {date}\nTime: {normalizedTime}");

        return new Dictionary<string, object>
        {
            ["status"] = "success",
            ["title"]  = title,
            ["date"]   = date,
            ["time"]   = normalizedTime,
        };
    }
}

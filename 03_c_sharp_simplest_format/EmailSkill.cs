namespace MultiAgentFramework.Skills;

public class EmailSkill : ISkill
{
    public string Description => "Send, read, and manage emails";

    // Internal/IP logic — never exposed as a tool
    private readonly string _routingPolicyVersion = "email-router-v3";

    private string SanitizeBody(string body) => body.Trim();

    private string SelectMailChannel()
    {
        return _routingPolicyVersion == "email-router-v3" ? "primary" : "fallback";
    }

    public Dictionary<string, object> send_email(string to, string body, string subject = "No Subject")
    {
        var cleanBody = SanitizeBody(body);
        var _ = SelectMailChannel(); // Internal — not returned

        Console.WriteLine($"\nEMAIL SENT!\nTo: {to}\nSubject: {subject}\nBody: {cleanBody}");

        return new Dictionary<string, object>
        {
            ["status"]  = "success",
            ["to"]      = to,
            ["subject"] = subject,
            ["preview"] = cleanBody.Length > 50 ? cleanBody[..50] : cleanBody,
        };
    }
}

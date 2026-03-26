namespace MultiAgentFramework.Core;

public sealed class MultiAgentPlanner
{
    private readonly IReadOnlyList<Agent> _agents;
    private readonly OllamaLLMClient _llm;

    public MultiAgentPlanner(IReadOnlyList<Agent> agents, OllamaLLMClient llm)
    {
        _agents = agents;
        _llm = llm;
    }

    public object RouteQuery(string query)
    {
        var agentNames = string.Join(", ", _agents.Select(a => a.Name));

        var routerPrompt = $"""
            You are a router in a multi-agent system.

            Available agents:
            {agentNames}

            Rules:
            WorkflowAgent -> emails, calendar events, tasks
            ResearchAgent -> web search, research, information gathering

            Return ONLY the agent name.
            """;

        var routerResponse = _llm.Chat(new List<ChatMessage>
        {
            new("system", routerPrompt),
            new("user", query)
        });

        var predicted = routerResponse["response"]?.GetValue<string>()?.Trim()
            ?? routerResponse["tool"]?.GetValue<string>()?.Trim()
            ?? string.Empty;

        foreach (var agent in _agents)
        {
            if (predicted.Contains(agent.Name, StringComparison.OrdinalIgnoreCase))
            {
                return agent.Execute(query);
            }
        }

        var lower = query.ToLowerInvariant();

        if (new[] { "email", "mail", "calendar", "meeting", "task" }.Any(k => lower.Contains(k)))
        {
            return _agents[0].Execute(query);
        }

        if (new[] { "search", "find", "research" }.Any(k => lower.Contains(k)))
        {
            return _agents[1].Execute(query);
        }

        return "No suitable agent found.";
    }
}

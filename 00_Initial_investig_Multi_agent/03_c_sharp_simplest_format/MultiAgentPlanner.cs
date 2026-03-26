namespace MultiAgentFramework;

/// <summary>
/// Routes user queries to the appropriate agent using the LLM,
/// with a keyword-based fallback guard.
/// </summary>
public class MultiAgentPlanner
{
    private readonly List<Agent> _agents;
    private readonly OllamaLLMClient _llm;

    public MultiAgentPlanner(List<Agent> agents, OllamaLLMClient llmClient)
    {
        _agents = agents;
        _llm    = llmClient;
    }

    public object RouteQuery(string query)
    {
        var agentNames = _agents.Select(a => a.Name).ToList();

        var routerPrompt = $"""
            You are a router in a multi-agent system.

            Available agents:

            {string.Join(", ", agentNames)}

            Rules:

            WorkflowAgent → emails, calendar events, tasks
            ResearchAgent → web search, research, information gathering

            Return ONLY the agent name.

            Example:
            WorkflowAgent
            """;

        var routerMessages = new List<ChatMessage>
        {
            new("system", routerPrompt),
            new("user",   query),
        };

        var routerResponse = _llm.Chat(routerMessages);

        var predAgent = routerResponse["response"]?.GetValue<string>()?.Trim()
                     ?? routerResponse["tool"]?.GetValue<string>()?.Trim()
                     ?? "";

        // -------------------------------------------------------
        // LLM routing
        // -------------------------------------------------------
        foreach (var agent in _agents)
        {
            if (predAgent.Contains(agent.Name, StringComparison.OrdinalIgnoreCase))
                return agent.Execute(query);
        }

        // -------------------------------------------------------
        // Keyword fallback guard
        // -------------------------------------------------------
        var q = query.ToLower();

        if (new[] { "email", "mail", "calendar", "meeting", "task" }.Any(k => q.Contains(k)))
            return _agents[0].Execute(query);

        if (new[] { "search", "find", "research" }.Any(k => q.Contains(k)))
            return _agents[1].Execute(query);

        return "No suitable agent found.";
    }
}

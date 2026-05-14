from multi_agent_planner import Agent, MultiAgentPlanner
from skills_mcp_servers import EmailSkill, CalendarSkill, TaskSkill, WebSearchSkill
from ollama_llm_client import OllamaLLMClient

def main():
    llm_client = OllamaLLMClient()

    # Define agents with their own skills
    workflow_agent = Agent(
        name="WorkflowAgent",
        skills={
            "send_email": EmailSkill(),
            "create_event": CalendarSkill(),
            "create_task": TaskSkill()
        },
        llm_client=llm_client
    )

    research_agent = Agent(
        name="ResearchAgent",
        skills={"search": WebSearchSkill()},
        llm_client=llm_client
    )

    # Orchestrator that routes queries to the right agent
    planner = MultiAgentPlanner(agents=[workflow_agent, research_agent], llm_client=llm_client)

    # Demo queries
    demo_queries = [
        'Send an email to "hafezbahrami@gmail.com" saying "This is a test"',
        "Schedule a meeting called 'Team Sync' for tomorrow at 2 PM",
        "Create a high priority task to review the quarterly report",
        "Search for the latest AI trends in 2026"
    ]

    for query in demo_queries:
        print("\nQuery:", query)
        response = planner.route_query(query)
        print("Response:", response)

    # Interactive mode
    DO_interactive = False
    if DO_interactive:
        print("\nInteractive Mode. Type 'exit' to quit.")
        while True:
            user_input = input("You: ").strip()
            if user_input.lower() in ["exit", "quit"]:
                break
            response = planner.route_query(user_input)
            print("Planner:", response)


if __name__ == "__main__":
    main()
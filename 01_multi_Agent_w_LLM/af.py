"""
AF: Agentic Framework
Microsoft-Agentic-Framework with Local Ollama/Llama Integration
Works with local Llama models
"""

import os
import json

from typing import Dict, List, Any, Optional
from dataclasses import dataclass
import requests
from ollama_server import OllamaManager
from ollama_llm_client import OllamaLLMClient

from mcp_servers import MCPServer, EmailMCPServer, WebSearchMCPServer, CalendarMCPServer, TaskManagementMCPServer


# ============================================================================
# AGENT
# ============================================================================

class Agent:
    """Agent that uses LLM to understand and execute natural language requests"""
    
    def __init__(self, name: str, description: str, mcp_servers: List[MCPServer], llm_client: OllamaLLMClient):
        self.name = name
        self.description = description
        self.mcp_servers = mcp_servers
        self.llm_client = llm_client
        self.conversation_history = []
    
    def get_all_tools(self) -> List[Dict[str, Any]]:
        """Gather all tool schemas from MCP servers"""
        tools = []
        for server in self.mcp_servers:
            tools.extend(server.get_tool_schema())
        return tools
    
    def execute_tool_call(self, function_name: str, arguments: Dict[str, Any]) -> Any:
        """Execute a tool call on the appropriate MCP server"""
        for server in self.mcp_servers:
            for capability in server.get_capabilities():
                if capability == function_name:
                    return server.execute(capability, arguments)
        return {"error": f"Tool {function_name} not found"}
    
    def process_query(self, user_query: str) -> str:
        """Process a natural language query using LLM reasoning"""
        
        print(f"\n{'='*60}")
        print(f" {self.name} processing query:")
        print(f"Query: {user_query}")
        print(f"{'='*60}")
        
        # Build system message
        system_message = {
            "role": "system",
            "content": f"""You are {self.name}, {self.description}.
                            You have access to various tools through MCP servers. 
                            Analyze the user's request and use the appropriate tools to help them.
                            Be helpful, concise, and action-oriented."""
                        }
        
        # Add user message
        messages = [
            system_message,
            {"role": "user", "content": user_query}
        ]
        
        # Get available tools
        tools = self.get_all_tools()
        
        # Call LLM
        print(f"\n {self.name} thinking with Llama...")
        response = self.llm_client.chat(messages, tools)
        
        # Parse response
        choice = response.get("choices", [{}])[0]
        message = choice.get("message", {})
        
        # Check if LLM wants to call a tool
        tool_calls = message.get("tool_calls", [])
        
        if tool_calls:
            print(f"\n {self.name} using tools...")
            results = []
            
            for tool_call in tool_calls:
                function_name = tool_call.get("function", {}).get("name")
                arguments_str = tool_call.get("function", {}).get("arguments", "{}")
                arguments = json.loads(arguments_str) if isinstance(arguments_str, str) else arguments_str
                
                print(f"\nCalling: {function_name}")
                print(f"Arguments: {json.dumps(arguments, indent=2)}")
                
                result = self.execute_tool_call(function_name, arguments)
                results.append(result)
            
            # Format final response
            final_response = f" Task completed successfully!\n"
            for result in results:
                if isinstance(result, dict):
                    final_response += f"\nResult: {json.dumps(result, indent=2)}"
                else:
                    final_response += f"\nResult: {result}"
            
            return final_response
        else:
            # LLM responded with text only
            content = message.get("content", "I'm not sure how to help with that.")
            return content


# ============================================================================
# AGENTIC FRAMEWORK
# ============================================================================

class AgenticFramework:
    """Framework that routes natural language queries to appropriate agents"""
    
    def __init__(self, llm_client: OllamaLLMClient):
        self.llm_client = llm_client
        self.agents: Dict[str, Agent] = {}
    
    def register_agent(self, agent: Agent):
        """Register an agent with the framework"""
        self.agents[agent.name] = agent
        print(f"✓ Registered agent: {agent.name}")
    
    def route_query(self, user_query: str) -> str:
        """Route query to the most appropriate agent"""
        
        # Simple keyword-based routing
        query_lower = user_query.lower()
        
        if any(word in query_lower for word in ["email", "send", "message"]):
            agent_name = "WorkflowAgent"
        elif any(word in query_lower for word in ["calendar", "event", "meeting", "schedule"]):
            agent_name = "WorkflowAgent"
        elif any(word in query_lower for word in ["task", "todo", "remind"]):
            agent_name = "WorkflowAgent"
        elif any(word in query_lower for word in ["search", "find", "look up", "research"]):
            agent_name = "ResearchAgent"
        else:
            agent_name = list(self.agents.keys())[0] if self.agents else None
        
        if agent_name and agent_name in self.agents:
            return self.agents[agent_name].process_query(user_query)
        else:
            return "No suitable agent found to handle this query."
    
    def interactive_mode(self):
        """Run interactive chat mode"""
        print("\n" + "="*60)
        print("Ollama Agentic Framework - Interactive Mode")
        print("="*60)
        print("Type your queries in natural language.")
        print("Type 'quit' or 'exit' to stop.\n")
        
        while True:
            try:
                user_input = input("\n👤 You: ").strip()
                
                if user_input.lower() in ['quit', 'exit', 'q']:
                    print("\nGoodbye!")
                    break
                
                if not user_input:
                    continue
                
                response = self.route_query(user_input)
                print(f"\n Assistant: {response}")
                
            except KeyboardInterrupt:
                print("\n\n Goodbye!")
                break
            except Exception as e:
                print(f"\n Error: {e}")


# ============================================================================
# MAIN
# ============================================================================

def main():
    print("="*60)
    print("Ollama-Powered Agentic Framework")
    print("="*60)
    
    # Initialize and start Ollama
    ollama_manager = OllamaManager(
        url=os.getenv("OLLAMA_URL", "http://localhost:11434"),
        model=os.getenv("OLLAMA_MODEL", "llama3.2:3b")
    )
    
    try:
        ollama_manager.ensure_running()
        ollama_manager.test_model()
    except Exception as e:
        print(f"Warning: {e}")
        print("Continuing with mock responses...")
    
    # Initialize LLM client
    llm_client = OllamaLLMClient(
        url=ollama_manager.url,
        model=ollama_manager.model
    )
    
    # Initialize framework
    framework = AgenticFramework(llm_client)
    
    # Create agents
    workflow_agent = Agent(
        name="WorkflowAgent",
        description="An agent specialized in workflow automation, communication, and task management",
        mcp_servers=[
            EmailMCPServer(),
            CalendarMCPServer(),
            TaskManagementMCPServer()
        ],
        llm_client=llm_client
    )
    
    research_agent = Agent(
        name="ResearchAgent",
        description="An agent specialized in research and information gathering",
        mcp_servers=[
            WebSearchMCPServer()
        ],
        llm_client=llm_client
    )
    
    # Register agents
    framework.register_agent(workflow_agent)
    framework.register_agent(research_agent)
    
    print("\n" + "="*60)
    print("Running Demo Queries")
    print("="*60)
    
    # Demo queries
    demo_queries = [
        'I need to send an email to "hafezbahrami@gmail.com", and the content of the email should be "hey, Hafez, this is just a test"',
        "Schedule a meeting called 'Team Sync' for tomorrow at 2 PM",
        "Create a high priority task to review the quarterly report",
        "Search for the latest AI trends in 2024"
    ]
    
    for i, query in enumerate(demo_queries, 1):
        print(f"\nDemo Query {i}: {query}")
        response = framework.route_query(query)
        print(f"\nResponse: {response}")
    
    print("\n" + "="*60)
    print("Demo Complete!")
    print("="*60)
    
    # Interactive mode
    print("\nStarting interactive mode...")
    print("You can now type your queries!\n")
    framework.interactive_mode()


if __name__ == "__main__":
    main()

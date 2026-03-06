"""
Enhanced Microsoft-Agentic-Framework with LLM Integration
Now supports natural language queries with real LLM reasoning
"""

import os
import json
from typing import Dict, List, Any, Optional
from dataclasses import dataclass
from enum import Enum
import requests
from dotenv import load_dotenv

# Load environment variables
load_dotenv()


# ============================================================================
# LLM CLIENT
# ============================================================================

class LLMClient:
    """Unified LLM client that can call different LLM providers"""
    
    def __init__(self):
        self.provider = self._detect_provider()
        
    def _detect_provider(self) -> str:
        """Detect which LLM provider to use based on env vars"""
        if os.getenv("AZURE_OPENAI_API_KEY"):
            return "azure"
        elif os.getenv("ASPEN_LLM_SERVER_APIKEY"):
            return "aspen"
        elif os.getenv("OLLAMA_API_URL"):
            return "ollama"
        else:
            return "mock"  # Fallback to mock responses
    
    def chat(self, messages: List[Dict[str, str]], tools: Optional[List[Dict]] = None) -> Dict[str, Any]:
        """Send chat completion request to LLM"""
        
        if self.provider == "azure":
            return self._call_azure(messages, tools)
        elif self.provider == "aspen":
            return self._call_aspen(messages, tools)
        elif self.provider == "ollama":
            return self._call_ollama(messages, tools)
        else:
            return self._mock_response(messages, tools)
    
    def _call_azure(self, messages: List[Dict[str, str]], tools: Optional[List[Dict]] = None) -> Dict[str, Any]:
        """Call Azure OpenAI"""
        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        api_key = os.getenv("AZURE_OPENAI_API_KEY")
        api_version = os.getenv("AZURE_OPENAI_API_VERSION", "2023-05-15")
        deployment = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o")
        
        url = f"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={api_version}"
        
        headers = {
            "Content-Type": "application/json",
            "api-key": api_key
        }
        
        payload = {
            "messages": messages,
            "temperature": 0.7,
            "max_tokens": 1000
        }
        
        if tools:
            payload["tools"] = tools
            payload["tool_choice"] = "auto"
        
        try:
            response = requests.post(url, headers=headers, json=payload, timeout=30)
            response.raise_for_status()
            return response.json()
        except Exception as e:
            print(f"Azure API error: {e}")
            return self._mock_response(messages, tools)
    
    def _call_aspen(self, messages: List[Dict[str, str]], tools: Optional[List[Dict]] = None) -> Dict[str, Any]:
        """Call Aspen LLM Server"""
        url = os.getenv("ASPEN_LLM_SERVER_URL")
        api_key = os.getenv("ASPEN_LLM_SERVER_APIKEY")
        
        headers = {
            "Content-Type": "application/json",
            "Authorization": f"Bearer {api_key}"
        }
        
        payload = {
            "messages": messages,
            "temperature": 0.7,
            "max_tokens": 1000
        }
        
        if tools:
            payload["tools"] = tools
        
        try:
            response = requests.post(url, headers=headers, json=payload, timeout=30)
            response.raise_for_status()
            return response.json()
        except Exception as e:
            print(f"Aspen API error: {e}")
            return self._mock_response(messages, tools)
    
    def _call_ollama(self, messages: List[Dict[str, str]], tools: Optional[List[Dict]] = None) -> Dict[str, Any]:
        """Call Ollama local LLM"""
        url = os.getenv("OLLAMA_API_URL", "http://localhost:11434/api/chat")
        
        payload = {
            "model": "llama2",  # or any model you have installed
            "messages": messages,
            "stream": False
        }
        
        if tools:
            payload["tools"] = tools
        
        try:
            response = requests.post(url, json=payload, timeout=30)
            response.raise_for_status()
            return response.json()
        except Exception as e:
            print(f"Ollama API error: {e}")
            return self._mock_response(messages, tools)
    
    def _mock_response(self, messages: List[Dict[str, str]], tools: Optional[List[Dict]] = None) -> Dict[str, Any]:
        """Generate mock response for testing without real API"""
        user_message = messages[-1]["content"] if messages else ""
        
        # Simple pattern matching for mock responses
        if "email" in user_message.lower():
            if tools:
                # Return a tool call
                return {
                    "choices": [{
                        "message": {
                            "role": "assistant",
                            "content": None,
                            "tool_calls": [{
                                "id": "call_123",
                                "type": "function",
                                "function": {
                                    "name": "send_email",
                                    "arguments": json.dumps({
                                        "to": "hafezbahrami@gmail.com",
                                        "subject": "Test Email",
                                        "body": "hey, Hafez, this is just a test"
                                    })
                                }
                            }]
                        }
                    }]
                }
            else:
                return {
                    "choices": [{
                        "message": {
                            "role": "assistant",
                            "content": "I'll help you send that email to hafezbahrami@gmail.com"
                        }
                    }]
                }
        elif "search" in user_message.lower() or "find" in user_message.lower():
            if tools:
                return {
                    "choices": [{
                        "message": {
                            "role": "assistant",
                            "content": None,
                            "tool_calls": [{
                                "id": "call_124",
                                "type": "function",
                                "function": {
                                    "name": "search",
                                    "arguments": json.dumps({
                                        "query": user_message
                                    })
                                }
                            }]
                        }
                    }]
                }
        
        # Default response
        return {
            "choices": [{
                "message": {
                    "role": "assistant",
                    "content": "I understand. Let me help you with that."
                }
            }]
        }


# ============================================================================
# MCP SERVERS (Enhanced)
# ============================================================================

@dataclass
class MCPServer:
    """Model Context Protocol Server - provides tools/resources to agents"""
    name: str
    description: str
    
    def get_capabilities(self) -> List[str]:
        """Return list of capabilities this server provides"""
        raise NotImplementedError
    
    def get_tool_schema(self) -> List[Dict[str, Any]]:
        """Return OpenAI function/tool schema for LLM"""
        raise NotImplementedError
    
    def execute(self, capability: str, params: Dict[str, Any]) -> Any:
        """Execute a capability with given parameters"""
        raise NotImplementedError


class EmailMCPServer(MCPServer):
    def __init__(self):
        super().__init__(
            name="EmailMCP",
            description="Provides email sending capabilities"
        )
    
    def get_capabilities(self) -> List[str]:
        return ["send_email", "read_inbox", "draft_email"]
    
    def get_tool_schema(self) -> List[Dict[str, Any]]:
        return [{
            "type": "function",
            "function": {
                "name": "send_email",
                "description": "Send an email to a recipient",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "to": {
                            "type": "string",
                            "description": "Email address of the recipient"
                        },
                        "subject": {
                            "type": "string",
                            "description": "Subject line of the email"
                        },
                        "body": {
                            "type": "string",
                            "description": "Body content of the email"
                        }
                    },
                    "required": ["to", "body"]
                }
            }
        }]
    
    def execute(self, capability: str, params: Dict[str, Any]) -> Any:
        if capability == "send_email":
            to = params.get("to", "")
            subject = params.get("subject", "No Subject")
            body = params.get("body", "")
            
            # Simulate sending email
            print(f"\n📧 EMAIL SENT!")
            print(f"To: {to}")
            print(f"Subject: {subject}")
            print(f"Body: {body}")
            print("=" * 60)
            
            return {
                "status": "success",
                "message": f"Email sent to {to}",
                "details": {
                    "to": to,
                    "subject": subject,
                    "body_preview": body[:50] + "..." if len(body) > 50 else body
                }
            }
        elif capability == "read_inbox":
            return ["Email 1", "Email 2", "Email 3"]
        elif capability == "draft_email":
            return "Draft created successfully"
        return None


class WebSearchMCPServer(MCPServer):
    def __init__(self):
        super().__init__(
            name="WebSearchMCP",
            description="Provides web search capabilities"
        )
    
    def get_capabilities(self) -> List[str]:
        return ["search", "fetch_url"]
    
    def get_tool_schema(self) -> List[Dict[str, Any]]:
        return [{
            "type": "function",
            "function": {
                "name": "search",
                "description": "Search the web for information",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "query": {
                            "type": "string",
                            "description": "Search query"
                        }
                    },
                    "required": ["query"]
                }
            }
        }]
    
    def execute(self, capability: str, params: Dict[str, Any]) -> Any:
        if capability == "search":
            query = params.get("query", "")
            print(f"\n🔍 WEB SEARCH EXECUTED!")
            print(f"Query: {query}")
            print("=" * 60)
            return {
                "status": "success",
                "results": [
                    f"Result 1 for '{query}'",
                    f"Result 2 for '{query}'",
                    f"Result 3 for '{query}'"
                ]
            }
        elif capability == "fetch_url":
            url = params.get("url", "")
            return f"Content from: {url}"
        return None


class CalendarMCPServer(MCPServer):
    def __init__(self):
        super().__init__(
            name="CalendarMCP",
            description="Provides calendar management"
        )
    
    def get_capabilities(self) -> List[str]:
        return ["create_event", "list_events", "update_event"]
    
    def get_tool_schema(self) -> List[Dict[str, Any]]:
        return [{
            "type": "function",
            "function": {
                "name": "create_event",
                "description": "Create a calendar event",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "title": {
                            "type": "string",
                            "description": "Event title"
                        },
                        "date": {
                            "type": "string",
                            "description": "Event date (YYYY-MM-DD)"
                        },
                        "time": {
                            "type": "string",
                            "description": "Event time (HH:MM)"
                        }
                    },
                    "required": ["title", "date"]
                }
            }
        }]
    
    def execute(self, capability: str, params: Dict[str, Any]) -> Any:
        if capability == "create_event":
            title = params.get("title", "")
            date = params.get("date", "")
            time = params.get("time", "")
            
            print(f"\n📅 CALENDAR EVENT CREATED!")
            print(f"Title: {title}")
            print(f"Date: {date}")
            print(f"Time: {time}")
            print("=" * 60)
            
            return {
                "status": "success",
                "event": {
                    "title": title,
                    "date": date,
                    "time": time
                }
            }
        return None


class TaskManagementMCPServer(MCPServer):
    def __init__(self):
        super().__init__(
            name="TaskManagementMCP",
            description="Provides task tracking and management"
        )
    
    def get_capabilities(self) -> List[str]:
        return ["create_task", "update_status", "list_tasks"]
    
    def get_tool_schema(self) -> List[Dict[str, Any]]:
        return [{
            "type": "function",
            "function": {
                "name": "create_task",
                "description": "Create a new task",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "task": {
                            "type": "string",
                            "description": "Task description"
                        },
                        "priority": {
                            "type": "string",
                            "enum": ["low", "medium", "high"],
                            "description": "Task priority"
                        }
                    },
                    "required": ["task"]
                }
            }
        }]
    
    def execute(self, capability: str, params: Dict[str, Any]) -> Any:
        if capability == "create_task":
            task = params.get("task", "")
            priority = params.get("priority", "medium")
            
            print(f"\n✅ TASK CREATED!")
            print(f"Task: {task}")
            print(f"Priority: {priority}")
            print("=" * 60)
            
            return {
                "status": "success",
                "task": {
                    "description": task,
                    "priority": priority
                }
            }
        return None


# ============================================================================
# ENHANCED AGENT WITH LLM REASONING
# ============================================================================

class IntelligentAgent:
    """Agent that uses LLM to understand and execute natural language requests"""
    
    def __init__(self, name: str, description: str, mcp_servers: List[MCPServer], llm_client: LLMClient):
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
        print(f"🤖 {self.name} processing query:")
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
        print(f"\n💭 {self.name} thinking...")
        response = self.llm_client.chat(messages, tools)
        
        # Parse response
        choice = response.get("choices", [{}])[0]
        message = choice.get("message", {})
        
        # Check if LLM wants to call a tool
        tool_calls = message.get("tool_calls", [])
        
        if tool_calls:
            print(f"\n🔧 {self.name} using tools...")
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
            final_response = f"✅ Task completed successfully!\n"
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
# ENHANCED AGENTIC FRAMEWORK
# ============================================================================

class EnhancedAgenticFramework:
    """Framework that routes natural language queries to appropriate agents"""
    
    def __init__(self, llm_client: LLMClient):
        self.llm_client = llm_client
        self.agents: Dict[str, IntelligentAgent] = {}
    
    def register_agent(self, agent: IntelligentAgent):
        """Register an agent with the framework"""
        self.agents[agent.name] = agent
        print(f"✓ Registered agent: {agent.name}")
    
    def route_query(self, user_query: str) -> str:
        """Route query to the most appropriate agent"""
        
        # Simple keyword-based routing (can be enhanced with LLM-based routing)
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
            # Default to first available agent
            agent_name = list(self.agents.keys())[0] if self.agents else None
        
        if agent_name and agent_name in self.agents:
            return self.agents[agent_name].process_query(user_query)
        else:
            return "No suitable agent found to handle this query."
    
    def interactive_mode(self):
        """Run interactive chat mode"""
        print("\n" + "="*60)
        print("🚀 Enhanced Agentic Framework - Interactive Mode")
        print("="*60)
        print("Type your queries in natural language.")
        print("Type 'quit' or 'exit' to stop.\n")
        
        while True:
            try:
                user_input = input("\n👤 You: ").strip()
                
                if user_input.lower() in ['quit', 'exit', 'q']:
                    print("\n👋 Goodbye!")
                    break
                
                if not user_input:
                    continue
                
                response = self.route_query(user_input)
                print(f"\n🤖 Assistant: {response}")
                
            except KeyboardInterrupt:
                print("\n\n👋 Goodbye!")
                break
            except Exception as e:
                print(f"\n❌ Error: {e}")


# ============================================================================
# MAIN DEMO
# ============================================================================

def main():
    print("="*60)
    print("🚀 Enhanced Microsoft-Agentic-Framework with LLM")
    print("="*60)
    
    # Initialize LLM client
    llm_client = LLMClient()
    print(f"LLM Provider: {llm_client.provider}")
    
    # Initialize framework
    framework = EnhancedAgenticFramework(llm_client)
    
    # Create WorkflowAgent with email, calendar, and task management
    workflow_agent = IntelligentAgent(
        name="WorkflowAgent",
        description="An agent specialized in workflow automation, communication, and task management",
        mcp_servers=[
            EmailMCPServer(),
            CalendarMCPServer(),
            TaskManagementMCPServer()
        ],
        llm_client=llm_client
    )
    
    # Create ResearchAgent with web search
    research_agent = IntelligentAgent(
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
    print("🧪 Running Demo Queries")
    print("="*60)
    
    # Demo query 1: Send email
    demo_query_1 = 'I need to send an email to "hafezbahrami@gmail.com", and the content of the email should be "hey, Hafez, this is just a test"'
    print(f"\n📝 Demo Query 1: {demo_query_1}")
    response = framework.route_query(demo_query_1)
    print(f"\n✅ Response: {response}")
    
    # Demo query 2: Create calendar event
    demo_query_2 = "Schedule a meeting called 'Team Sync' for tomorrow at 2 PM"
    print(f"\n📝 Demo Query 2: {demo_query_2}")
    response = framework.route_query(demo_query_2)
    print(f"\n✅ Response: {response}")
    
    # Demo query 3: Create task
    demo_query_3 = "Create a high priority task to review the quarterly report"
    print(f"\n📝 Demo Query 3: {demo_query_3}")
    response = framework.route_query(demo_query_3)
    print(f"\n✅ Response: {response}")
    
    # Demo query 4: Web search
    demo_query_4 = "Search for the latest AI trends in 2024"
    print(f"\n📝 Demo Query 4: {demo_query_4}")
    response = framework.route_query(demo_query_4)
    print(f"\n✅ Response: {response}")
    
    print("\n" + "="*60)
    print("✅ Demo Complete!")
    print("="*60)
    
    # Optional: Start interactive mode
    print("\n💡 Starting interactive mode...")
    print("You can now type your queries!\n")
    framework.interactive_mode()


if __name__ == "__main__":
    main()

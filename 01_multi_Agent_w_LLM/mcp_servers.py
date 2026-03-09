
from typing import Dict, List, Any
from dataclasses import dataclass


# ============================================================================
# MCP SERVERS (Same as before)
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
        """
        Return OpenAI function/tool schema for LLM. It means This function describes tools to the LLM.
        The LLM must know: (a) tool name, (b) tool description, (c) tool parameters, (d) parameter types.
"""
        raise NotImplementedError
    
    def execute(self, capability: str, params: Dict[str, Any]) -> Any:
        """execute() is the actual implementation of the tool above."""
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
            print(f"\nEMAIL SENT!")
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
            print(f"\n WEB SEARCH EXECUTED!")
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
            
            print(f"\n CALENDAR EVENT CREATED!")
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
            
            print(f"\n TASK CREATED!")
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


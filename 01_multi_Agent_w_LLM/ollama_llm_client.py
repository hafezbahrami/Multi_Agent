import json

from typing import Dict, List, Any, Optional
from dataclasses import dataclass
import requests
from ollama_server import OllamaManager

from dotenv import load_dotenv
import os
load_dotenv()
OLLAMA_URL = os.getenv("OLLAMA_URL", "http://localhost:11434")
MODEL_NAME = os.getenv("OLLAMA_MODEL", "llama3.2:3b")


# ============================================================================
# OLLAMA LLM CLIENT
# ============================================================================

class OllamaLLMClient:
    """LLM client specifically for Ollama with function calling support"""
    
    def __init__(self, url: str = OLLAMA_URL, model: str = MODEL_NAME):
        self.url = url
        self.model = model
        self.provider = "ollama"
    
    def chat(self, messages: List[Dict[str, str]], tools: Optional[List[Dict]] = None) -> Dict[str, Any]:
        """Send chat completion request to Ollama with tool calling support"""
        
        # Convert messages to a single prompt for Ollama
        prompt = self._build_prompt(messages, tools)
        
        try:
            response = requests.post(
                f"{self.url}/api/generate",
                json={
                    "model": self.model,
                    "prompt": prompt,
                    "stream": False,
                    "temperature": 0.7
                },
                timeout=30
            )
            response.raise_for_status()
            result = response.json()
            
            # Parse the response to detect tool calls
            response_text = result.get("response", "")
            
            return self._parse_response(response_text, tools)
            
        except Exception as e:
            print(f"Ollama API error: {e}")
            # Fallback to mock response
            return self._mock_response(messages, tools)
    
    def _build_prompt(self, messages: List[Dict[str, str]], tools: Optional[List[Dict]] = None) -> str:
        """Build a prompt from messages and tools for Ollama"""
        
        prompt_parts = []
        
        # Add system message if present
        for msg in messages:
            if msg["role"] == "system":
                prompt_parts.append(f"SYSTEM: {msg['content']}\n")
            elif msg["role"] == "user":
                prompt_parts.append(f"USER: {msg['content']}\n")
            elif msg["role"] == "assistant":
                prompt_parts.append(f"ASSISTANT: {msg['content']}\n")
        
        # Add tools information
        if tools:
            prompt_parts.append("\nAVAILABLE TOOLS:\n")
            for tool in tools:
                func = tool.get("function", {})
                prompt_parts.append(f"- {func.get('name')}: {func.get('description')}\n")
                params = func.get("parameters", {}).get("properties", {})
                if params:
                    prompt_parts.append(f"  Parameters: {', '.join(params.keys())}\n")
            
            prompt_parts.append("\nTo use a tool, respond with JSON in this format:\n")
            prompt_parts.append('{"tool": "tool_name", "parameters": {"param1": "value1"}}\n')
            prompt_parts.append("\nASSISTANT: ")
        else:
            prompt_parts.append("\nASSISTANT: ")
        
        return "".join(prompt_parts)
    
    def _parse_response(self, response_text: str, tools: Optional[List[Dict]] = None) -> Dict[str, Any]:
        """Parse Ollama response and detect tool calls"""
        
        # Try to extract JSON for tool calls
        if tools and "{" in response_text and "}" in response_text:
            try:
                # Find JSON in response
                start = response_text.find("{")
                end = response_text.rfind("}") + 1
                json_str = response_text[start:end]
                tool_call_data = json.loads(json_str)
                
                if "tool" in tool_call_data or "function" in tool_call_data:
                    # Extract tool name and parameters
                    tool_name = tool_call_data.get("tool") or tool_call_data.get("function")
                    params = tool_call_data.get("parameters") or tool_call_data.get("arguments", {})
                    
                    return {
                        "choices": [{
                            "message": {
                                "role": "assistant",
                                "content": None,
                                "tool_calls": [{
                                    "id": "call_ollama",
                                    "type": "function",
                                    "function": {
                                        "name": tool_name,
                                        "arguments": json.dumps(params)
                                    }
                                }]
                            }
                        }]
                    }
            except json.JSONDecodeError:
                pass  # Not a valid tool call, treat as regular text
        
        # Regular text response
        return {
            "choices": [{
                "message": {
                    "role": "assistant",
                    "content": response_text
                }
            }]
        }
    
    def _mock_response(self, messages: List[Dict[str, str]], tools: Optional[List[Dict]] = None) -> Dict[str, Any]:
        """Generate mock response for testing"""
        user_message = messages[-1]["content"] if messages else ""
        
        # Simple pattern matching for mock responses
        if "email" in user_message.lower():
            if tools:
                # Extract email and content from user message
                import re
                email_match = re.search(r'[\w\.-]+@[\w\.-]+', user_message)
                email = email_match.group(0) if email_match else "test@example.com"
                
                # Try to extract quoted content
                content_match = re.search(r'"([^"]*)"', user_message)
                content = content_match.group(1) if content_match else "Test message"
                
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
                                        "to": email,
                                        "subject": "Message from Agent",
                                        "body": content
                                    })
                                }
                            }]
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
        elif "calendar" in user_message.lower() or "meeting" in user_message.lower() or "schedule" in user_message.lower():
            if tools:
                import re
                # Extract meeting details
                title_match = re.search(r'called ["\']([^"\']*)["\']', user_message)
                title = title_match.group(1) if title_match else "Meeting"
                
                return {
                    "choices": [{
                        "message": {
                            "role": "assistant",
                            "content": None,
                            "tool_calls": [{
                                "id": "call_125",
                                "type": "function",
                                "function": {
                                    "name": "create_event",
                                    "arguments": json.dumps({
                                        "title": title,
                                        "date": "2024-03-20",
                                        "time": "14:00"
                                    })
                                }
                            }]
                        }
                    }]
                }
        elif "task" in user_message.lower() or "todo" in user_message.lower():
            if tools:
                # Extract task description
                task_text = user_message
                for prefix in ["create a task", "add a task", "task:", "todo:"]:
                    if prefix in task_text.lower():
                        task_text = task_text[task_text.lower().find(prefix) + len(prefix):].strip()
                
                priority = "high" if "high priority" in user_message.lower() else "medium"
                
                return {
                    "choices": [{
                        "message": {
                            "role": "assistant",
                            "content": None,
                            "tool_calls": [{
                                "id": "call_126",
                                "type": "function",
                                "function": {
                                    "name": "create_task",
                                    "arguments": json.dumps({
                                        "task": task_text,
                                        "priority": priority
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


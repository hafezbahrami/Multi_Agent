# Enhanced Microsoft-Agentic-Framework with LLM Integration

## 🚀 What's New

This is an upgraded version of the basic framework that now includes:

- **LLM Integration**: Agents use real language models to understand natural language queries
- **Natural Language Interface**: Just type what you want in plain English
- **Tool Calling**: Agents intelligently choose and execute the right tools
- **Multi-Provider Support**: Works with Azure OpenAI, Aspen LLM, Ollama, or mock mode
- **Interactive Mode**: Chat with your agents in real-time

## 📋 Prerequisites

```bash
pip install -r requirements.txt
```

## 🔑 Setup

1. **Copy the example environment file:**
```bash
cp .env.example .env
```

2. **Configure your LLM provider** in `.env`:

### Option A: Azure OpenAI (Recommended)
```bash
AZURE_OPENAI_ENDPOINT=https://your-endpoint.openai.azure.com
AZURE_OPENAI_API_KEY=your-api-key
AZURE_OPENAI_API_VERSION=2023-05-15
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o
```

### Option B: Ollama (Local)
```bash
OLLAMA_API_URL=http://localhost:11434/api/chat
```

### Option C: Aspen LLM Server
```bash
ASPEN_LLM_SERVER_URL=https://your-server.com/v1/chat/completions
ASPEN_LLM_SERVER_APIKEY=your-api-key
```

### Option D: Mock Mode (No API needed)
Leave all API keys empty - the system will use mock responses for testing

## 🎯 Usage

### Run Demo Queries
```bash
python enhanced_agent_framework.py
```

This will run 4 demo queries:
1. Send an email
2. Schedule a meeting
3. Create a task
4. Search the web

### Interactive Mode

After the demo, the framework enters interactive mode where you can type your own queries:

```
👤 You: I need to send an email to "hafezbahrami@gmail.com" with subject "Meeting Tomorrow" and say "Let's meet at 3 PM"

🤖 Assistant: ✅ Task completed successfully!
📧 EMAIL SENT!
To: hafezbahrami@gmail.com
Subject: Meeting Tomorrow
Body: Let's meet at 3 PM
```

## 💬 Example Queries

### Email Tasks
```
- Send an email to john@example.com saying "Project completed"
- I need to email the team about tomorrow's meeting
- Draft an email to hafezbahrami@gmail.com with test content
```

### Calendar Tasks
```
- Schedule a meeting called "Team Sync" for tomorrow at 2 PM
- Create a calendar event for "Doctor Appointment" on 2024-03-15
- Add "Product Launch" to my calendar on Friday
```

### Task Management
```
- Create a high priority task to review the quarterly report
- Add a task: finish the presentation
- Remind me to call the client
```

### Research Tasks
```
- Search for the latest AI trends in 2024
- Find information about quantum computing
- Look up recent news about SpaceX
```

## 🏗️ Architecture

```
User Query (Natural Language)
       ↓
Enhanced Agentic Framework
       ↓
[Query Router] → Selects appropriate agent
       ↓
┌──────────────┴──────────────┐
│                              │
WorkflowAgent          ResearchAgent
│                              │
├─ EmailMCP               WebSearchMCP
├─ CalendarMCP
└─ TaskManagementMCP
       ↓                       ↓
LLM Reasoning (Azure/Ollama/Aspen)
       ↓                       ↓
Tool Execution (MCP Servers)
       ↓                       ↓
    Results → User
```

## 🤖 Agents

### WorkflowAgent
Handles communication, scheduling, and task management

**MCP Servers:**
- **EmailMCP**: Send emails, read inbox, draft messages
- **CalendarMCP**: Create events, list schedule, update events
- **TaskManagementMCP**: Create tasks, update status, list todos

### ResearchAgent
Handles information gathering and research

**MCP Servers:**
- **WebSearchMCP**: Search web, fetch URLs

## 🔧 How It Works

1. **User Input**: You type a natural language query
2. **Routing**: Framework analyzes keywords and routes to appropriate agent
3. **LLM Reasoning**: Agent uses LLM to understand intent and plan actions
4. **Tool Selection**: LLM decides which MCP server tools to use
5. **Execution**: Selected tools execute on MCP servers
6. **Response**: Results are formatted and returned to user

## 🎨 Key Features

### LLM Client
- Auto-detects available LLM provider from environment
- Supports Azure OpenAI, Aspen, Ollama
- Falls back to mock mode for testing without API

### Tool Calling
- MCP servers expose tools as OpenAI function schemas
- LLM intelligently selects and calls appropriate tools
- Parameters are automatically extracted and validated

### Conversation Management
- Maintains conversation history
- System prompts guide agent behavior
- Context-aware responses

## 📝 Adding New Capabilities

### Add a new MCP Server:

```python
class SlackMCPServer(MCPServer):
    def __init__(self):
        super().__init__(
            name="SlackMCP",
            description="Provides Slack messaging capabilities"
        )
    
    def get_capabilities(self) -> List[str]:
        return ["send_message", "read_channel"]
    
    def get_tool_schema(self) -> List[Dict[str, Any]]:
        return [{
            "type": "function",
            "function": {
                "name": "send_message",
                "description": "Send a Slack message",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "channel": {"type": "string"},
                        "message": {"type": "string"}
                    },
                    "required": ["channel", "message"]
                }
            }
        }]
    
    def execute(self, capability: str, params: Dict[str, Any]) -> Any:
        if capability == "send_message":
            # Your Slack API logic here
            return {"status": "success"}
```

### Add it to an agent:

```python
workflow_agent = IntelligentAgent(
    name="WorkflowAgent",
    description="...",
    mcp_servers=[
        EmailMCPServer(),
        CalendarMCPServer(),
        TaskManagementMCPServer(),
        SlackMCPServer()  # New!
    ],
    llm_client=llm_client
)
```

## 🐛 Troubleshooting

### "No module named 'requests'"
```bash
pip install -r requirements.txt
```

### "LLM Provider: mock"
Your API keys aren't configured. Check your `.env` file.

### Azure OpenAI Errors
- Verify your endpoint URL is correct
- Check API key is valid
- Ensure deployment name matches your Azure deployment

### Ollama Connection Errors
- Make sure Ollama is running: `ollama serve`
- Check if the model is installed: `ollama list`

## 📊 Comparison: Basic vs Enhanced

| Feature | Basic Framework | Enhanced Framework |
|---------|----------------|-------------------|
| User Input | Python function calls | Natural language |
| Agent Intelligence | Static routing | LLM reasoning |
| Tool Selection | Manual | Automatic via LLM |
| Extensibility | Manual coding | Add tools as schemas |
| User Experience | Technical | Conversational |

## 🎓 Next Steps

1. **Add Real API Integrations**: Replace mock executions with real API calls
2. **Add More Agents**: Create specialized agents for different domains
3. **Improve Routing**: Use LLM-based routing instead of keyword matching
4. **Add Memory**: Implement persistent conversation history
5. **Add Multi-turn Conversations**: Support follow-up questions
6. **Add Logging**: Track all agent actions and decisions
7. **Add Error Handling**: Robust error recovery and retries

## 📄 License

MIT

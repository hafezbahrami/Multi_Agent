# Quick Start Guide

## 🎯 Get Running in 3 Steps

### Step 1: Install Dependencies
```bash
pip install requests python-dotenv
```

### Step 2: Choose Your Mode

#### A. Quick Test (No API Key Needed)
Just run it! The framework will use mock mode:
```bash
python enhanced_agent_framework.py
```

#### B. Use Real Azure OpenAI
1. Create a `.env` file:
```bash
AZURE_OPENAI_ENDPOINT=https://your-endpoint.openai.azure.com
AZURE_OPENAI_API_KEY=your-api-key-here
AZURE_OPENAI_API_VERSION=2023-05-15
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o
```

2. Run:
```bash
python enhanced_agent_framework.py
```

#### C. Use Local Ollama
1. Install and start Ollama:
```bash
ollama serve
ollama pull llama2
```

2. Create `.env`:
```bash
OLLAMA_API_URL=http://localhost:11434/api/chat
```

3. Run:
```bash
python enhanced_agent_framework.py
```

### Step 3: Try Natural Language Queries

After the demo completes, type your queries:

```
👤 You: Send an email to test@example.com saying "Hello World"
👤 You: Schedule a meeting tomorrow at 3 PM
👤 You: Create a task to review the code
👤 You: Search for Python tutorials
```

Type `quit` to exit.

## 📝 Example Queries You Can Try

### Emails
```
- Send an email to hafezbahrami@gmail.com with subject "Update" and body "All systems operational"
- I need to email john@company.com about the project status
- Draft an email to the team
```

### Calendar
```
- Schedule a meeting called "Sprint Planning" for March 15 at 10 AM
- Create an event "Lunch with Client" tomorrow at noon
- Add "Code Review" to my calendar Friday afternoon
```

### Tasks
```
- Create a high priority task: finish documentation
- Add task "Call supplier" with medium priority
- Make a todo item to review pull requests
```

### Research
```
- Search for latest news about AI
- Find information on Python async programming
- Look up quantum computing basics
```

## 🔍 What's Happening Behind the Scenes?

1. **You type**: "Send email to hafezbahrami@gmail.com"
2. **Framework routes**: Keywords detected → WorkflowAgent selected
3. **LLM thinks**: Analyzes query → Identifies need for `send_email` tool
4. **Tool executes**: EmailMCP.send_email() is called
5. **You see**: Confirmation message with details

## 🎨 Customization

### Add Your Own MCP Server

```python
# 1. Create the server class
class YourMCPServer(MCPServer):
    def __init__(self):
        super().__init__(name="YourMCP", description="Does cool stuff")
    
    def get_tool_schema(self):
        return [{
            "type": "function",
            "function": {
                "name": "your_action",
                "description": "What this does",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "param1": {"type": "string", "description": "..."}
                    },
                    "required": ["param1"]
                }
            }
        }]
    
    def execute(self, capability, params):
        # Your logic here
        return {"status": "success"}

# 2. Add to agent
workflow_agent = IntelligentAgent(
    name="WorkflowAgent",
    description="...",
    mcp_servers=[
        EmailMCPServer(),
        YourMCPServer()  # Add here!
    ],
    llm_client=llm_client
)
```

## 🐛 Common Issues

**Q: Getting "mock" as LLM provider?**  
A: Your API keys aren't set. Either add them to `.env` or test with mock mode.

**Q: Tool not being called?**  
A: Make sure your query uses keywords related to the tool (email, calendar, task, search).

**Q: Azure OpenAI error?**  
A: Check endpoint URL, API key, and deployment name in `.env`.

## 📚 What to Read Next

- `README_ENHANCED.md` - Full documentation
- `enhanced_agent_framework.py` - Source code with comments
- `.env.example` - All configuration options

## 🚀 Ready?

```bash
python enhanced_agent_framework.py
```

Have fun! 🎉

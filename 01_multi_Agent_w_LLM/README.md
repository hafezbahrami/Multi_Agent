# 🦙 Ollama-Powered Multi-Agent Framework

**Complete Guide - Everything You Need in One Place**

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [What You Get](#what-you-get)
3. [Quick Start (3 Steps)](#quick-start)
4. [Architecture](#architecture)
5. [Complete Setup Guide](#complete-setup-guide)
6. [Usage Examples](#usage-examples)
7. [Agents & MCP Servers](#agents--mcp-servers)
8. [How It Works](#how-it-works)
9. [Configuration](#configuration)
10. [Troubleshooting](#troubleshooting)
11. [Visualization Diagrams](#visualization-diagrams)
12. [Customization & Extension](#customization--extension)
13. [Advanced Topics](#advanced-topics)
14. [FAQ](#faq)
15. [Reference](#reference)

---

## Overview

### What Is This?

A **production-ready multi-agent framework** that uses your **local Llama 3.2 model** through Ollama. 

**Key Features:**
- ✅ **NO API Keys Required** - 100% local, completely free
- ✅ **Natural Language Interface** - Just type what you want
- ✅ **Multi-Agent System** - Specialized agents for different tasks
- ✅ **MCP Protocol** - Extensible tool architecture
- ✅ **Auto Ollama Management** - Handles server startup automatically
- ✅ **Privacy First** - Your data never leaves your machine

### The Problem It Solves

Instead of writing code to interact with AI, just type:
```
"Send an email to hafezbahrami@gmail.com saying 'hey, Hafez, this is just a test'"
```

The framework:
1. Routes your query to the right agent
2. Uses Llama to understand your intent
3. Calls the appropriate tool (email, calendar, task, search)
4. Returns the result

---

## What You Get

### 📦 Core Files

```
ollama_agent_framework.py    # Main framework (USE THIS)
test_ollama.py                # Test your setup
.env                          # Configuration
```

### 📚 Documentation

```
README.md                     # This complete guide
PROJECT_SUMMARY.md            # Project overview
SETUP.md                      # Step-by-step setup
VISUALIZATION_GUIDE.md        # How to read diagrams
```

### 📊 Diagrams

```
ollama-architecture-detailed.mermaid    # System overview
ollama-execution-flow.mermaid           # Query flow
ollama-components.mermaid               # Class structure
ollama-sequence.mermaid                 # Interaction timeline
agent-framework-architecture.excalidraw # Visual overview
```

### 🔄 Alternative Versions

```
enhanced_agent_framework.py   # Cloud API version (Azure/etc)
agent_framework.py            # Basic conceptual version
```

---

## Quick Start

### Prerequisites

- ✅ Ollama installed with `llama3.2:3b` model
- ✅ Python 3.8+
- ✅ `requests` library

### Step 1: Install Dependencies

```bash
pip install requests
```

### Step 2: Verify Setup

```bash
python test_ollama.py
```

Expected output:
```
✅ Ollama server running
✅ Model 'llama3.2:3b' found!
✅ Text generation working!
✅ ALL TESTS PASSED!
```

### Step 3: Run the Framework

```bash
python ollama_agent_framework.py
```

The framework will:
1. ✅ Check/start Ollama server
2. ✅ Test your model
3. ✅ Run 4 demo queries
4. ✅ Enter interactive mode

### That's It! 🎉

You're now ready to use natural language to control agents!

---

## Architecture

### High-Level Overview

```
┌─────────────────────────────────────────────┐
│           User (Natural Language)           │
└──────────────────┬──────────────────────────┘
                   ↓
┌──────────────────────────────────────────────┐
│      Ollama Agentic Framework                │
│  • Query Router                              │
│  • Ollama Manager (auto-start server)       │
│  • LLM Client (talks to Llama)              │
└──────────────┬───────────────────────────────┘
               ↓
    ┌──────────┴──────────┐
    ↓                      ↓
┌─────────────┐    ┌─────────────┐
│WorkflowAgent│    │ResearchAgent│
├─────────────┤    ├─────────────┤
│• EmailMCP   │    │• SearchMCP  │
│• CalendarMCP│    └─────────────┘
│• TaskMCP    │
└─────────────┘
       ↓
┌──────────────────────────────────────────────┐
│      Ollama Server (localhost:11434)         │
│            Llama 3.2 3B Model                │
└──────────────────────────────────────────────┘
```

### System Components

**1. User Interface Layer**
- Natural language queries
- Interactive chat mode
- Response display

**2. Framework Layer**
- **Query Router**: Keyword-based agent selection
- **Ollama Manager**: Server lifecycle management
- **LLM Client**: Communication with Ollama

**3. Agent Layer**
- **WorkflowAgent**: Handles emails, calendar, tasks
- **ResearchAgent**: Handles information gathering

**4. MCP Server Layer**
- **EmailMCP**: Send/read emails
- **CalendarMCP**: Manage events
- **TaskManagementMCP**: Create/track tasks
- **WebSearchMCP**: Search information

**5. Execution Layer**
- Ollama Server (local)
- Llama 3.2 Model (local)

---

## Complete Setup Guide

### Part 1: Verify Ollama Installation

#### Check Ollama Version
```bash
ollama --version
```

If not installed:
- **macOS**: `brew install ollama`
- **Linux**: `curl -fsSL https://ollama.com/install.sh | sh`
- **Windows**: Download from https://ollama.com/download

#### Check Model
```bash
ollama list
```

You should see:
```
NAME              ID              SIZE      MODIFIED
llama3.2:3b       a80c4f17acd5    2.0 GB    2 days ago
```

If missing:
```bash
ollama pull llama3.2:3b
```

Your model blobs are stored in:
```
/usr/share/ollama/.ollama/models/blobs/
```

### Part 2: Python Setup

#### Install Python Dependencies
```bash
pip install requests
```

#### Verify Installation
```bash
python -c "import requests; print('✅ Requests installed')"
```

### Part 3: Test Your Setup

#### Run Test Script
```bash
python test_ollama.py
```

The script will:
1. ✅ Check if Ollama is running
2. ✅ Start it if needed
3. ✅ Verify model availability
4. ✅ Test text generation

#### Expected Output
```
============================================================
🦙 Ollama Setup Test
============================================================

📍 Step 1: Checking Ollama server...
✅ Ollama server already running

📍 Step 2: Checking for model...
📦 Available models: llama3.2:3b
✅ Model 'llama3.2:3b' found!

📍 Step 3: Testing text generation...
🧪 Testing text generation with llama3.2:3b...

💬 Prompt: Explain reinforcement learning in 2 sentences.

🤖 Model Output:
Reinforcement learning is a type of machine learning where an agent 
learns to make decisions by interacting with an environment and 
receiving rewards or penalties. The goal is to maximize cumulative 
reward over time.

✅ Text generation working!

============================================================
✅ ALL TESTS PASSED!
============================================================
```

### Part 4: Run the Framework

```bash
python ollama_agent_framework.py
```

#### What Happens:
1. Framework initializes
2. Ollama server check/start
3. Model verification
4. Demo queries execute:
   - Send email
   - Schedule meeting
   - Create task
   - Web search
5. Interactive mode starts

### Part 5: Verification Checklist

Before proceeding, ensure:

- [ ] Ollama installed (`ollama --version` works)
- [ ] Model downloaded (`ollama list` shows llama3.2:3b)
- [ ] Python has requests (`pip list | grep requests`)
- [ ] Test script passes (`python test_ollama.py`)
- [ ] Framework runs (`python ollama_agent_framework.py`)

If all checked ✅, you're fully set up!

---

## Usage Examples

### Interactive Mode

After running the framework, you'll see:

```
============================================================
🚀 Ollama Agentic Framework - Interactive Mode
============================================================
Type your queries in natural language.
Type 'quit' or 'exit' to stop.

👤 You: 
```

### Example Queries

#### 1. Email Tasks

**Query:**
```
Send an email to hafezbahrami@gmail.com saying "hey, Hafez, this is just a test"
```

**Output:**
```
🤖 WorkflowAgent processing query...
💭 WorkflowAgent thinking with Llama...
🔧 WorkflowAgent using tools...

📧 EMAIL SENT!
To: hafezbahrami@gmail.com
Subject: Message from Agent
Body: hey, Hafez, this is just a test
============================================================

✅ Task completed successfully!
```

**More examples:**
```
- Email john@company.com about the project deadline
- Send an email to the team with the quarterly results
- Draft an email to support@example.com reporting a bug
```

#### 2. Calendar Tasks

**Query:**
```
Schedule a meeting called "Team Sync" for tomorrow at 2 PM
```

**Output:**
```
🤖 WorkflowAgent processing query...
💭 WorkflowAgent thinking with Llama...
🔧 WorkflowAgent using tools...

📅 CALENDAR EVENT CREATED!
Title: Team Sync
Date: 2024-03-20
Time: 14:00
============================================================

✅ Task completed successfully!
```

**More examples:**
```
- Create a calendar event for "Product Launch" on Friday
- Schedule "Doctor Appointment" for March 15 at 10 AM
- Add "Code Review Session" to my calendar tomorrow
```

#### 3. Task Management

**Query:**
```
Create a high priority task to review the quarterly report
```

**Output:**
```
🤖 WorkflowAgent processing query...
💭 WorkflowAgent thinking with Llama...
🔧 WorkflowAgent using tools...

✅ TASK CREATED!
Task: review the quarterly report
Priority: high
============================================================

✅ Task completed successfully!
```

**More examples:**
```
- Add a task to finish the presentation
- Create a medium priority task: call the supplier
- Make a todo item to update the documentation
```

#### 4. Research Tasks

**Query:**
```
Search for the latest AI trends in 2024
```

**Output:**
```
🤖 ResearchAgent processing query...
💭 ResearchAgent thinking with Llama...
🔧 ResearchAgent using tools...

🔍 WEB SEARCH EXECUTED!
Query: latest AI trends in 2024
============================================================

✅ Task completed successfully!
Result: {
  "status": "success",
  "results": [
    "Result 1 for 'latest AI trends in 2024'",
    "Result 2 for 'latest AI trends in 2024'",
    "Result 3 for 'latest AI trends in 2024'"
  ]
}
```

**More examples:**
```
- Find information about Python async programming
- Search for quantum computing basics
- Look up recent news about SpaceX launches
```

### Exiting Interactive Mode

Type any of:
- `quit`
- `exit`
- `q`
- Press `Ctrl+C`

---

## Agents & MCP Servers

### WorkflowAgent

**Purpose**: Communication, scheduling, and task management

**Specialization**: Daily productivity tasks

**MCP Servers:**

#### 1. EmailMCP
```
Capabilities:
  • send_email    - Send emails to recipients
  • read_inbox    - Read inbox messages
  • draft_email   - Create email drafts

Tool Schema:
  send_email(to: str, subject: str, body: str)
  
Example:
  "Send email to john@example.com about the meeting"
```

#### 2. CalendarMCP
```
Capabilities:
  • create_event  - Create calendar events
  • list_events   - List scheduled events
  • update_event  - Modify existing events

Tool Schema:
  create_event(title: str, date: str, time: str)
  
Example:
  "Schedule 'Team Meeting' for tomorrow at 3 PM"
```

#### 3. TaskManagementMCP
```
Capabilities:
  • create_task    - Create new tasks
  • update_status  - Update task status
  • list_tasks     - List all tasks

Tool Schema:
  create_task(task: str, priority: str)
  
Example:
  "Create a high priority task to review the code"
```

### ResearchAgent

**Purpose**: Information gathering and research

**Specialization**: Finding and retrieving information

**MCP Servers:**

#### 1. WebSearchMCP
```
Capabilities:
  • search      - Search the web
  • fetch_url   - Fetch content from URLs

Tool Schema:
  search(query: str)
  
Example:
  "Search for Python best practices"
```

---

## How It Works

### Complete Execution Flow

#### Phase 1: User Input
```
User types: "Send email to hafezbahrami@gmail.com"
```

#### Phase 2: Ollama Check/Start
```python
1. OllamaManager.ensure_running()
2. Check: is_ollama_running()?
   - If NO: start_ollama() → "ollama serve"
   - If YES: continue
3. Test model availability
4. ✅ Ollama ready
```

#### Phase 3: Query Routing
```python
1. Framework receives query
2. Router analyzes keywords:
   - "email" detected → route to WorkflowAgent
   - "search" detected → route to ResearchAgent
   - "calendar" detected → route to WorkflowAgent
   - "task" detected → route to WorkflowAgent
3. Selected agent activated
```

#### Phase 4: Agent Processing
```python
1. Agent.process_query(user_query)
2. Build system message:
   "You are WorkflowAgent, specialized in..."
3. Build user message:
   "Send email to hafezbahrami@gmail.com"
4. Gather tool schemas from MCP servers:
   - EmailMCP.get_tool_schema()
   - CalendarMCP.get_tool_schema()
   - TaskMCP.get_tool_schema()
```

#### Phase 5: LLM Reasoning
```python
1. LLMClient.chat(messages, tools)
2. Build prompt for Llama:
   """
   SYSTEM: You are WorkflowAgent...
   USER: Send email to hafezbahrami@gmail.com
   AVAILABLE TOOLS:
   - send_email: Send an email
     Parameters: to, subject, body
   - create_event: Create calendar event
     Parameters: title, date, time
   ...
   """
3. POST to Ollama: /api/generate
4. Llama 3.2 processes:
   - Understands: user wants to send email
   - Identifies: need send_email tool
   - Extracts: to="hafezbahrami@gmail.com", body="..."
5. Llama responds:
   {
     "tool": "send_email",
     "parameters": {
       "to": "hafezbahrami@gmail.com",
       "subject": "Message",
       "body": "..."
     }
   }
```

#### Phase 6: Parse Response
```python
1. Parse Llama's JSON response
2. Extract tool call:
   - function_name: "send_email"
   - arguments: {to, subject, body}
3. Agent recognizes tool call detected
```

#### Phase 7: Tool Execution
```python
1. Agent.execute_tool_call("send_email", arguments)
2. Find MCP server with "send_email" capability
3. EmailMCP.execute("send_email", arguments)
4. EmailMCP:
   - Simulates sending email
   - Prints details
   - Returns success status
5. Result: {
     "status": "success",
     "message": "Email sent to ...",
     "details": {...}
   }
```

#### Phase 8: Response Formatting
```python
1. Agent formats result
2. Creates response:
   """
   ✅ Task completed successfully!
   
   Result: {
     "status": "success",
     ...
   }
   """
```

#### Phase 9: Display to User
```
📧 EMAIL SENT!
To: hafezbahrami@gmail.com
Subject: Message
Body: hey, Hafez...
============================================================

✅ Task completed successfully!
```

#### Phase 10: Interactive Loop
```
Framework: Continue in interactive mode?
User: Types new query OR "quit"
  - New query → Go to Phase 2
  - Quit → Exit
```

---

## Configuration

### Environment Variables

Create a `.env` file:

```bash
# Ollama Configuration
OLLAMA_URL=http://localhost:11434
OLLAMA_MODEL=llama3.2:3b

# Optional: Model path (for reference)
# OLLAMA_MODELS_PATH=/usr/share/ollama/.ollama/models/blobs
```

### Available Settings

| Variable | Default | Description |
|----------|---------|-------------|
| `OLLAMA_URL` | `http://localhost:11434` | Ollama server URL |
| `OLLAMA_MODEL` | `llama3.2:3b` | Model to use |

### Using Different Models

```bash
# Faster model (1B parameters)
OLLAMA_MODEL=llama3.2:1b

# More capable model (8B parameters)
OLLAMA_MODEL=llama3.1:8b

# Code-specialized model
OLLAMA_MODEL=codellama:7b
```

First, pull the model:
```bash
ollama pull llama3.2:1b
```

Then update `.env` and restart the framework.

---

## Troubleshooting

### Issue 1: "Ollama command not found"

**Cause**: Ollama not installed

**Solution**:
```bash
# macOS
brew install ollama

# Linux
curl -fsSL https://ollama.com/install.sh | sh

# Windows
# Download from https://ollama.com/download
```

### Issue 2: "Model not found"

**Cause**: Model not downloaded

**Solution**:
```bash
ollama pull llama3.2:3b
ollama list  # Verify
```

### Issue 3: "Connection refused" or "Server not running"

**Cause**: Ollama server not started

**Solution**:
```bash
# Manual start
ollama serve

# Or let the framework start it automatically
python ollama_agent_framework.py
```

### Issue 4: "Server failed to start"

**Possible Causes**:
1. Port 11434 in use
2. Ollama not properly installed
3. Permission issues

**Solutions**:
```bash
# Check port usage
lsof -i :11434

# Kill existing process
pkill ollama

# Restart
ollama serve
```

### Issue 5: Slow Responses

**Causes**: Model size, hardware limitations

**Solutions**:

1. **Use smaller model**:
```bash
ollama pull llama3.2:1b
echo "OLLAMA_MODEL=llama3.2:1b" > .env
```

2. **Check resources**:
```bash
# Monitor CPU/RAM
top
htop  # if available
```

3. **GPU acceleration** (if NVIDIA GPU):
```bash
nvidia-smi  # Check GPU
# Ollama auto-uses GPU if available
```

### Issue 6: "Module not found: requests"

**Cause**: Missing Python package

**Solution**:
```bash
pip install requests
# or
pip3 install requests
```

### Issue 7: Framework uses mock responses

**Cause**: Ollama not responding properly

**Solution**:
```bash
# Test Ollama directly
ollama run llama3.2:3b "Hello"

# If this works, restart framework
python ollama_agent_framework.py
```

### Issue 8: Tool not being called

**Cause**: Query keywords not matching

**Solution**: Use clear keywords:
- Email: "email", "send", "message"
- Calendar: "calendar", "schedule", "meeting", "event"
- Task: "task", "todo", "remind"
- Search: "search", "find", "look up"

### Getting Help

1. **Run test script**:
```bash
python test_ollama.py
```

2. **Check logs**: Framework prints detailed info

3. **Verify setup**: Review the checklist in [Complete Setup Guide](#complete-setup-guide)

---

## Visualization Diagrams

### Available Diagrams

We provide **5 comprehensive diagrams** to help you understand the system:

#### 1. Architecture Detailed (`ollama-architecture-detailed.mermaid`)
- **Type**: System Architecture
- **Shows**: All components and relationships
- **Use for**: Big picture understanding

#### 2. Execution Flow (`ollama-execution-flow.mermaid`)
- **Type**: Flowchart
- **Shows**: Step-by-step query processing
- **Use for**: Tracing query execution

#### 3. Components (`ollama-components.mermaid`)
- **Type**: Class Diagram
- **Shows**: All classes, properties, methods
- **Use for**: Understanding code structure

#### 4. Sequence (`ollama-sequence.mermaid`)
- **Type**: Sequence Diagram
- **Shows**: Time-ordered interactions
- **Use for**: Understanding communication flow

#### 5. Visual Overview (`agent-framework-architecture.excalidraw`)
- **Type**: Hand-drawn style
- **Shows**: High-level structure
- **Use for**: Presentations

### How to View

**Option 1: GitHub**
- Upload `.mermaid` files
- They render automatically

**Option 2: Mermaid Live**
1. Go to https://mermaid.live
2. Paste diagram code
3. View live preview

**Option 3: VS Code**
1. Install "Markdown Preview Mermaid Support"
2. Create `.md` file with:
````markdown
```mermaid
[paste code here]
```
````
3. Preview

**Option 4: Excalidraw**
1. Go to https://excalidraw.com
2. File → Open
3. Select `.excalidraw` file

See `VISUALIZATION_GUIDE.md` for complete details.

---

## Customization & Extension

### Adding a New MCP Server

#### Step 1: Create MCP Server Class

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
                        "channel": {
                            "type": "string",
                            "description": "Slack channel name"
                        },
                        "message": {
                            "type": "string",
                            "description": "Message content"
                        }
                    },
                    "required": ["channel", "message"]
                }
            }
        }]
    
    def execute(self, capability: str, params: Dict[str, Any]) -> Any:
        if capability == "send_message":
            channel = params.get("channel", "")
            message = params.get("message", "")
            
            # Your Slack API logic here
            print(f"\n💬 SLACK MESSAGE SENT!")
            print(f"Channel: {channel}")
            print(f"Message: {message}")
            
            return {
                "status": "success",
                "channel": channel,
                "message": message
            }
        return None
```

#### Step 2: Add to Agent

```python
workflow_agent = IntelligentAgent(
    name="WorkflowAgent",
    description="...",
    mcp_servers=[
        EmailMCP(),
        CalendarMCP(),
        TaskManagementMCP(),
        SlackMCPServer()  # Add here!
    ],
    llm_client=llm_client
)
```

#### Step 3: Update Router (Optional)

```python
# In route_query method, add:
elif any(word in query_lower for word in ["slack", "channel", "dm"]):
    agent_name = "WorkflowAgent"
```

### Creating a New Agent

```python
# Create specialized agent
document_agent = IntelligentAgent(
    name="DocumentAgent",
    description="An agent specialized in document processing",
    mcp_servers=[
        PDFProcessorMCP(),
        WordProcessorMCP(),
        SpreadsheetMCP()
    ],
    llm_client=llm_client
)

# Register with framework
framework.register_agent(document_agent)

# Update router
elif any(word in query_lower for word in ["pdf", "document", "word"]):
    agent_name = "DocumentAgent"
```

### Integrating Real APIs

Replace mock executions with real API calls:

```python
class EmailMCP(MCPServer):
    def execute(self, capability: str, params: Dict[str, Any]) -> Any:
        if capability == "send_email":
            # Real email sending with SMTP
            import smtplib
            from email.mime.text import MIMEText
            
            msg = MIMEText(params["body"])
            msg['Subject'] = params.get("subject", "No Subject")
            msg['From'] = "your-email@example.com"
            msg['To'] = params["to"]
            
            with smtplib.SMTP('smtp.gmail.com', 587) as server:
                server.starttls()
                server.login("your-email@example.com", "password")
                server.send_message(msg)
            
            return {"status": "success", "to": params["to"]}
```

---

## Advanced Topics

### Performance Optimization

#### 1. Model Selection

| Model | Size | Speed | Capability |
|-------|------|-------|-----------|
| llama3.2:1b | 1.3 GB | ⚡⚡⚡ Fastest | Good for simple tasks |
| llama3.2:3b | 2.0 GB | ⚡⚡ Fast | Balanced (recommended) |
| llama3.1:8b | 4.7 GB | ⚡ Moderate | Best quality |

#### 2. Hardware Acceleration

**GPU Acceleration** (NVIDIA):
```bash
# Check GPU
nvidia-smi

# Ollama automatically uses GPU
# No configuration needed
```

**CPU Optimization**:
```bash
# Set CPU threads (optional)
export OLLAMA_NUM_THREADS=8
```

#### 3. Response Caching

Implement response caching for repeated queries:

```python
from functools import lru_cache

@lru_cache(maxsize=100)
def cached_query(query_hash):
    # Process query
    return result
```

### Multi-Agent Collaboration

Enable agents to work together:

```python
class AgentCollaborationFramework:
    def collaborative_query(self, query: str):
        # Step 1: ResearchAgent gathers info
        research_result = self.research_agent.process_query(
            f"Find information about {query}"
        )
        
        # Step 2: WorkflowAgent acts on it
        action_result = self.workflow_agent.process_query(
            f"Based on this info: {research_result}, send email summary"
        )
        
        return action_result
```

### Persistent Memory

Add conversation history storage:

```python
import json

class PersistentAgent(IntelligentAgent):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.history_file = f"{self.name}_history.json"
        self.load_history()
    
    def load_history(self):
        try:
            with open(self.history_file, 'r') as f:
                self.conversation_history = json.load(f)
        except FileNotFoundError:
            self.conversation_history = []
    
    def save_history(self):
        with open(self.history_file, 'w') as f:
            json.dump(self.conversation_history, f)
    
    def process_query(self, query):
        result = super().process_query(query)
        self.conversation_history.append({
            "query": query,
            "result": result
        })
        self.save_history()
        return result
```

### Logging & Monitoring

Add comprehensive logging:

```python
import logging

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('agent_framework.log'),
        logging.StreamHandler()
    ]
)

logger = logging.getLogger('AgenticFramework')

# In your code:
logger.info(f"Processing query: {query}")
logger.debug(f"LLM response: {response}")
logger.error(f"Error in tool execution: {error}")
```

---

## FAQ

### General Questions

**Q: Do I need an internet connection?**
A: No! Everything runs locally. Internet only needed to download the model initially.

**Q: What data is sent to the cloud?**
A: None. Your queries and data never leave your machine.

**Q: How much does it cost?**
A: $0. Completely free. No subscriptions, no API fees.

**Q: Can I use this commercially?**
A: Yes, subject to Llama's license. Check https://ollama.ai/library/llama3.2

### Technical Questions

**Q: Which Python version do I need?**
A: Python 3.8 or higher.

**Q: Can I use a different model?**
A: Yes! Any Ollama model. Update `OLLAMA_MODEL` in `.env`.

**Q: How do I update Ollama?**
A:
```bash
# macOS
brew upgrade ollama

# Linux
curl -fsSL https://ollama.com/install.sh | sh
```

**Q: Can I run multiple instances?**
A: Yes, but they'll share the same Ollama server.

**Q: How do I backup my setup?**
A: Copy your `.env` file and any custom code. Models are in `/usr/share/ollama/.ollama/models/`.

### Usage Questions

**Q: Why doesn't it understand my query?**
A: Use clear keywords: "email", "calendar", "task", "search".

**Q: Can it actually send real emails?**
A: Currently simulated. You can integrate real APIs (see [Customization](#customization--extension)).

**Q: How do I add more capabilities?**
A: Create new MCP servers and add to agents (see [Adding MCP Server](#adding-a-new-mcp-server)).

**Q: Can agents talk to each other?**
A: Not by default, but you can implement collaboration (see [Multi-Agent Collaboration](#multi-agent-collaboration)).

### Performance Questions

**Q: Why is it slow?**
A: 
- Use smaller model: `llama3.2:1b`
- Use GPU if available
- Close other apps

**Q: How much RAM does it need?**
A: 
- Minimum: 8 GB
- Recommended: 16 GB
- With llama3.2:1b: 4-6 GB
- With llama3.2:3b: 6-8 GB
- With llama3.1:8b: 10-12 GB

**Q: Does it use GPU automatically?**
A: Yes, if you have NVIDIA GPU with CUDA.

### Troubleshooting Questions

**Q: "Connection refused" error?**
A: Start Ollama: `ollama serve`

**Q: Framework uses mock responses?**
A: Ollama not responding. Test: `ollama run llama3.2:3b "test"`

**Q: Test script fails?**
A: Check:
1. Ollama installed? `ollama --version`
2. Model downloaded? `ollama list`
3. Port free? `lsof -i :11434`

---

## Reference

### System Requirements

**Minimum**:
- OS: macOS, Linux, Windows
- RAM: 8 GB
- Storage: 5 GB free
- CPU: Any modern CPU
- Python: 3.8+

**Recommended**:
- RAM: 16 GB+
- Storage: 10 GB+ free
- GPU: NVIDIA GPU with CUDA
- CPU: Multi-core processor
- Python: 3.10+

### File Structure

```
ollama-agent-framework/
├── ollama_agent_framework.py       # Main framework
├── test_ollama.py                  # Setup test
├── .env                            # Configuration
├── requirements.txt                # Dependencies
│
├── README.md                       # This file
├── PROJECT_SUMMARY.md              # Overview
├── SETUP.md                        # Detailed setup
├── VISUALIZATION_GUIDE.md          # Diagram guide
│
├── ollama-architecture-detailed.mermaid
├── ollama-execution-flow.mermaid
├── ollama-components.mermaid
├── ollama-sequence.mermaid
└── agent-framework-architecture.excalidraw
```

### Command Reference

```bash
# Ollama Commands
ollama --version                    # Check version
ollama list                         # List models
ollama pull llama3.2:3b            # Download model
ollama serve                        # Start server
ollama run llama3.2:3b "test"      # Test model
pkill ollama                        # Stop server

# Python Commands
pip install requests                # Install dependency
python test_ollama.py              # Test setup
python ollama_agent_framework.py   # Run framework

# System Commands
lsof -i :11434                     # Check port
nvidia-smi                         # Check GPU
top / htop                         # Monitor resources
```

### Configuration Variables

```bash
# .env file
OLLAMA_URL=http://localhost:11434  # Ollama server URL
OLLAMA_MODEL=llama3.2:3b           # Model name
```

### API Endpoints

Ollama server exposes:

```
GET  /api/tags               # List models
POST /api/generate           # Generate text
POST /api/chat               # Chat completion
GET  /                       # Health check
```

### Model Information

**Llama 3.2 Models**:
- `llama3.2:1b` - 1.3 GB, fastest
- `llama3.2:3b` - 2.0 GB, balanced
- `llama3.1:8b` - 4.7 GB, most capable

**Specialized Models**:
- `codellama:7b` - Code generation
- `mistral:7b` - General purpose
- `neural-chat:7b` - Conversational

### Links

- **Ollama**: https://ollama.ai
- **Ollama Docs**: https://ollama.ai/docs
- **Model Library**: https://ollama.ai/library
- **Llama 3.2**: https://ollama.ai/library/llama3.2
- **MCP Protocol**: https://modelcontextprotocol.io
- **Mermaid Live**: https://mermaid.live
- **Excalidraw**: https://excalidraw.com

---

## 🎉 You're All Set!

**To get started right now:**

```bash
# 1. Test your setup
python test_ollama.py

# 2. Run the framework
python ollama_agent_framework.py

# 3. Start chatting!
👤 You: Send an email to test@example.com saying "Hello from my AI agent!"
```

**Need help?** Check the [Troubleshooting](#troubleshooting) section or run `python test_ollama.py` for diagnostics.

**Want to customize?** See [Customization & Extension](#customization--extension).

**Want to understand more?** Check out the [Visualization Diagrams](#visualization-diagrams).

---

## License

MIT License - Free to use for any purpose!

---

**Built with ❤️ using Ollama, Llama 3.2, and the MCP Protocol**

**Version**: 1.0.0  
**Last Updated**: 2024

---

*Happy building! 🚀*

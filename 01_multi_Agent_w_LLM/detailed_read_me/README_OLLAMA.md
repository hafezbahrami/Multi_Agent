# 🦙 Ollama-Powered Multi-Agent Framework

## NO API KEYS REQUIRED! 100% Local & Free

This version uses your local Llama 3.2 model running on Ollama - completely free and private!

## ✅ Prerequisites

1. **Ollama installed** with `llama3.2:3b` model
2. **Python packages**:
```bash
pip install requests
```

That's it! No API keys, no cloud services, no costs!

## 🚀 Quick Start

### Step 1: Verify Your Ollama Setup

Check if you have the model:
```bash
ollama list
```

You should see `llama3.2:3b` in the list. If not:
```bash
ollama pull llama3.2:3b
```

### Step 2: Run the Framework

```bash
python ollama_agent_framework.py
```

The framework will:
1. ✅ Check if Ollama server is running
2. ✅ Start it automatically if needed
3. ✅ Test your Llama model
4. ✅ Run 4 demo queries
5. ✅ Enter interactive mode

## 💬 Example Usage

```
👤 You: Send an email to hafezbahrami@gmail.com saying "Testing the agent framework"

🤖 WorkflowAgent processing query...
💭 WorkflowAgent thinking with Llama...
🔧 WorkflowAgent using tools...

📧 EMAIL SENT!
To: hafezbahrami@gmail.com
Subject: Message from Agent
Body: Testing the agent framework

✅ Task completed successfully!
```

## 🎯 What It Does

The framework automatically:
- **Starts Ollama** if it's not running
- **Uses your local Llama model** for reasoning
- **Routes queries** to the right agent
- **Executes tools** based on Llama's decisions
- **No external API calls** - everything is local!

## 🏗️ How It Works

```
Your Query
    ↓
Framework starts/checks Ollama server
    ↓
Llama 3.2 analyzes your intent locally
    ↓
Agent selects appropriate MCP server tool
    ↓
Tool executes (email, calendar, tasks, search)
    ↓
Results returned to you
```

## 📂 Your Ollama Setup

Based on your configuration:
- **Model**: `llama3.2:3b`
- **Server**: `http://localhost:11434`
- **Blobs**: `/usr/share/ollama/.ollama/models/blobs`

## 🔧 Configuration

Edit `.env` if needed:
```bash
# Change these if your setup is different
OLLAMA_URL=http://localhost:11434
OLLAMA_MODEL=llama3.2:3b
```

## 🤖 Available Agents

### WorkflowAgent
Handles daily tasks and communication

**Tools:**
- **Email**: Send emails, draft messages
- **Calendar**: Create events, schedule meetings
- **Tasks**: Create todos, set priorities

**Example queries:**
- "Send email to john@example.com"
- "Schedule team meeting tomorrow at 3 PM"
- "Create high priority task: finish report"

### ResearchAgent
Handles information gathering

**Tools:**
- **Web Search**: Find information

**Example queries:**
- "Search for Python tutorials"
- "Find latest AI news"

## 🎨 Key Features

✅ **Automatic Ollama Management**
- Checks if server is running
- Starts it if needed
- Tests model availability

✅ **Smart Pattern Matching**
- Extracts emails from queries
- Detects dates and times
- Identifies task priorities

✅ **Fallback Handling**
- Works even if Llama response isn't perfect
- Pattern-based extraction as backup
- Always tries to execute the user's intent

✅ **100% Local & Private**
- No data sent to cloud
- No API keys needed
- No subscription costs

## 🐛 Troubleshooting

### "Ollama server failed to start"
```bash
# Manually start Ollama
ollama serve

# Then run the framework again
python ollama_agent_framework.py
```

### "Model not found"
```bash
# Pull the model
ollama pull llama3.2:3b

# Verify it's installed
ollama list
```

### Framework uses mock responses instead of Llama
This happens when:
- Ollama isn't responding
- Model takes too long
- Server connection fails

The framework will continue with pattern-based matching. To fix:
1. Check `ollama serve` is running
2. Test: `ollama run llama3.2:3b "Hello"`
3. Restart the framework

### Slow responses
Llama 3.2 3B is fast, but if it's slow:
- Close other apps using GPU/CPU
- Use a smaller model: `ollama pull llama3.2:1b`
- Update `.env`: `OLLAMA_MODEL=llama3.2:1b`

## 📊 Performance Notes

**Llama 3.2 3B Performance:**
- Response time: 1-5 seconds (depending on hardware)
- Memory usage: ~4-6 GB RAM
- Works well on: CPU or GPU
- Best for: Quick tasks, tool calling, simple reasoning

**Tips for better performance:**
- GPU acceleration: Much faster if you have one
- RAM: More RAM = smoother operation
- Model size: 1B model is faster, 3B is more capable

## 🔄 Comparison: Ollama vs Cloud APIs

| Feature | Ollama (This) | Azure OpenAI | Other Cloud APIs |
|---------|---------------|--------------|------------------|
| **Cost** | Free ✅ | $$ 💰 | $$$ 💰💰 |
| **Privacy** | 100% Local ✅ | Cloud ❌ | Cloud ❌ |
| **API Key** | None ✅ | Required ❌ | Required ❌ |
| **Internet** | Not needed ✅ | Required ❌ | Required ❌ |
| **Speed** | Fast (local) ✅ | Variable 🟡 | Variable 🟡 |
| **Capabilities** | Good for tasks ✅ | Best 🟡 | Good 🟡 |

## 🚀 Next Steps

1. **Try it out**: Run the demo and test queries
2. **Customize**: Add your own MCP servers
3. **Integrate**: Connect to real email/calendar APIs
4. **Experiment**: Try different Llama models:
   - `llama3.2:1b` - Fastest
   - `llama3.2:3b` - Balanced (current)
   - `llama3.1:8b` - Most capable
   - `codellama:7b` - For coding tasks

## 📝 Code Integration Example

Your existing Ollama starter code is now integrated!

The framework includes:
```python
# Your OllamaManager class handles:
✓ is_ollama_running() - Health check
✓ start_ollama() - Auto-start server
✓ test_model() - Model verification

# All automatic in main()!
```

## 💡 Tips

1. **First run**: Might take 10-20 seconds as Ollama loads the model
2. **Subsequent runs**: Much faster (model stays in memory)
3. **Interactive mode**: Keep typing queries - Llama is ready!
4. **Restart Ollama**: If issues arise, `pkill ollama` then re-run

## 🎓 Learn More

- [Ollama Documentation](https://ollama.ai/docs)
- [Llama 3.2 Model Card](https://ollama.ai/library/llama3.2)
- [MCP Protocol Spec](https://modelcontextprotocol.io)

## 📄 License

MIT - Use freely for any purpose!

---

**Ready to go?**

```bash
python ollama_agent_framework.py
```

Enjoy your fully local, private, free AI agent framework! 🎉

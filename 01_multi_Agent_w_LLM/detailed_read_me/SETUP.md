# 🚀 Complete Setup Guide - Ollama Multi-Agent Framework

## Step-by-Step Installation

### Step 1: Verify Ollama Installation

Check if Ollama is installed:
```bash
ollama --version
```

If not installed, get it from: https://ollama.ai

### Step 2: Check Your Model

List installed models:
```bash
ollama list
```

You should see `llama3.2:3b` in the output. Example:
```
NAME              ID              SIZE      MODIFIED
llama3.2:3b       a80c4f17acd5    2.0 GB    2 days ago
```

If you don't see it, pull it:
```bash
ollama pull llama3.2:3b
```

This will download ~2 GB. Your blobs are stored in:
```
/usr/share/ollama/.ollama/models/blobs/
```

### Step 3: Install Python Dependencies

```bash
pip install requests
```

That's it! No other dependencies needed.

### Step 4: Test Your Setup

Run the test script:
```bash
python test_ollama.py
```

Expected output:
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
[Your model's response will appear here]

✅ Text generation working!

============================================================
✅ ALL TESTS PASSED!
============================================================
```

### Step 5: Run the Agent Framework

```bash
python ollama_agent_framework.py
```

The framework will:
1. Start Ollama (if not running)
2. Test your model
3. Run 4 demo queries
4. Enter interactive mode

## 📁 File Structure

After setup, you should have:

```
your-project/
├── ollama_agent_framework.py    # Main framework
├── test_ollama.py                # Setup test script
├── .env                          # Configuration (optional)
├── README_OLLAMA.md              # This documentation
└── SETUP.md                      # This setup guide
```

## ⚙️ Configuration

### Default Configuration

The framework uses these defaults:
- **URL**: `http://localhost:11434`
- **Model**: `llama3.2:3b`

### Custom Configuration

Create a `.env` file to customize:

```bash
# Ollama server URL
OLLAMA_URL=http://localhost:11434

# Model to use
OLLAMA_MODEL=llama3.2:3b

# Or use a different model:
# OLLAMA_MODEL=llama3.2:1b    # Faster, smaller
# OLLAMA_MODEL=llama3.1:8b    # More capable
# OLLAMA_MODEL=codellama:7b   # For coding
```

## 🔍 Verification Checklist

Before running the framework, verify:

- ✅ Ollama is installed (`ollama --version`)
- ✅ Model is downloaded (`ollama list`)
- ✅ Server can start (`ollama serve` or auto-starts)
- ✅ Python has requests (`pip list | grep requests`)
- ✅ Test script passes (`python test_ollama.py`)

## 🐛 Common Issues & Solutions

### Issue 1: "Ollama command not found"

**Solution**: Install Ollama
```bash
# macOS
brew install ollama

# Linux
curl -fsSL https://ollama.com/install.sh | sh

# Windows
# Download from https://ollama.com/download
```

### Issue 2: "Model not found"

**Solution**: Pull the model
```bash
ollama pull llama3.2:3b
```

### Issue 3: "Connection refused" or "Server not running"

**Solution**: Start Ollama server
```bash
# In a separate terminal
ollama serve

# Or let the framework start it automatically
```

### Issue 4: "Server failed to start"

**Possible causes:**
1. Port 11434 already in use
2. Ollama not properly installed
3. Permission issues

**Solutions:**
```bash
# Check if port is in use
lsof -i :11434

# Kill existing Ollama process
pkill ollama

# Restart
ollama serve
```

### Issue 5: Slow responses

**Solutions:**
1. **Use smaller model**:
   ```bash
   ollama pull llama3.2:1b
   ```
   Update `.env`: `OLLAMA_MODEL=llama3.2:1b`

2. **Check system resources**:
   ```bash
   # Monitor CPU/RAM usage
   top
   htop  # if installed
   ```

3. **Enable GPU acceleration** (if you have NVIDIA GPU):
   ```bash
   # Ollama automatically uses GPU if available
   # Check with:
   nvidia-smi
   ```

### Issue 6: "Module not found: requests"

**Solution**: Install requests
```bash
pip install requests
# or
pip3 install requests
```

## 💻 System Requirements

### Minimum:
- **RAM**: 8 GB
- **Storage**: 5 GB free
- **CPU**: Any modern CPU
- **OS**: macOS, Linux, Windows

### Recommended:
- **RAM**: 16 GB+
- **Storage**: 10 GB+ free
- **GPU**: NVIDIA GPU (optional, for speed)
- **CPU**: Multi-core processor

## 🎯 Quick Commands Reference

```bash
# Check Ollama version
ollama --version

# List installed models
ollama list

# Pull a model
ollama pull llama3.2:3b

# Start server manually
ollama serve

# Test a model directly
ollama run llama3.2:3b "Hello, how are you?"

# Stop Ollama
pkill ollama

# Check server status
curl http://localhost:11434/api/tags

# Test the framework setup
python test_ollama.py

# Run the agent framework
python ollama_agent_framework.py
```

## 🎓 Next Steps After Setup

1. **Run the test script**:
   ```bash
   python test_ollama.py
   ```

2. **Try the demo**:
   ```bash
   python ollama_agent_framework.py
   ```

3. **Enter interactive mode** and try queries:
   - "Send an email to test@example.com"
   - "Schedule a meeting tomorrow"
   - "Create a task to finish the report"

4. **Customize the framework**:
   - Add your own MCP servers
   - Connect to real APIs
   - Try different models

## 📊 Performance Expectations

### Llama 3.2 3B on different hardware:

**High-end GPU (RTX 4090, etc.)**
- Load time: 1-2 seconds
- Response time: 0.5-2 seconds
- Tokens/sec: 100-200

**Mid-range GPU (GTX 1660, etc.)**
- Load time: 2-5 seconds
- Response time: 2-5 seconds
- Tokens/sec: 30-60

**CPU only (modern Intel/AMD)**
- Load time: 5-10 seconds
- Response time: 5-15 seconds
- Tokens/sec: 5-15

**CPU only (older/laptop)**
- Load time: 10-20 seconds
- Response time: 15-30 seconds
- Tokens/sec: 2-8

## 🔐 Privacy & Security

✅ **100% Local**: All processing happens on your machine
✅ **No API Keys**: No credentials to manage
✅ **No Cloud Calls**: No data sent to external servers
✅ **Private**: Your queries never leave your computer

## 📚 Additional Resources

- **Ollama Docs**: https://ollama.ai/docs
- **Model Library**: https://ollama.ai/library
- **Llama 3.2**: https://ollama.ai/library/llama3.2
- **MCP Spec**: https://modelcontextprotocol.io

## ✅ Setup Complete Checklist

Before considering setup complete, ensure:

- [ ] Ollama installed and version confirmed
- [ ] Model downloaded (llama3.2:3b)
- [ ] Python requests installed
- [ ] Test script passes all checks
- [ ] Framework runs demo successfully
- [ ] Interactive mode works

If all checked ✅, you're ready to go!

---

**Need help?**

Run the test script for diagnostics:
```bash
python test_ollama.py
```

It will tell you exactly what's wrong and how to fix it.

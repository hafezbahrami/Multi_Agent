# MultiAgentFramework — C# Port

A faithful C# (.NET 8) port of the Python multi-agent planner framework.

---

## Project Structure

```
MultiAgentFramework/
├── MultiAgentFramework.csproj
├── Program.cs                  ← Entry point (mirrors af.py)
├── OllamaLLMClient.cs          ← HTTP client for Ollama (mirrors ollama_llm_client.py)
├── Agent.cs                    ← Agent + parameter normalisation (mirrors multi_agent_planner.py)
├── MultiAgentPlanner.cs        ← Router / orchestrator
└── Skills/
    ├── ISkill.cs               ← Base interface
    ├── EmailSkill.cs           ← send_email tool
    ├── CalendarSkill.cs        ← create_event tool
    └── OtherSkills.cs          ← create_task + search tools
```

---

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Ollama](https://ollama.ai) running locally (default: `http://localhost:11434`)

---

## Environment Variables

| Variable       | Default                    | Description                |
|----------------|----------------------------|----------------------------|
| `OLLAMA_URL`   | `http://localhost:11434`   | Ollama server URL          |
| `OLLAMA_MODEL` | `llama3.2:3b`              | Model to use               |

---

## Build & Run

```bash
cd MultiAgentFramework
dotnet build
dotnet run
```

---

## Python → C# Design Decisions

| Python Pattern                        | C# Equivalent                                     |
|---------------------------------------|---------------------------------------------------|
| `inspect.signature` for reflection    | `MethodInfo.GetParameters()` via `System.Reflection` |
| `dict` return types from skills       | `Dictionary<string, object>`                     |
| `json.loads` for LLM response         | `System.Text.Json.JsonNode.Parse`                |
| `dotenv` for env vars                 | `Environment.GetEnvironmentVariable`             |
| `requests.post`                       | `System.Net.Http.HttpClient`                     |
| `hasattr` / `getattr`                 | `Type.GetMethod` + `MethodInfo.Invoke`           |
| `PARAMETER_MAP` dict                  | `Dictionary<string, Dictionary<string, string>>` |

---

## Enabling Interactive Mode

In `Program.cs`, change:

```csharp
const bool doInteractive = false;
```
to `true`.

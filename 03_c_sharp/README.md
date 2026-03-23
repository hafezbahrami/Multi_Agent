# MultiAgentFramework (Single Codebase)

This repository now contains one clean .NET 8 project that combines:

1. Multi-agent orchestration flow
2. Tool discoverability/catalog generation (Task 2)
3. Thin API facade with stable response envelope (Task 3)
4. Entitlement, metering, and redaction hooks (Task 5)

## Structure

- `Program.cs`: demo runner for planner flow and task-focused API flow
- `Core/`: agent, planner, and Ollama client
- `Skills/`: tool/skill implementations
- `Discovery/`: reflection-based tool catalog generation
- `Contracts/`: attributes and DTO contracts
- `Execution/`: entitlement policy, API facade, telemetry, redaction

## Structure Explained In Layman Terms

Think of this app like a small company:

1. Reception gets the request.
2. Manager decides who should handle it.
3. Worker does the task.
4. Security checks permission.
5. Accounting records what happened.

How your folders map to that idea:

1. `Program.cs`
- The demo runner. It starts everything and sends sample requests.

2. `Core/`
- The "brain" of the system.
- `MultiAgentPlanner` decides which agent should handle a query.
- `Agent` chooses and executes a tool.
- `OllamaLLMClient` talks to the LLM.

3. `Skills/`
- The "workers" that actually do tasks: email, calendar, task, search.

4. `Contracts/`
- Shared forms and labels.
- Request/response shapes and tool metadata attributes live here.

5. `Discovery/`
- The automatic "tool catalog builder".
- `ToolCatalogGenerator` scans skill methods and creates a tool catalog.

6. `Execution/`
- The runtime "gate + meter" layer.
- Checks entitlement, executes tools, redacts sensitive output, and logs usage events.

### What Reflection Means Here

In `ToolCatalogGenerator`, reflection means: "look at class methods at runtime instead of hardcoding them manually."

So instead of writing a static list like:
- send_email
- create_event
- create_task

the code automatically reads methods that have `[Tool(...)]` metadata and builds the catalog for you.

Why this is useful:

1. Less manual maintenance.
2. Lower chance of docs and code going out of sync.
3. New tools can appear in catalog automatically when properly annotated.

### End-To-End Flow (Simple)

1. Request enters from `Program.cs`.
2. Planner routes to the right agent.
3. Agent picks and invokes a skill tool.
4. API facade checks permission and wraps response format.
5. Output is sanitized and metering is recorded.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Optional: [Ollama](https://ollama.ai) if you want real LLM responses

Optional environment variables:

- `OLLAMA_URL` (default `http://localhost:11434`)
- `OLLAMA_MODEL` (default `llama3.2:3b`)

## Ollama Server Checklist (Recommended)

Use this quick checklist before running the app.

### 1) Confirm Ollama is installed

```bash
ollama --version
```

### 2) Confirm server is running on port 11434

```bash
curl -s http://localhost:11434/api/tags
```

If server is not running, start it:

```bash
ollama serve
```

Keep that terminal open while testing, or run Ollama as a background service on your machine.

### 3) Confirm model name (important)

```bash
ollama list
```

Pick the model name from this output and use it in `OLLAMA_MODEL`.
Example: `llama3.2:3b`

### 4) Export environment variables (same style as your previous Python setup)

```bash
export ENV=dev
export OLLAMA_URL=http://localhost:11434
export OLLAMA_MODEL=llama3.2:3b
```

Then run:

```bash
dotnet run --project MultiAgentFramework.csproj
```

### 5) If model is missing, pull it

```bash
ollama pull llama3.2:3b
```

### About your blobs path

Your path `/usr/share/ollama/.ollama/models/blobs` is valid as storage, but files there are hash blobs (for example `sha256-...`), not user-facing model names.

Use `ollama list` for model names, not blob filenames.

### Common mistakes

1. `ls la` typo should be `ls -la`.
2. Server not running while app tries to call `/api/generate`.
3. `OLLAMA_MODEL` set to a name that is not present in `ollama list`.

## Build

```bash
dotnet build MultiAgentFramework.csproj
```

or

```bash
dotnet build 03_c_sharp.sln
```

## Run

```bash
dotnet run --project MultiAgentFramework.csproj
```

## Notes

- If Ollama is unavailable, the app still runs with fallback/echo behavior in the LLM client.
- This repo intentionally has a single project entry in the solution.

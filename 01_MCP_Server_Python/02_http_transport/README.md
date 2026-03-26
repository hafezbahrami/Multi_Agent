# MCP Server (HTTP Transport) — Streamable HTTP

This folder contains a minimal Model Context Protocol (MCP) server using **HTTP transport** (FastMCP `streamable-http`) plus a small Python client that connects over HTTP.

If you already have a stdio-based MCP server in this repo, this is the same *logical* MCP server (tools, protocol methods), but running over a different *transport layer*.

## What’s in this folder

- `MCP_server.py`
  - FastMCP server exposing a single tool: `hello_tool(name: str) -> str`
  - Runs with `transport="streamable-http"`
- `Dummy_MCP_http_Client.py`
  - Minimal Streamable-HTTP MCP client
  - Does: `initialize` → `tools/list` → `tools/call (hello_tool)`
  - Optional `--spawn-server` mode to start/stop the server automatically

## Quickstart

### Option A — Run server and client separately

1) Start the HTTP server:

```bash
cd 01_MCP_Server_Python/02_http_transport
pipenv run python MCP_server.py
```

2) In another terminal, run the client:

```bash
cd 01_MCP_Server_Python/02_http_transport
pipenv run python Dummy_MCP_http_Client.py
```

### Option B — One-shot (client spawns the server)

```bash
cd 01_MCP_Server_Python/02_http_transport
pipenv run python Dummy_MCP_http_Client.py --spawn-server
```

## Default URL + configuration

By default, FastMCP binds:

- Host: `127.0.0.1`
- Port: `8000`
- Streamable HTTP path: `/mcp`

So the endpoint URL is:

- `http://127.0.0.1:8000/mcp`

You can change server settings via environment variables (FastMCP settings prefix is `FASTMCP_`):

```bash
export FASTMCP_HOST=127.0.0.1
export FASTMCP_PORT=8000
export FASTMCP_STREAMABLE_HTTP_PATH=/mcp
```

You can change what the *client* connects to via:

```bash
export MCP_SERVER_URL=http://127.0.0.1:8000/mcp
```

## Big picture: HTTP transport vs stdio transport

MCP is a protocol (JSON-RPC framed messages such as `initialize`, `tools/list`, `tools/call`).
The **transport** is how those messages move between client and server.

### Stdio transport (common for local tools)

**How it works**

- A host app or client typically **spawns** the server as a subprocess.
- Messages are exchanged over `stdin` / `stdout`.

**Good fit when**

- You want simplest local integration.
- The MCP host expects stdio (very common for desktop integrations).
- You want minimal network/security concerns.

### HTTP transport (good for “server as a service”)

**How it works**

- The MCP server runs as a **web service** (ASGI app via `uvicorn`).
- Clients connect over HTTP using an MCP HTTP transport (here: `streamable-http`).

**Good fit when**

- You want the MCP server to be long-running and reusable.
- You want multiple clients/sessions.
- You want easier deployment to containers/VMs and standard HTTP observability.

### Which is “better”?

Neither is universally better; it depends on the intended deployment:

- Prefer **stdio** for local, single-host, spawn-per-session integrations.
- Prefer **HTTP** when you want a network service, multi-client usage, or infrastructure-friendly deployment.

## Security note (important if you go beyond localhost)

This demo binds to `127.0.0.1` by default. If you bind to `0.0.0.0` or expose the port publicly, you should add appropriate protections (auth/TLS/reverse proxy, and careful network controls).

## How to debug this http-based server/client

The easiest way to understand the client↔server communication is to debug them in **two separate VS Code debug sessions**.

Why: if you run the client with `--spawn-server`, the server is started as a child process via `subprocess.Popen(...)`, which is harder to step through in the debugger. Subprocess/spawning is not specific to stdio. It’s just a process lifecycle choice your demo client makes. In our HTTP setup, the transport is HTTP (“streamable-http”), but the client still has two ways to get a server running: (a) Run the server as a separate process we start ourself (terminal / VS Code debug session), Or, (b) in our demo, --spawn-server makes the client start the server for convenience using subprocess.Popen(...).

### 0) Turn on useful logs

Set these (locally) to make the server and transport more chatty:

- `FASTMCP_LOG_LEVEL=DEBUG`
- (optional) `FASTMCP_DEBUG=true`

Note: Our Env variables are set in .env file. Sometimes we need to "source .env" file, so they are getting loaded properly.

### Why do we have both `launch.json` and `tasks.json`?

VS Code has two related (but different) systems:

- **`.vscode/launch.json`**: Debugger configurations (Run/Debug)
  - Defines *what to debug* (server vs client), with what `cwd`, arguments, environment variables, and debugger options (`justMyCode`, etc.).
- **`.vscode/tasks.json`**: Task runner configurations (Terminal Tasks)
  - Defines *helper commands* to run before/after debugging (build steps, lint, wait-for-port, etc.).

For one-click debugging of *both* processes, we usually need **both**:

- The compound debug config in `launch.json` starts server + client.
- The client debug config uses `preLaunchTask` that points to a task in `tasks.json`.
- That task waits until the server is actually listening on `127.0.0.1:8000`, to avoid a “client starts too early” race.

If you don’t need one-click debugging, you can ignore the task and start server debugging first, then start client debugging manually.

### 1) Debug the server (VS Code debug session #1)

1. Open `MCP_server.py`.
2. Put a breakpoint inside `hello_tool(...)`.
3. Start debugging the server (Run and Debug → Python: Current File).
4. Ensure the working directory is `01_MCP_Server_Python/02_http_transport`.
5. Confirm the server starts and listens on `http://127.0.0.1:8000` and serves the MCP endpoint at `/mcp`. (/mcp is not a human “web page” endpoint, so opening it in a browser tab won’t show anything meaningful.)

What you should see:

- Uvicorn startup logs
- Streamable HTTP session manager starting

### 2) Debug the client (VS Code debug session #2)

1. Open `Dummy_MCP_http_Client.py`.
2. Put breakpoints in `_run_client()` on these lines:
  - `await session.initialize()`
  - `await session.list_tools()`
  - `await session.call_tool(...)`
3. Start debugging the client (Python: Current File) **without** `--spawn-server`.

As you step:

- After `initialize()`, the server logs should show a new Streamable HTTP session.
- After `list_tools()`, the server logs should show a `ListToolsRequest`.
- On `call_tool(...)`, the server should hit your breakpoint inside `hello_tool(...)`.

### 3) Step into the transport implementation (optional, but best for learning)

If you want to see the actual mechanics of how messages are sent/received over HTTP, allow stepping into library code:

1. In your debug settings, set `justMyCode: false`.
2. Add breakpoints inside the MCP library (in your venv `site-packages`), for example:
  - Client-side: `mcp/client/streamable_http.py` (POST writer + GET stream handling)
  - Server-side: `mcp/server/streamable_http_manager.py` (session creation, request dispatch)

You’ll observe the transport pattern:

- client→server messages are sent via `POST /mcp`
- server→client messages are delivered via a streaming `GET /mcp`
- session lifecycle ends with `DELETE /mcp`

### 4) If you prefer `--spawn-server`

You can still debug the client with `--spawn-server` and watch server logs in the terminal, but you typically won’t be able to step through server code unless you set up an “attach” workflow (e.g., using `debugpy`) or run the server in its own debug session.

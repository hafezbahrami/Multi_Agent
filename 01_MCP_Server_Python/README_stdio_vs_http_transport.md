# MCP Transport Options (Python): `stdio` vs HTTP

This repo contains two minimal MCP server setups under `01_MCP_Server_Python/`:

- **Stdio transport**: `01_stdio_transport/`
  - Best for local, host-spawns-server integrations.
- **HTTP transport (Streamable HTTP)**: `02_http_transport/`
  - Best for running the MCP server as a network-accessible service.

Both approaches implement the *same MCP concepts* (initialize → discover tools → call tools). The main difference is **how bytes move** between client and server (the transport layer).

## What changes (and what does not)

**Same across both transports**

- MCP protocol semantics (JSON-RPC methods such as `initialize`, `tools/list`, `tools/call`)
- Your tool definitions (e.g., a `hello_tool` registered via `@mcp.tool()`)
- Your application logic and tool code

**Different across transports**

- Process and lifecycle model (subprocess vs long-running service)
- Deployment options (local only vs LAN/remote)
- Security surface area (local process boundary vs exposed network endpoint)
- Operational tooling (process logs vs HTTP logs/metrics/proxies)

## Quick comparison

| Topic | Stdio transport (`transport="stdio"`) | HTTP transport (`transport="streamable-http"` / `"sse"`) |
|---|---|---|
| How the server runs | Usually spawned as a subprocess | Long-running web service (ASGI via Uvicorn) |
| How messages flow | `stdin` / `stdout` | HTTP requests + streaming/session semantics |
| Typical environment | Local desktop/agent host | Container/VM/service deployments |
| Multi-client support | Usually one server per host session | Natural fit for multiple clients/sessions |
| Debugging | Inspect process stdout/stderr | Standard HTTP logs + tracing patterns |
| Security | Mostly local | Network-exposed; needs auth/TLS/proxy if remote |
| Client availability | Very widely supported | Requires client support for the chosen HTTP transport |

## Pros / cons

### Stdio transport

**Pros**

- Very simple operationally: the host starts the server and owns its lifecycle.
- Works well with “local tools” use cases and many MCP hosts.
- Minimal network security concerns.

**Cons**

- Hard to share across processes/machines; generally not suitable as a remote service.
- Scaling is often “one server process per host/client session”.
- Observability is tied to local process output unless you add more logging.

### HTTP transport (Streamable HTTP / SSE)

**Pros**

- Natural “server as a service” model (long-running, reusable, multi-client).
- Deployable behind standard infrastructure (reverse proxies, container platforms).
- More standard ops patterns (health checks, centralized logs/metrics).

**Cons**

- Larger security surface: if not kept on localhost, you must plan auth/TLS and network controls.
- Requires an HTTP-capable MCP client/host.
- More moving parts (Uvicorn + HTTP environment concerns like ports, binding, proxies).

## Which should you choose?

A good rule of thumb:

- Choose **stdio** when the MCP server is a *local tool* controlled by a host app (spawned on demand).
- Choose **HTTP** when the MCP server is a *shared service* (multi-client, containerized, remote, or independently managed lifecycle).

If you expect to run the server on another machine, in Docker/Kubernetes, or share it among multiple clients, HTTP is usually the more practical option.

## Notes on HTTP transports in FastMCP

FastMCP supports multiple transports. In this repo, the HTTP example uses:

- `transport="streamable-http"`

Defaults in FastMCP (unless overridden):

- Host: `127.0.0.1`
- Port: `8000`
- Streamable HTTP path: `/mcp`

So the default endpoint is:

- `http://127.0.0.1:8000/mcp`

Server settings can be configured via environment variables with the `FASTMCP_` prefix, for example:

```bash
export FASTMCP_HOST=127.0.0.1
export FASTMCP_PORT=8000
export FASTMCP_STREAMABLE_HTTP_PATH=/mcp
```

Client scripts can typically take a server URL via an env var such as:

```bash
export MCP_SERVER_URL=http://127.0.0.1:8000/mcp
```

## Big-picture insights / pitfalls

- **Don’t expose HTTP MCP publicly by accident**: binding to `0.0.0.0` makes it reachable on the network. If you do that, you should front it with proper controls (auth, TLS, firewall rules, reverse proxy).
- **Transport choice affects lifecycle**: stdio is naturally ephemeral (per host session), HTTP is naturally persistent.
- **Scaling differs**:
  - stdio scaling is “more processes”, often tightly coupled to clients.
  - HTTP scaling is “more server instances”, but may require considering session stickiness depending on transport mode.
- **Client compatibility matters**: choose the transport your intended host/client actually supports.

## Where to look next

- Stdio example folder: `01_stdio_transport/`
- HTTP example folder: `02_http_transport/` (also has its own README with exact run commands)

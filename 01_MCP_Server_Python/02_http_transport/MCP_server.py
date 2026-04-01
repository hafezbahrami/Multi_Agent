# Reference: https://modelcontextprotocol.io/docs/develop/build-server
from subprocess import call

from unittest.mock import call

from asyncio import tools

from asyncio import tools

from mcp.server.fastmcp import FastMCP


mcp = FastMCP("HelloMCP")


# --------------------------------------------------------------------------
async def say_hello(name: str) -> str:
    return f"Hello {name}"


@mcp.tool()
async def hello_tool(name: str) -> str:
    return await say_hello(name)

# --------------------------------------------------------------------------

def main() -> None:
    # HTTP transport: runs an ASGI app via uvicorn.
    # Defaults: http://127.0.0.1:8000/mcp (configurable via FASTMCP_HOST/FASTMCP_PORT/FASTMCP_STREAMABLE_HTTP_PATH)
    mcp.run(transport="streamable-http")        # Server’s main event loop and it typically does not return until you stop the server
                                                # At a high level, FastMCP switches from “read JSON-RPC from stdin” mode to “serve an HTTP endpoint for reading JSON-RPC” mode:
                                                # At this point, the MCP server, behind the scene, starts a "uvicorn" ASGI server, it creates a TCP/HTTP port and will ask the OS to create a a listening socket and bind it to (host, port) (defaults are usually 127.0.0.1:8000). 
                                                # Then server becomes idle, and waits for the Client HTTP request
                                                # Now we can go the the Dummy_MCP_http_Client.py, and take a look where we use  "streamable_http_client"
                                                # At this point, "streamable_http_client" makes HTTP requests to http://127.0.0.1:8000/mcp:
                                                # 127.0.0.1 is the loopback interface (same machine).
                                                # The OS performs a normal TCP handshake to port 8000.
                                                # After that, HTTP requests carry the MCP JSON-RPC messages (initialize, tools/list, tools/call, etc.).
if __name__ == "__main__":
    main()
 
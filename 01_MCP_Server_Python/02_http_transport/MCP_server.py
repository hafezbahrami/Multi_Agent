# Reference: https://modelcontextprotocol.io/docs/develop/build-server
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
    mcp.run(transport="streamable-http")


if __name__ == "__main__":
    main()
 
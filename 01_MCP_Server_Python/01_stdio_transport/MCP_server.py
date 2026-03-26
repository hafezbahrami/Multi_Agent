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
    mcp.run(transport="stdio")                  # Enters the server loop: (this is the “read JSON from stdin forever” loop)
    # mcp.run()


if __name__ == "__main__":
    main()
 
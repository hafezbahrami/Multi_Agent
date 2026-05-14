"""Minimal MCP client for the Streamable HTTP transport.

This script is intentionally small: it initializes a session, lists tools,
and calls the `hello_tool` exposed by `MCP_server.py`.

Defaults:
- Server URL: http://127.0.0.1:8000/mcp (override via MCP_SERVER_URL)

Usage:
- Start server separately: `pipenv run python MCP_server.py`
- Run client:             `pipenv run python Dummy_MCP_http_Client.py`

Or run with `--spawn-server` to start/stop the server automatically.
"""

import os
import signal
import subprocess
import sys
from pathlib import Path

import anyio

from mcp.client.session import ClientSession
from mcp.client.streamable_http import streamable_http_client


THIS_DIR = Path(__file__).resolve().parent
SERVER_CMD = [sys.executable, str(THIS_DIR / "MCP_server.py")]
SERVER_URL = os.getenv("MCP_SERVER_URL", "http://127.0.0.1:8000/mcp")


# If we use --spawn-server, the server becomes a child process started via subprocess.Popen, which is much harder to debug/attach without extra setup.
def _spawn_server() -> subprocess.Popen[str]:
    return subprocess.Popen(SERVER_CMD, text=True)


async def _run_client() -> None:
    async with streamable_http_client(SERVER_URL) as (read_stream, write_stream, _get_session_id):
        async with ClientSession(read_stream, write_stream) as session:
            init = await session.initialize()
            tools = await session.list_tools()
            result = await session.call_tool("hello_tool", {"name": "World"})

    print("======== INITIALIZE ========")
    try:
        print(init.model_dump_json(indent=2))
    except Exception:
        print(init)

    print("======== TOOLS ========")
    try:
        print(tools.model_dump_json(indent=2))
    except Exception:
        print(tools)

    print("======== CALL RESULT ========")
    try:
        print(result.model_dump_json(indent=2))
    except Exception:
        print(result)


def main() -> None:
    spawn = "--spawn-server" in sys.argv
    server: subprocess.Popen[str] | None = None
    try:
        if spawn:
            server = _spawn_server()
            anyio.run(lambda: anyio.sleep(0.5))

        anyio.run(_run_client)
    finally:
        if server and server.poll() is None:
            server.send_signal(signal.SIGTERM)
            try:
                server.wait(timeout=5)
            except subprocess.TimeoutExpired:
                server.kill()


if __name__ == "__main__":
    main()
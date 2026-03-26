# Reference: https://modelcontextprotocol.io/docs/develop/build-server

import sys
import os
import json
import subprocess
from pathlib import Path
from subprocess import PIPE

from mcp_process_debug import list_descendants, pid_ppid


THIS_DIR = Path(__file__).resolve().parent                                          # /....XXXX.../Multi_Agent/01_MCP_Server_Python'
SERVER_CMD = [sys.executable, str(THIS_DIR / "MCP_server.py")]                      # sys.executable = Python-Executable=/....XXXX....location_for_venv..../bin/python'

# -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
# -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
# -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
# Some General points: in JSON-RPC, the "method" field is the verb that tells the server what action you’re requesting.
# There are two different “kinds” of methods in your 4 messages:
# (1) Protocol methods (handled by the MCP server framework itself)
#           "initialize"  &&  "notifications/initialized"  &&   "tools/list"
#           These are part of the MCP protocol. The FastMCP server knows these method names and has built-in handlers for them.
# (2) Tool methods (handled by your code, indirectly)
#           "tools/call" is still a protocol method, but inside its "params" we provide "name": "hello_tool".
#                                            --->        "hello_tool" is your tool name, registered by the decorator in MCP_server.py:12-14

# The client implementation (Dummy_MCP_stdio_Client.py) chooses which of those protocol methods to send and when. In “real life”, an MCP client library (or an app like a desktop host) typically generates these automatically.
# The server framework (FastMCP) implements the handlers for those protocol methods.






# JSON RPC: to init the session, notify the server, and list tools. In a real client, these would be sent as needed rather than all at once.
INIT_SESSION = json.dumps(
    {
        "jsonrpc": "2.0",
        "id": 1,
        "method": "initialize",
        "params": {
            "protocolVersion": "2025-10-05",
            "capabilities": {},
            "clientInfo": {
                "name": "manual-client",
                "version": "0.0.0",
            }
        }
    }
)

# JSON RPC: notify the server that the client is ready to receive notifications. This is important for the server to know when it can start sending events or updates to the client. 
NOTIFY_SESSION = json.dumps(
    {
        "jsonrpc": "2.0",
        "method": "notifications/initialized",
        "params": {},
    }
)


# JSON RPC: to list the tools available on the server. This allows the client to discover what actions it can request from the server, and is typically done after initialization and any necessary setup is complete.
LIST_TOOLS = json.dumps(
    {
        "jsonrpc": "2.0",
        "id": 2,
        "method": "tools/list",
        "params": {
            "cursor": "optional-cursor-value"
        }
    }
)

# JSON RPC: call a specific tool (here: hello_tool) with arguments.
CALL_HELLO_TOOL = json.dumps(
    {
        "jsonrpc": "2.0",
        "id": 3,
        "method": "tools/call",
        "params": {
            "name": "hello_tool",
            "arguments": {
                "name": "World"
            },
        },
    }
)

# -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
# -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
# -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


JOINED_INPUT = "\n".join([INIT_SESSION, NOTIFY_SESSION, LIST_TOOLS, CALL_HELLO_TOOL])  # Combine all JSON-RPC messages to be sent to server, in one single input, separated by \n  ==> JOINED_INPUT = 'JSON_RPC1 \n JSON_RPC2 \n JSON_RPC3'
SERVER = subprocess.Popen(SERVER_CMD, stdin=PIPE, stdout=PIPE, stderr=PIPE, text=True) # start the MCP-server, as a sub-process object of current process, and with the following executable commands, all the stdio PIPEs


def _truthy_env(var_name: str) -> bool:
    value = os.getenv(var_name)
    if value is None:
        return False
    return value.strip().lower() in ("1", "true", "yes", "y", "on")


SHOW_PROC = ("--show-proc" in sys.argv) or _truthy_env("MCP_SHOW_PROC")
if SHOW_PROC:
    print("======== PROCESS INFO ========")
    print(f"Client PID: {os.getpid()} (PPID: {os.getppid()})")
    server_ppid = pid_ppid(SERVER.pid)[1] if pid_ppid(SERVER.pid) else -1
    print(f"Server PID: {SERVER.pid} (PPID: {server_ppid})")
    descendants = list_descendants(SERVER.pid, max_depth=4)
    if descendants:
        print("Server descendants (PID <- PPID):")
        for pid, ppid, depth, cmd in descendants:
            indent = "  " * (depth - 1)
            print(f"{indent}- {pid} <- {ppid}: {cmd}")
    else:
        print("Server descendants: <none observed>")

# What happens in the line below, once we hit "SERVER.communicate":
#      (1) Server start-up:  --> look at the MCP_Server.py. It will create the mcp object, and named it "HelloMCP", then it goes through the mina(), and starts the server-loop, and start listening to any incoming stdin/out
#                                it also goes though all @mcp.tool() decorator, and registers all our tools
#      (2) each 4 client-JSON-GRC message triggers some-special-protocols in the server
#                             -->              JSON-GRC1                                                               JSON-GRC2                                                JSON-GRC3                                      JSON-GRC4
#                             --> protocol handshake (protocol version, capabilities, etc.)    ==>   marking “client is ready” (protocol bookkeeping)   ==> Returning metadata for all registered tools  ==>  dispatcheing to your tool by name: hello_tool(...) in 
 
try:
    stdout, stderr = SERVER.communicate(JOINED_INPUT, timeout=10)
except subprocess.TimeoutExpired:
    SERVER.terminate()
    stdout, stderr = SERVER.communicate(timeout=5)
print("======== STDOUT ========")
for i in stdout.splitlines():                                                               # splits the server’s raw-bit stdout into lines
    print(json.dumps(json.loads(i), indent=4))                                              # parses each line-i as JSON, then pretty-prints it with indentation
    # Look at Note 1 below
print("======== STDERR ========")
print(stderr)

if SERVER.poll() is None:
    SERVER.kill()
    SERVER.wait()


# Note 1: About what we get in stdout.splitlines() print-out:
#           The key idea: your client sends requests like “initialize”, “list tools”, “call tool”; the server replies with a matching "id".
#                       "jsonrpc":  "2.0": this is JSON-RPC protocol framing (MCP uses JSON-RPC as its message envelope).
#                       "id": N:    correlates a response to the specific request you previously sent with "id": N.
#                       "result":   {...}: the successful payload (if it were an error, you’d typically see an "error": {...} instead).

# fOR JSON-RPC ID=1:
#   {
#       "jsonrpc": "2.0",
#       "id": 1,
#       "result": {
#       "protocolVersion": "2025-11-25",
#       "capabilities":     { "experimental": {}, "prompts": {}, "resources": {}, "tools": {} },
#       "serverInfo":       { "name": "HelloMCP", "version": "1.26.0" }
#   }
# }

# fOR JSON-RPC ID=2:
#   {
#       "jsonrpc": "2.0",
#       "id": 2,
#       "result": {
#       "tools": [
#           {
#               "name": "hello_tool",
#               "description": "",
#               "inputSchema": { "type": "object", "properties": {}, "required": [] },
#               "outputSchema": { "type": "object", "properties": {}, "required": [] }
#           }
#       ]
#   }
# }

# fOR JSON-RPC ID=3:
#   {
#       "jsonrpc": "2.0",
#       "id": 3,
#       "result": {
#           "content": [
#                           { "type": "text", "text": "Hello World" }
#                       ],
#           "structuredContent": { "result": "Hello World" },
#           "isError": false
#               }
#   }
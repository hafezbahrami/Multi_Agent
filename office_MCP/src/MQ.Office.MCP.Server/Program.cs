using System.Diagnostics;
using MQ.Office.MCP.Server;
using MQ.Office.MCP.Server.Excel;
using MQ.Office.MCP.Server.Word;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// When MCP_WAIT_FOR_DEBUGGER=1 the server pauses at startup until a debugger attaches.
bool waitForDebugger = Environment.GetEnvironmentVariable("MCP_WAIT_FOR_DEBUGGER") == "1";
if (waitForDebugger)
{
    while (!Debugger.IsAttached)
        Thread.Sleep(100);
}

// Ensure each MCP response is flushed immediately over the stdio pipe.
// Without this, .NET fully buffers stdout when it is redirected.
Console.OutputEncoding = System.Text.Encoding.UTF8;
var stdout = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
Console.SetOut(stdout);

var builder = Host.CreateApplicationBuilder(args);

// Route ALL logging away from stdout (reserved for MCP JSON-RPC) to a
// daily rolling file under %TEMP%\MQOfficeMCP\.
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new TempFileLoggerProvider());

builder.Services.AddOfficeMcpServices();

var host = builder.Build();

if (waitForDebugger)
    Debugger.Break();

await host.RunAsync();

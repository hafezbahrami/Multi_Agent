using System.Diagnostics;

namespace MQ.Office.MCP.Server.IntegrationTests;

/// <summary>
/// Launches the MQ.Office.MCP.Server executable as a child process and exposes
/// its stdin/stdout for MCP JSON-RPC communication. These tests require Word
/// and Excel to be installed on the host.
/// </summary>
public sealed class McpServerFixture : IDisposable
{
    private readonly StringWriter _stderrSink = new();

    public Process Server { get; }

    public string StandardErrorSnapshot => _stderrSink.ToString();

    public McpServerFixture()
    {
        string serverDll = ResolveServerDll();
        var psi = new ProcessStartInfo("dotnet", $"\"{serverDll}\"")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        Server = Process.Start(psi)
            ?? throw new InvalidOperationException("Could not start MQ.Office.MCP.Server.");

        Server.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                lock (_stderrSink) _stderrSink.WriteLine(e.Data);
        };
        Server.BeginErrorReadLine();
    }

    private static string ResolveServerDll()
    {
        // Walk up from this test assembly's bin/Debug to find the server output.
        string baseDir = AppContext.BaseDirectory;
        var dir = new DirectoryInfo(baseDir);
        while (dir is not null && dir.Name != "src")
            dir = dir.Parent;
        if (dir is null)
            throw new InvalidOperationException("Could not locate src/ directory.");

        string config = baseDir.Contains("Release") ? "Release" : "Debug";
        return Path.Combine(
            dir.FullName,
            "MQ.Office.MCP.Server",
            "bin", config, "net10.0",
            "MQ.Office.MCP.Server.dll");
    }

    public void Dispose()
    {
        try
        {
            if (!Server.HasExited)
            {
                Server.Kill(entireProcessTree: true);
                Server.WaitForExit(5000);
            }
        }
        catch { /* ignore */ }
        Server.Dispose();
    }
}

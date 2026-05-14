using MQ.Office.MCP.Server;
using Microsoft.Extensions.Logging;

namespace MQ.Office.MCP.Server.Tests;

public class TempFileLoggerProviderTests
{
    [Fact]
    public void CreateLogger_ReturnsLogger_ThatWritesToTempDirectory()
    {
        using var provider = new TempFileLoggerProvider();
        var logger = provider.CreateLogger("test-category");

        Assert.True(logger.IsEnabled(LogLevel.Information));
        Assert.False(logger.IsEnabled(LogLevel.None));

        logger.LogInformation("hello {Name}", "world");
        logger.LogError(new InvalidOperationException("boom"), "with exception");

        // BeginScope returns null (no-op scope provider)
        Assert.Null(logger.BeginScope("scope"));

        // Log file should exist and contain our entry
        var dir = Path.Combine(Path.GetTempPath(), "MQOfficeMCP");
        var logFile = Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd}.log");
        Assert.True(File.Exists(logFile));
        string contents = File.ReadAllText(logFile);
        Assert.Contains("hello world", contents);
        Assert.Contains("InvalidOperationException", contents);
    }

    [Fact]
    public void Logger_DoesNothing_WhenLevelDisabled()
    {
        using var provider = new TempFileLoggerProvider();
        var logger = provider.CreateLogger("noop");

        // Calling with LogLevel.None should be a no-op (IsEnabled returns false)
        logger.Log(LogLevel.None, new EventId(0), "state", null, (s, _) => s);

        // No assertion target beyond not throwing; coverage is the goal.
        Assert.False(logger.IsEnabled(LogLevel.None));
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var provider = new TempFileLoggerProvider();
        provider.Dispose();
        provider.Dispose(); // safe to call twice
    }
}

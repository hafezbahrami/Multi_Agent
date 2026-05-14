using Microsoft.Extensions.Logging;

namespace MQ.Office.MCP.Server;

/// <summary>
/// Writes log entries to a daily rolling file under <c>%TEMP%\MQOfficeMCP\</c>.
/// Keeps log output away from stdout, which is reserved for MCP JSON-RPC transport.
/// </summary>
internal sealed class TempFileLoggerProvider : ILoggerProvider
{
    private readonly string _logPath;
    private readonly object _fileLock = new();
    private readonly string _sessionId;

    public TempFileLoggerProvider()
    {
        var dir = Path.Combine(Path.GetTempPath(), "MQOfficeMCP");
        Directory.CreateDirectory(dir);
        _logPath = Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd}.log");
        _sessionId = $"pid={Environment.ProcessId};session={Guid.NewGuid():N}";
    }

    public ILogger CreateLogger(string categoryName) =>
        new FileLogger(categoryName, _logPath, _fileLock, _sessionId);

    public void Dispose() { }

    private sealed class FileLogger(string category, string path, object fileLock, string sessionId) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel,-11}] [{sessionId}] {category}: {formatter(state, exception)}";
            if (exception is not null)
                entry += Environment.NewLine + exception.ToString();

            lock (fileLock)
            {
                File.AppendAllText(path, entry + Environment.NewLine);
            }
        }
    }
}

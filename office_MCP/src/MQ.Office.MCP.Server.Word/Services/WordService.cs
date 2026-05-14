using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MQ.Office.MCP.Server.Word;

[SupportedOSPlatform("windows")]
/// <summary>
/// Wraps the Microsoft Word COM API via late-binding (dynamic dispatch).
/// Word must be installed on the machine; the ProgID is "Word.Application".
/// </summary>
public sealed partial class WordService(ILogger<WordService>? logger = null) : IWordService
{
    private dynamic? _app;
    private bool _disposed;
    private readonly ILogger<WordService> _logger = logger ?? NullLogger<WordService>.Instance;

    // ---- Testing seams (internal via InternalsVisibleTo) -----------------

    internal Func<string, dynamic?> _comBootstrap = DefaultBootstrap;

    [ExcludeFromCodeCoverage]
    private static dynamic? DefaultBootstrap(string progId)
    {
        var active = TryGetActiveComObject(progId);
        if (active is not null)
            return (dynamic)active;
        var t = Type.GetTypeFromProgID(progId, throwOnError: true)!;
        return (dynamic?)Activator.CreateInstance(t);
    }

    internal void InjectComApp(dynamic app) => _app = app;

    // ---- P/Invoke: GetActiveObject ---------------------------------------

    [DllImport("oleaut32.dll")]
    private static extern int GetActiveObject(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
        IntPtr pvReserved,
        [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);

    [ExcludeFromCodeCoverage]
    private static object? TryGetActiveComObject(string progId)
    {
        try
        {
            var clsidValue = Microsoft.Win32.Registry.GetValue(
                $@"HKEY_CLASSES_ROOT\{progId}\CLSID", "", null);
            if (clsidValue is not string clsidText || string.IsNullOrWhiteSpace(clsidText))
                return null;
            if (!Guid.TryParse(clsidText, out var clsid))
                return null;
            int hr = GetActiveObject(clsid, IntPtr.Zero, out object obj);
            return hr == 0 ? obj : null;
        }
        catch
        {
            return null;
        }
    }

    // ---- App accessor & guards -------------------------------------------

    private dynamic App
    {
        // Defensive: every caller invokes EnsureActiveDocument first, which
        // throws with a richer message when _app is null. The fallback throw
        // below is unreachable from the public surface.
        [ExcludeFromCodeCoverage]
        get
        {
            if (_app is null)
                throw new InvalidOperationException(
                    "Microsoft Word is not running. " +
                    "Call OpenDocument first, or start Word manually.");
            return _app;
        }
    }

    private void EnsureActiveDocument()
    {
        if (_app is null)
            throw new InvalidOperationException(
                "Microsoft Word is not running. " +
                "Use the word_open_document tool to open a document before calling this operation.");

        dynamic? doc = null;
        try { doc = _app.ActiveDocument; }
        catch (COMException ex)
        {
            throw new InvalidOperationException(
                "Word is running but encountered a COM error when checking for an active document. " +
                $"Try restarting Word. (HR: 0x{ex.HResult:X8})", ex);
        }

        if (doc is null)
            throw new InvalidOperationException(
                "Word is running but no document is currently open. " +
                "Use the word_open_document tool to open a document before calling this operation.");
    }

    // ---- IDisposable -----------------------------------------------------

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            if (_app is not null)
                Marshal.FinalReleaseComObject(_app);
        }
        catch { /* swallow on shutdown */ }
        _app = null;
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MQ.Office.MCP.Server.Excel;

[SupportedOSPlatform("windows")]
/// <summary>
/// Wraps the Microsoft Excel COM API via late-binding (dynamic dispatch).
/// Excel must be installed on the machine; the ProgID is "Excel.Application".
/// </summary>
public sealed partial class ExcelService(ILogger<ExcelService>? logger = null) : IExcelService
{
    private dynamic? _app;
    private bool _disposed;
    private readonly ILogger<ExcelService> _logger = logger ?? NullLogger<ExcelService>.Instance;

    // ---- Testing seams ---------------------------------------------------

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

    // ---- P/Invoke --------------------------------------------------------

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

    // ---- App accessor & guards ------------------------------------------

    private dynamic App
    {
        // Defensive: every caller invokes EnsureActiveWorkbook first, which
        // throws with a richer message when _app is null. The fallback throw
        // below is unreachable from the public surface.
        [ExcludeFromCodeCoverage]
        get
        {
            if (_app is null)
                throw new InvalidOperationException(
                    "Microsoft Excel is not running. " +
                    "Call OpenWorkbook first, or start Excel manually.");
            return _app;
        }
    }

    private void EnsureActiveWorkbook()
    {
        if (_app is null)
            throw new InvalidOperationException(
                "Microsoft Excel is not running. " +
                "Use the excel_open_workbook tool to open a workbook before calling this operation.");

        dynamic? wb = null;
        try { wb = _app.ActiveWorkbook; }
        catch (COMException ex)
        {
            throw new InvalidOperationException(
                "Excel is running but encountered a COM error when checking for an active workbook. " +
                $"Try restarting Excel. (HR: 0x{ex.HResult:X8})", ex);
        }

        if (wb is null)
            throw new InvalidOperationException(
                "Excel is running but no workbook is currently open. " +
                "Use the excel_open_workbook tool to open a workbook before calling this operation.");
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

using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace MQ.Office.MCP.Server.Excel;

[SupportedOSPlatform("windows")]
public sealed partial class ExcelService
{
    /// <inheritdoc/>
    public string OpenWorkbook(string? workbookPath = null)
    {
        _app = _comBootstrap("Excel.Application")
            ?? throw new InvalidOperationException(
                "Could not connect to or create a Microsoft Excel instance.");

        _app.Visible = true;

        dynamic wb;
        if (string.IsNullOrWhiteSpace(workbookPath))
        {
            wb = _app.Workbooks.Add();
            _logger.LogInformation("Created new Excel workbook.");
        }
        else
        {
            workbookPath = Path.GetFullPath(workbookPath);
            if (!File.Exists(workbookPath))
                throw new FileNotFoundException($"Workbook file not found: {workbookPath}");

            wb = _app.Workbooks.Open(workbookPath);
            _logger.LogInformation("Opened Excel workbook from file: {WorkbookPath}", workbookPath);
        }

        return (string)wb.Name;
    }

    /// <inheritdoc/>
    public string SaveWorkbook(string? saveAsPath = null)
    {
        EnsureActiveWorkbook();
        dynamic wb = App.ActiveWorkbook;

        if (string.IsNullOrWhiteSpace(saveAsPath))
        {
            wb.Save();
            _logger.LogInformation("Saved Excel workbook: {Name}", (string)wb.Name);
        }
        else
        {
            saveAsPath = Path.GetFullPath(saveAsPath);
            wb.SaveAs(saveAsPath);
            _logger.LogInformation("Saved Excel workbook as: {Path}", saveAsPath);
        }

        return (string)wb.FullName;
    }

    /// <inheritdoc/>
    public void CloseWorkbook(bool save = true)
    {
        EnsureActiveWorkbook();
        dynamic wb = App.ActiveWorkbook;
        wb.Close(save);
    }
}

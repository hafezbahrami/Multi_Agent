using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MQ.Office.MCP.Server.Excel;

public sealed partial class ExcelTools
{
    private readonly ILogger<ExcelTools> _logger = logger ?? NullLogger<ExcelTools>.Instance;

    [McpServerTool(Name = "excel_open_workbook")]
    [Description(
        "Opens a Microsoft Excel workbook file (.xlsx, .xls), or creates a new empty workbook when no file is specified. " +
        "Launches Excel if it is not already running. " +
        "Returns JSON with success flag and the workbook name.")]
    public string OpenWorkbook(
        [Description("Absolute path to the Excel workbook file (e.g. C:\\Sheets\\Budget.xlsx). " +
                     "Omit or leave empty to create a new blank workbook.")]
        string? workbookPath = null)
    {
        try
        {
            string name = excel.OpenWorkbook(workbookPath);
            _logger.LogInformation("Opened Excel workbook: {Name}", name);
            return Ok(new { name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open Excel workbook.");
            return FailSafe(ex);
        }
    }

    [McpServerTool(Name = "excel_save_workbook")]
    [Description(
        "Saves the currently active Excel workbook. " +
        "If saveAsPath is provided, performs Save As to that path. " +
        "Returns JSON with success flag and the saved workbook's full path.")]
    public string SaveWorkbook(
        [Description("Optional absolute path to save the workbook to (Save As). Omit to save in place.")]
        string? saveAsPath = null)
    {
        try
        {
            string fullName = excel.SaveWorkbook(saveAsPath);
            return Ok(new { fullName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save Excel workbook.");
            return FailSafe(ex);
        }
    }

    [McpServerTool(Name = "excel_close_workbook")]
    [Description("Saves (optionally) and closes the currently active Excel workbook. Returns JSON with success flag.")]
    public string CloseWorkbook(
        [Description("Set to true to save the workbook before closing (default: true).")]
        bool save = true)
    {
        try
        {
            excel.CloseWorkbook(save);
            return Ok(new { saved = save });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close Excel workbook.");
            return FailSafe(ex);
        }
    }
}

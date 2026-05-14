using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace MQ.Office.MCP.Server.Excel;

/// <summary>
/// MCP tool class exposing Microsoft Excel operations as callable tools.
/// </summary>
[McpServerToolType]
public sealed partial class ExcelTools(IExcelService excel, ILogger<ExcelTools>? logger = null)
{
    private static string Ok(object data) =>
        JsonSerializer.Serialize(new { success = true, data });

    private static string Fail(string error) =>
        JsonSerializer.Serialize(new { success = false, error });

    private static string FailSafe(Exception ex) => Fail(ex switch
    {
        InvalidOperationException => ex.Message,
        FileNotFoundException => "The specified file was not found.",
        NotImplementedException => "This operation is not yet implemented.",
        _ => "An unexpected error occurred. Check logs for details."
    });
}

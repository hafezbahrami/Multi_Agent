namespace MQ.Office.MCP.Server.Excel;

public partial interface IExcelService
{
    /// <summary>
    /// Connects to a running Excel instance or starts a new one, then opens the
    /// workbook at <paramref name="workbookPath"/>, or creates a new empty workbook
    /// when <paramref name="workbookPath"/> is null or empty.
    /// Returns the workbook name.
    /// </summary>
    string OpenWorkbook(string? workbookPath = null);

    /// <summary>
    /// Saves the currently active workbook. If <paramref name="saveAsPath"/>
    /// is provided, performs a Save As to that path instead.
    /// </summary>
    string SaveWorkbook(string? saveAsPath = null);

    /// <summary>
    /// Saves (optionally) and closes the currently active workbook.
    /// </summary>
    void CloseWorkbook(bool save = true);
}

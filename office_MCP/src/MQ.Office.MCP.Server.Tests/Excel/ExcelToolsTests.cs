using MQ.Office.MCP.Server.Excel;

namespace MQ.Office.MCP.Server.Tests.Excel;

internal sealed class StubExcelService(Exception? throws = null, string returnValue = "Book.xlsx") : IExcelService
{
    public string OpenWorkbook(string? p) => throws is not null ? throw throws : returnValue;
    public string SaveWorkbook(string? p) => throws is not null ? throw throws : returnValue;
    public void CloseWorkbook(bool save) { if (throws is not null) throw throws; }
    public void Dispose() { }
}

public class ExcelToolsTests
{
    [Fact]
    public void OpenWorkbook_Success_ReturnsJsonWithName()
    {
        var tools = new ExcelTools(new StubExcelService(returnValue: "Book.xlsx"));
        string json = tools.OpenWorkbook(null);
        Assert.Contains("\"success\":true", json);
        Assert.Contains("Book.xlsx", json);
    }

    [Fact]
    public void OpenWorkbook_InvalidOperation_ReturnsErrorMessage()
    {
        var tools = new ExcelTools(new StubExcelService(new InvalidOperationException("specific msg")));
        string json = tools.OpenWorkbook(null);
        Assert.Contains("specific msg", json);
    }

    [Fact]
    public void OpenWorkbook_FileNotFound_ReturnsGenericFileError()
    {
        var tools = new ExcelTools(new StubExcelService(new FileNotFoundException("internal path")));
        string json = tools.OpenWorkbook("anything");
        Assert.Contains("file was not found", json);
    }

    [Fact]
    public void OpenWorkbook_NotImplemented_ReturnsNotImplementedMessage()
    {
        var tools = new ExcelTools(new StubExcelService(new NotImplementedException()));
        string json = tools.OpenWorkbook(null);
        Assert.Contains("not yet implemented", json);
    }

    [Fact]
    public void OpenWorkbook_UnknownException_ReturnsGenericMessage()
    {
        var tools = new ExcelTools(new StubExcelService(new Exception("leak this?")));
        string json = tools.OpenWorkbook(null);
        Assert.Contains("unexpected error", json);
        Assert.DoesNotContain("leak this?", json);
    }

    [Fact]
    public void SaveWorkbook_Success_ReturnsFullName()
    {
        var tools = new ExcelTools(new StubExcelService(returnValue: @"C:\full\path.xlsx"));
        string json = tools.SaveWorkbook(null);
        Assert.Contains("\"success\":true", json);
    }

    [Fact]
    public void SaveWorkbook_Failure_ReturnsError()
    {
        var tools = new ExcelTools(new StubExcelService(new InvalidOperationException("save fail")));
        string json = tools.SaveWorkbook(null);
        Assert.Contains("save fail", json);
    }

    [Fact]
    public void CloseWorkbook_Success_ReturnsSavedFlag()
    {
        var tools = new ExcelTools(new StubExcelService());
        string json = tools.CloseWorkbook(save: true);
        Assert.Contains("\"success\":true", json);
        Assert.Contains("\"saved\":true", json);
    }

    [Fact]
    public void CloseWorkbook_Failure_ReturnsError()
    {
        var tools = new ExcelTools(new StubExcelService(new InvalidOperationException("close fail")));
        string json = tools.CloseWorkbook();
        Assert.Contains("close fail", json);
    }
}

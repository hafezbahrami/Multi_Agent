using MQ.Office.MCP.Server.Excel;
using MQ.Office.MCP.Server.Tests.Helpers;
using System.Runtime.InteropServices;

namespace MQ.Office.MCP.Server.Tests.Excel;

public class ExcelServiceTests
{
    [Fact]
    public void OpenWorkbook_CreatesNewWhenNoPath()
    {
        var svc = new ExcelService();
        svc._comBootstrap = _ => FakeComApp.CreateExcelApp("Book1.xlsx");

        string name = svc.OpenWorkbook(null);

        Assert.Equal("Book1.xlsx", name);
    }

    [Fact]
    public void OpenWorkbook_OpensExistingFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"mq-excel-{Guid.NewGuid():N}.xlsx");
        File.WriteAllText(tempFile, "");
        try
        {
            var svc = new ExcelService();
            svc._comBootstrap = _ => FakeComApp.CreateExcelApp("OpenedBook.xlsx");

            string name = svc.OpenWorkbook(tempFile);

            Assert.Equal("OpenedBook.xlsx", name);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void OpenWorkbook_ThrowsWhenFileMissing()
    {
        var svc = new ExcelService();
        svc._comBootstrap = _ => FakeComApp.CreateExcelApp();

        Assert.Throws<FileNotFoundException>(
            () => svc.OpenWorkbook(@"C:\nope\does-not-exist.xlsx"));
    }

    [Fact]
    public void OpenWorkbook_ThrowsWhenBootstrapReturnsNull()
    {
        var svc = new ExcelService();
        svc._comBootstrap = _ => null;

        var ex = Assert.Throws<InvalidOperationException>(() => svc.OpenWorkbook(null));
        Assert.Contains("Could not connect", ex.Message);
    }

    [Fact]
    public void SaveWorkbook_WithoutOpen_ThrowsInvalidOperation()
    {
        var svc = new ExcelService();
        Assert.Throws<InvalidOperationException>(() => svc.SaveWorkbook(null));
    }

    [Fact]
    public void SaveWorkbook_ReturnsFullName()
    {
        var svc = new ExcelService();
        svc.InjectComApp(FakeComApp.CreateExcelApp("Book.xlsx", @"C:\Temp\Book.xlsx"));

        string full = svc.SaveWorkbook(null);

        Assert.Equal(@"C:\Temp\Book.xlsx", full);
    }

    [Fact]
    public void SaveWorkbook_WithSaveAsPath_CallsSaveAs()
    {
        var svc = new ExcelService();
        svc.InjectComApp(FakeComApp.CreateExcelApp("Book.xlsx", @"C:\Temp\Book.xlsx"));

        string full = svc.SaveWorkbook(@"C:\Temp\Renamed.xlsx");

        Assert.Equal(@"C:\Temp\Book.xlsx", full);
    }

    [Fact]
    public void CloseWorkbook_HappyPath_DoesNotThrow()
    {
        var svc = new ExcelService();
        svc.InjectComApp(FakeComApp.CreateExcelApp());
        svc.CloseWorkbook(save: true);

        svc.InjectComApp(FakeComApp.CreateExcelApp());
        svc.CloseWorkbook(save: false);
    }

    [Fact]
    public void CloseWorkbook_WithoutOpen_ThrowsInvalidOperation()
    {
        var svc = new ExcelService();
        Assert.Throws<InvalidOperationException>(() => svc.CloseWorkbook());
    }

    [Fact]
    public void EnsureActiveWorkbook_WrapsComException_AsInvalidOperation()
    {
        var app = new FakeCom()
            .SetGetter("ActiveWorkbook", () => throw FakeCom.MakeComException());
        var svc = new ExcelService();
        svc.InjectComApp(app);

        var ex = Assert.Throws<InvalidOperationException>(() => svc.SaveWorkbook(null));
        Assert.Contains("COM error", ex.Message);
        Assert.IsType<COMException>(ex.InnerException);
    }

    [Fact]
    public void EnsureActiveWorkbook_ThrowsWhenActiveWorkbookIsNull()
    {
        var app = new FakeCom().Set("ActiveWorkbook", null);
        var svc = new ExcelService();
        svc.InjectComApp(app);

        var ex = Assert.Throws<InvalidOperationException>(() => svc.SaveWorkbook(null));
        Assert.Contains("no workbook is currently open", ex.Message);
    }

    [Fact]
    public void Dispose_IsIdempotent_AndSwallowsComMarshalErrors()
    {
        var svc = new ExcelService();
        svc.InjectComApp(FakeComApp.CreateExcelApp());

        svc.Dispose();
        svc.Dispose();
    }

    [Fact]
    public void Dispose_WithoutApp_IsNoOp()
    {
        var svc = new ExcelService();
        svc.Dispose();
    }

    [Fact]
    public void App_Getter_ThrowsAfterDispose()
    {
        var svc = new ExcelService();
        svc.InjectComApp(FakeComApp.CreateExcelApp());
        svc.Dispose();

        Assert.Throws<InvalidOperationException>(() => svc.SaveWorkbook(null));
    }
}

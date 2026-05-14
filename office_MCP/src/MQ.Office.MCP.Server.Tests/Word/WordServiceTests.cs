using MQ.Office.MCP.Server.Tests.Helpers;
using MQ.Office.MCP.Server.Word;
using System.Runtime.InteropServices;

namespace MQ.Office.MCP.Server.Tests.Word;

public class WordServiceTests
{
    [Fact]
    public void OpenDocument_CreatesNewWhenNoPath()
    {
        var svc = new WordService();
        svc._comBootstrap = _ => FakeComApp.CreateWordApp("Doc1.docx");

        string name = svc.OpenDocument(null);

        Assert.Equal("Doc1.docx", name);
    }

    [Fact]
    public void OpenDocument_OpensExistingFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"mq-word-{Guid.NewGuid():N}.docx");
        File.WriteAllText(tempFile, "");
        try
        {
            var svc = new WordService();
            svc._comBootstrap = _ => FakeComApp.CreateWordApp("OpenedDoc.docx");

            string name = svc.OpenDocument(tempFile);

            Assert.Equal("OpenedDoc.docx", name);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void OpenDocument_ThrowsWhenFileMissing()
    {
        var svc = new WordService();
        svc._comBootstrap = _ => FakeComApp.CreateWordApp();

        Assert.Throws<FileNotFoundException>(
            () => svc.OpenDocument(@"C:\nope\does-not-exist.docx"));
    }

    [Fact]
    public void OpenDocument_ThrowsWhenBootstrapReturnsNull()
    {
        var svc = new WordService();
        svc._comBootstrap = _ => null;

        var ex = Assert.Throws<InvalidOperationException>(() => svc.OpenDocument(null));
        Assert.Contains("Could not connect", ex.Message);
    }

    [Fact]
    public void SaveDocument_WithoutOpenCase_ThrowsInvalidOperation()
    {
        var svc = new WordService();
        Assert.Throws<InvalidOperationException>(() => svc.SaveDocument(null));
    }

    [Fact]
    public void SaveDocument_ReturnsFullName()
    {
        var svc = new WordService();
        svc.InjectComApp(FakeComApp.CreateWordApp("Doc.docx", @"C:\Temp\Doc.docx"));

        string full = svc.SaveDocument(null);

        Assert.Equal(@"C:\Temp\Doc.docx", full);
    }

    [Fact]
    public void SaveDocument_WithSaveAsPath_CallsSaveAs()
    {
        var svc = new WordService();
        svc.InjectComApp(FakeComApp.CreateWordApp("Doc.docx", @"C:\Temp\Doc.docx"));

        string full = svc.SaveDocument(@"C:\Temp\Renamed.docx");

        Assert.Equal(@"C:\Temp\Doc.docx", full);
    }

    [Fact]
    public void CloseDocument_HappyPath_DoesNotThrow()
    {
        var svc = new WordService();
        svc.InjectComApp(FakeComApp.CreateWordApp());
        svc.CloseDocument(save: true);

        svc.InjectComApp(FakeComApp.CreateWordApp());
        svc.CloseDocument(save: false);
    }

    [Fact]
    public void CloseDocument_WithoutOpenCase_ThrowsInvalidOperation()
    {
        var svc = new WordService();
        Assert.Throws<InvalidOperationException>(() => svc.CloseDocument());
    }

    [Fact]
    public void EnsureActiveDocument_WrapsComException_AsInvalidOperation()
    {
        var app = new FakeCom()
            .SetGetter("ActiveDocument", () => throw FakeCom.MakeComException());
        var svc = new WordService();
        svc.InjectComApp(app);

        var ex = Assert.Throws<InvalidOperationException>(() => svc.SaveDocument(null));
        Assert.Contains("COM error", ex.Message);
        Assert.IsType<COMException>(ex.InnerException);
    }

    [Fact]
    public void EnsureActiveDocument_ThrowsWhenActiveDocumentIsNull()
    {
        var app = new FakeCom().Set("ActiveDocument", null);
        var svc = new WordService();
        svc.InjectComApp(app);

        var ex = Assert.Throws<InvalidOperationException>(() => svc.SaveDocument(null));
        Assert.Contains("no document is currently open", ex.Message);
    }

    [Fact]
    public void Dispose_IsIdempotent_AndSwallowsComMarshalErrors()
    {
        var svc = new WordService();
        svc.InjectComApp(FakeComApp.CreateWordApp());

        svc.Dispose(); // Marshal.FinalReleaseComObject throws → catch swallows.
        svc.Dispose(); // _disposed early-return.
    }

    [Fact]
    public void Dispose_WithoutApp_IsNoOp()
    {
        var svc = new WordService();
        svc.Dispose();
    }

    [Fact]
    public void App_Getter_ThrowsBeforeOpen()
    {
        // Hits the App property's null-check by calling SaveDocument (which uses App)
        // on a service whose _app has been set then nulled via Dispose.
        var svc = new WordService();
        svc.InjectComApp(FakeComApp.CreateWordApp());
        svc.Dispose();

        Assert.Throws<InvalidOperationException>(() => svc.SaveDocument(null));
    }
}

using MQ.Office.MCP.Server.Word;

namespace MQ.Office.MCP.Server.Tests.Word;

/// <summary>
/// Test double for <see cref="IWordService"/> that can be configured to throw
/// an arbitrary exception or return a value. Used to drive each branch of
/// <see cref="WordTools.FailSafe"/>.
/// </summary>
internal sealed class StubWordService(Exception? throws = null, string returnValue = "Doc.docx") : IWordService
{
    public string OpenDocument(string? p) => throws is not null ? throw throws : returnValue;
    public string SaveDocument(string? p) => throws is not null ? throw throws : returnValue;
    public void CloseDocument(bool save) { if (throws is not null) throw throws; }
    public int InsertText(string text, string location)
    {
        if (throws is not null) throw throws;
        return text?.Length ?? 0;
    }
    public int ReplaceText(string find, string replace, bool matchCase)
    {
        if (throws is not null) throw throws;
        return 1;
    }
    public void ClearDocument() { if (throws is not null) throw throws; }
    public void SetPageMargins(double? top, double? bottom, double? left, double? right, string unit)
    { if (throws is not null) throw throws; }
    public void SetParagraphFormat(double? lineSpacing, double? spaceBefore, double? spaceAfter, string? alignment, double? firstLineIndent, string unit)
    { if (throws is not null) throw throws; }
    public void Dispose() { }
}

public class WordToolsTests
{
    [Fact]
    public void OpenDocument_Success_ReturnsJsonWithName()
    {
        var tools = new WordTools(new StubWordService(returnValue: "Doc.docx"));
        string json = tools.OpenDocument(null);
        Assert.Contains("\"success\":true", json);
        Assert.Contains("Doc.docx", json);
    }

    [Fact]
    public void OpenDocument_InvalidOperation_ReturnsErrorMessage()
    {
        var tools = new WordTools(new StubWordService(new InvalidOperationException("specific msg")));
        string json = tools.OpenDocument(null);
        Assert.Contains("\"success\":false", json);
        Assert.Contains("specific msg", json);
    }

    [Fact]
    public void OpenDocument_FileNotFound_ReturnsGenericFileError()
    {
        var tools = new WordTools(new StubWordService(new FileNotFoundException("internal path")));
        string json = tools.OpenDocument("anything");
        Assert.Contains("file was not found", json);
    }

    [Fact]
    public void OpenDocument_NotImplemented_ReturnsNotImplementedMessage()
    {
        var tools = new WordTools(new StubWordService(new NotImplementedException()));
        string json = tools.OpenDocument(null);
        Assert.Contains("not yet implemented", json);
    }

    [Fact]
    public void OpenDocument_UnknownException_ReturnsGenericMessage()
    {
        var tools = new WordTools(new StubWordService(new Exception("leak this?")));
        string json = tools.OpenDocument(null);
        Assert.Contains("unexpected error", json);
        Assert.DoesNotContain("leak this?", json);
    }

    [Fact]
    public void SaveDocument_Success_ReturnsFullName()
    {
        var tools = new WordTools(new StubWordService(returnValue: @"C:\full\path.docx"));
        string json = tools.SaveDocument(null);
        Assert.Contains("\"success\":true", json);
    }

    [Fact]
    public void SaveDocument_Failure_ReturnsError()
    {
        var tools = new WordTools(new StubWordService(new InvalidOperationException("save fail")));
        string json = tools.SaveDocument(null);
        Assert.Contains("save fail", json);
    }

    [Fact]
    public void CloseDocument_Success_ReturnsSavedFlag()
    {
        var tools = new WordTools(new StubWordService());
        string json = tools.CloseDocument(save: true);
        Assert.Contains("\"success\":true", json);
        Assert.Contains("\"saved\":true", json);
    }

    [Fact]
    public void CloseDocument_Failure_ReturnsError()
    {
        var tools = new WordTools(new StubWordService(new InvalidOperationException("close fail")));
        string json = tools.CloseDocument();
        Assert.Contains("close fail", json);
    }
}

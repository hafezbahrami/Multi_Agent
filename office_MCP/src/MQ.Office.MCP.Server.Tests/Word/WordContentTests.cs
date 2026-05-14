using System.Dynamic;
using MQ.Office.MCP.Server.Tests.Helpers;
using MQ.Office.MCP.Server.Word;

namespace MQ.Office.MCP.Server.Tests.Word;

public class WordServiceClearTests
{
    [Fact]
    public void ClearDocument_CallsWholeStoryThenDelete()
    {
        var app = FakeComApp.CreateWordApp();
        bool wholeStoryCalled = false, deleteCalled = false;
        app.Selection.WholeStory = (Action)(() => wholeStoryCalled = true);
        app.Selection.Delete = (Action)(() => deleteCalled = true);

        var svc = new WordService();
        svc.InjectComApp(app);

        svc.ClearDocument();

        Assert.True(wholeStoryCalled);
        Assert.True(deleteCalled);
    }

    [Fact]
    public void ClearDocument_WithoutOpenDocument_Throws()
    {
        var svc = new WordService();
        Assert.Throws<InvalidOperationException>(() => svc.ClearDocument());
    }
}

public class WordToolsClearTests
{
    [Fact]
    public void ClearDocument_Success_ReturnsCleared()
    {
        var tools = new WordTools(new StubWordService());
        string json = tools.ClearDocument();
        Assert.Contains("\"success\":true", json);
        Assert.Contains("\"cleared\":true", json);
    }

    [Fact]
    public void ClearDocument_Failure_ReturnsError()
    {
        var tools = new WordTools(new StubWordService(new InvalidOperationException("no doc")));
        string json = tools.ClearDocument();
        Assert.Contains("\"success\":false", json);
        Assert.Contains("no doc", json);
    }
}

public class WordServiceContentTests
{
    [Fact]
    public void InsertText_AppendsToEnd_ByDefault()
    {
        var app = FakeComApp.CreateWordApp();
        int? endKeyArg = null;
        string? typed = null;
        app.Selection.EndKey = (Action<object>)(u => endKeyArg = (int)u);
        app.Selection.TypeText = (Action<object>)(t => typed = (string)t);

        var svc = new WordService();
        svc.InjectComApp(app);

        int count = svc.InsertText("hello world", "end");

        Assert.Equal(11, count);
        Assert.Equal(6, endKeyArg); // wdStory
        Assert.Equal("hello world", typed);
    }

    [Fact]
    public void InsertText_DefaultLocation_IsEnd()
    {
        var app = FakeComApp.CreateWordApp();
        int? endKeyArg = null;
        app.Selection.EndKey = (Action<object>)(u => endKeyArg = (int)u);

        var svc = new WordService();
        svc.InjectComApp(app);

        svc.InsertText("hi");

        Assert.Equal(6, endKeyArg);
    }

    [Fact]
    public void InsertText_Start_CallsHomeKey()
    {
        var app = FakeComApp.CreateWordApp();
        int? homeKeyArg = null;
        app.Selection.HomeKey = (Action<object>)(u => homeKeyArg = (int)u);

        var svc = new WordService();
        svc.InjectComApp(app);

        svc.InsertText("hi", "start");

        Assert.Equal(6, homeKeyArg);
    }

    [Fact]
    public void InsertText_Cursor_DoesNotMoveSelection()
    {
        var app = FakeComApp.CreateWordApp();
        bool endCalled = false, homeCalled = false;
        app.Selection.EndKey = (Action<object>)(_ => endCalled = true);
        app.Selection.HomeKey = (Action<object>)(_ => homeCalled = true);

        var svc = new WordService();
        svc.InjectComApp(app);

        svc.InsertText("hi", "cursor");

        Assert.False(endCalled);
        Assert.False(homeCalled);
    }

    [Fact]
    public void InsertText_UnsupportedLocation_Throws()
    {
        var svc = new WordService();
        svc.InjectComApp(FakeComApp.CreateWordApp());

        var ex = Assert.Throws<InvalidOperationException>(() => svc.InsertText("x", "middle"));
        Assert.Contains("location", ex.Message);
    }

    [Fact]
    public void InsertText_NullText_Throws()
    {
        var svc = new WordService();
        svc.InjectComApp(FakeComApp.CreateWordApp());

        Assert.Throws<InvalidOperationException>(() => svc.InsertText(null!, "end"));
    }

    [Fact]
    public void InsertText_WithoutOpenDocument_Throws()
    {
        var svc = new WordService();
        Assert.Throws<InvalidOperationException>(() => svc.InsertText("x", "end"));
    }

    [Fact]
    public void SetPageMargins_Cm_ConvertsToPoints()
    {
        var app = FakeComApp.CreateWordApp();
        var svc = new WordService();
        svc.InjectComApp(app);

        svc.SetPageMargins(top: 2.54, bottom: 2.54, left: 2.54, right: 2.54, unit: "cm");

        // 2.54 cm = 1 inch = 72 pt
        Assert.Equal(72.0, (double)app.ActiveDocument.PageSetup.TopMargin, 3);
        Assert.Equal(72.0, (double)app.ActiveDocument.PageSetup.BottomMargin, 3);
        Assert.Equal(72.0, (double)app.ActiveDocument.PageSetup.LeftMargin, 3);
        Assert.Equal(72.0, (double)app.ActiveDocument.PageSetup.RightMargin, 3);
    }

    [Fact]
    public void SetPageMargins_Inches_ConvertsToPoints()
    {
        var app = FakeComApp.CreateWordApp();
        var svc = new WordService();
        svc.InjectComApp(app);

        svc.SetPageMargins(top: 1.0, bottom: null, left: null, right: null, unit: "inches");

        Assert.Equal(72.0, (double)app.ActiveDocument.PageSetup.TopMargin, 3);
    }

    [Fact]
    public void SetPageMargins_Points_PassesThrough()
    {
        var app = FakeComApp.CreateWordApp();
        var svc = new WordService();
        svc.InjectComApp(app);

        svc.SetPageMargins(top: 36.0, bottom: 36.0, left: 36.0, right: 36.0, unit: "points");

        Assert.Equal(36.0, (double)app.ActiveDocument.PageSetup.TopMargin, 3);
    }

    [Fact]
    public void SetPageMargins_NullValues_AreSkipped()
    {
        var app = FakeComApp.CreateWordApp();
        // Pre-seed sentinel values so we can verify they aren't touched.
        app.ActiveDocument.PageSetup.TopMargin = 99.0;
        app.ActiveDocument.PageSetup.BottomMargin = 99.0;
        app.ActiveDocument.PageSetup.LeftMargin = 99.0;
        app.ActiveDocument.PageSetup.RightMargin = 99.0;

        var svc = new WordService();
        svc.InjectComApp(app);

        svc.SetPageMargins(top: null, bottom: null, left: 1.0, right: null, unit: "inches");

        Assert.Equal(99.0, (double)app.ActiveDocument.PageSetup.TopMargin, 3);
        Assert.Equal(99.0, (double)app.ActiveDocument.PageSetup.BottomMargin, 3);
        Assert.Equal(72.0, (double)app.ActiveDocument.PageSetup.LeftMargin, 3);
        Assert.Equal(99.0, (double)app.ActiveDocument.PageSetup.RightMargin, 3);
    }

    [Fact]
    public void SetPageMargins_UnknownUnit_Throws()
    {
        var svc = new WordService();
        svc.InjectComApp(FakeComApp.CreateWordApp());

        Assert.Throws<InvalidOperationException>(
            () => svc.SetPageMargins(top: 1, bottom: null, left: null, right: null, unit: "furlongs"));
    }

    [Fact]
    public void SetParagraphFormat_LineSpacing_AppliesMultipleRule()
    {
        var app = FakeComApp.CreateWordApp();
        var svc = new WordService();
        svc.InjectComApp(app);

        svc.SetParagraphFormat(
            lineSpacing: 1.5,
            spaceBefore: null,
            spaceAfter: null,
            alignment: null,
            firstLineIndent: null);

        Assert.Equal(5, (int)app.Selection.ParagraphFormat.LineSpacingRule); // wdLineSpaceMultiple
        Assert.Equal(18.0, (double)app.Selection.ParagraphFormat.LineSpacing, 3); // 1.5 * 12
    }

    [Theory]
    [InlineData("left", 0)]
    [InlineData("center", 1)]
    [InlineData("right", 2)]
    [InlineData("justify", 3)]
    public void SetParagraphFormat_Alignment_MapsToWdEnum(string alignment, int expected)
    {
        var app = FakeComApp.CreateWordApp();
        var svc = new WordService();
        svc.InjectComApp(app);

        svc.SetParagraphFormat(null, null, null, alignment, null);

        Assert.Equal(expected, (int)app.Selection.ParagraphFormat.Alignment);
    }

    [Fact]
    public void SetParagraphFormat_Spacing_ConvertsCmToPoints()
    {
        var app = FakeComApp.CreateWordApp();
        var svc = new WordService();
        svc.InjectComApp(app);

        svc.SetParagraphFormat(
            lineSpacing: null,
            spaceBefore: 2.54,
            spaceAfter: 2.54,
            alignment: null,
            firstLineIndent: 2.54,
            unit: "cm");

        Assert.Equal(72.0, (double)app.Selection.ParagraphFormat.SpaceBefore, 3);
        Assert.Equal(72.0, (double)app.Selection.ParagraphFormat.SpaceAfter, 3);
        Assert.Equal(72.0, (double)app.Selection.ParagraphFormat.FirstLineIndent, 3);
    }

    [Fact]
    public void SetParagraphFormat_NullsLeaveSentinelsUntouched()
    {
        var app = FakeComApp.CreateWordApp();
        app.Selection.ParagraphFormat.SpaceBefore = 99.0;
        app.Selection.ParagraphFormat.SpaceAfter = 99.0;
        app.Selection.ParagraphFormat.FirstLineIndent = 99.0;
        app.Selection.ParagraphFormat.Alignment = 42;
        app.Selection.ParagraphFormat.LineSpacing = 99.0;
        app.Selection.ParagraphFormat.LineSpacingRule = 99;

        var svc = new WordService();
        svc.InjectComApp(app);

        svc.SetParagraphFormat(null, null, null, null, null);

        Assert.Equal(99.0, (double)app.Selection.ParagraphFormat.SpaceBefore, 3);
        Assert.Equal(99.0, (double)app.Selection.ParagraphFormat.SpaceAfter, 3);
        Assert.Equal(99.0, (double)app.Selection.ParagraphFormat.FirstLineIndent, 3);
        Assert.Equal(42, (int)app.Selection.ParagraphFormat.Alignment);
        Assert.Equal(99.0, (double)app.Selection.ParagraphFormat.LineSpacing, 3);
        Assert.Equal(99, (int)app.Selection.ParagraphFormat.LineSpacingRule);
    }

    [Fact]
    public void SetParagraphFormat_UnknownAlignment_Throws()
    {
        var svc = new WordService();
        svc.InjectComApp(FakeComApp.CreateWordApp());

        Assert.Throws<InvalidOperationException>(
            () => svc.SetParagraphFormat(null, null, null, "sideways", null));
    }
}

public class WordToolsContentTests
{
    [Fact]
    public void InsertText_Success_ReturnsCount()
    {
        var tools = new WordTools(new StubWordService());
        string json = tools.InsertText("hello", "end");
        Assert.Contains("\"success\":true", json);
        Assert.Contains("\"inserted\":5", json);
    }

    [Fact]
    public void InsertText_Failure_ReturnsError()
    {
        var tools = new WordTools(new StubWordService(new InvalidOperationException("nope")));
        string json = tools.InsertText("x", "end");
        Assert.Contains("\"success\":false", json);
        Assert.Contains("nope", json);
    }

    [Fact]
    public void SetPageMargins_Success_ReturnsValues()
    {
        var tools = new WordTools(new StubWordService());
        string json = tools.SetPageMargins(2.5, 2.5, 2.5, 2.5, "cm");
        Assert.Contains("\"success\":true", json);
    }

    [Fact]
    public void SetPageMargins_Failure_ReturnsError()
    {
        var tools = new WordTools(new StubWordService(new InvalidOperationException("bad unit")));
        string json = tools.SetPageMargins(1, 1, 1, 1, "furlongs");
        Assert.Contains("bad unit", json);
    }

    [Fact]
    public void SetParagraphFormat_Success_ReturnsValues()
    {
        var tools = new WordTools(new StubWordService());
        string json = tools.SetParagraphFormat(1.5, 0.5, 0.5, "justify", 1.0, "cm");
        Assert.Contains("\"success\":true", json);
    }

    [Fact]
    public void SetParagraphFormat_Failure_ReturnsError()
    {
        var tools = new WordTools(new StubWordService(new InvalidOperationException("bad align")));
        string json = tools.SetParagraphFormat(null, null, null, "sideways", null);
        Assert.Contains("bad align", json);
    }
}

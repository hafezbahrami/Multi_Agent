using System.Dynamic;

namespace MQ.Office.MCP.Server.Tests.Helpers;

/// <summary>
/// Builds a fake COM app object using ExpandoObject so tests can exercise
/// WordService/ExcelService without requiring Word or Excel installed.
/// </summary>
internal static class FakeComApp
{
    public static dynamic CreateWordApp(string docName = "Document1", string? fullName = null)
    {
        dynamic doc = new ExpandoObject();
        doc.Name = docName;
        doc.FullName = fullName ?? Path.Combine(Path.GetTempPath(), docName);
        doc.Save = (Action)(() => { });
        doc.SaveAs2 = (Action<object>)(_ => { });
        doc.Close = (Action<object>)(_ => { });

        // PageSetup: properties are assigned via dynamic on ExpandoObject.
        dynamic pageSetup = new ExpandoObject();
        doc.PageSetup = pageSetup;

        dynamic documents = new ExpandoObject();
        documents.Add = (Func<dynamic>)(() => doc);
        documents.Open = (Func<object, dynamic>)(_ => doc);

        // Selection with TypeText / EndKey / HomeKey, ParagraphFormat, and Find.
        dynamic replacement = new ExpandoObject();
        replacement.Text = "";
        replacement.ClearFormatting = (Action)(() => { });

        dynamic find = new ExpandoObject();
        find.Text = "";
        find.Replacement = replacement;
        find.Forward = true;
        find.Wrap = 1;
        find.MatchCase = false;
        find.MatchWholeWord = false;
        find.MatchWildcards = false;
        find.ClearFormatting = (Action)(() => { });
        find.Execute = (Action<object>)(_ => { });

        dynamic paragraphFormat = new ExpandoObject();
        dynamic selection = new ExpandoObject();
        selection.ParagraphFormat = paragraphFormat;
        selection.Find = find;
        selection.TypeText = (Action<object>)(_ => { });
        selection.EndKey = (Action<object>)(_ => { });
        selection.HomeKey = (Action<object>)(_ => { });
        selection.WholeStory = (Action)(() => { });
        selection.Delete = (Action)(() => { });

        dynamic app = new ExpandoObject();
        app.Visible = false;
        app.Documents = documents;
        app.ActiveDocument = doc;
        app.Selection = selection;
        return app;
    }

    public static dynamic CreateExcelApp(string wbName = "Book1", string? fullName = null)
    {
        dynamic wb = new ExpandoObject();
        wb.Name = wbName;
        wb.FullName = fullName ?? Path.Combine(Path.GetTempPath(), wbName);
        wb.Save = (Action)(() => { });
        wb.SaveAs = (Action<object>)(_ => { });
        wb.Close = (Action<object>)(_ => { });

        dynamic workbooks = new ExpandoObject();
        workbooks.Add = (Func<dynamic>)(() => wb);
        workbooks.Open = (Func<object, dynamic>)(_ => wb);

        dynamic app = new ExpandoObject();
        app.Visible = false;
        app.Workbooks = workbooks;
        app.ActiveWorkbook = wb;
        return app;
    }
}

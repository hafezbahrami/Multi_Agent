using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace MQ.Office.MCP.Server.Word;

[SupportedOSPlatform("windows")]
public sealed partial class WordService
{
    /// <inheritdoc/>
    public string OpenDocument(string? documentPath = null)
    {
        _app = _comBootstrap("Word.Application")
            ?? throw new InvalidOperationException(
                "Could not connect to or create a Microsoft Word instance.");

        _app.Visible = true;

        dynamic doc;
        if (string.IsNullOrWhiteSpace(documentPath))
        {
            doc = _app.Documents.Add();
            _logger.LogInformation("Created new Word document.");
        }
        else
        {
            documentPath = Path.GetFullPath(documentPath);
            if (!File.Exists(documentPath))
                throw new FileNotFoundException($"Document file not found: {documentPath}");

            doc = _app.Documents.Open(documentPath);
            _logger.LogInformation("Opened Word document from file: {DocumentPath}", documentPath);
        }

        return (string)doc.Name;
    }

    /// <inheritdoc/>
    public string SaveDocument(string? saveAsPath = null)
    {
        EnsureActiveDocument();
        dynamic doc = App.ActiveDocument;

        if (string.IsNullOrWhiteSpace(saveAsPath))
        {
            doc.Save();
            _logger.LogInformation("Saved Word document: {Name}", (string)doc.Name);
        }
        else
        {
            saveAsPath = Path.GetFullPath(saveAsPath);
            doc.SaveAs2(saveAsPath);
            _logger.LogInformation("Saved Word document as: {Path}", saveAsPath);
        }

        return (string)doc.FullName;
    }

    /// <inheritdoc/>
    public void CloseDocument(bool save = true)
    {
        EnsureActiveDocument();
        dynamic doc = App.ActiveDocument;
        // wdSaveOptions: -1 = wdSaveChanges, 0 = wdDoNotSaveChanges
        doc.Close(save ? -1 : 0);
    }
}

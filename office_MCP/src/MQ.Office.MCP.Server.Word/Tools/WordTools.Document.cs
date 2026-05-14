using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MQ.Office.MCP.Server.Word;

public sealed partial class WordTools
{
    private readonly ILogger<WordTools> _logger = logger ?? NullLogger<WordTools>.Instance;

    [McpServerTool(Name = "word_open_document")]
    [Description(
        "Opens a Microsoft Word document file (.docx, .doc), or creates a new empty document when no file is specified. " +
        "Launches Word if it is not already running. " +
        "Returns JSON with success flag and the document name.")]
    public string OpenDocument(
        [Description("Absolute path to the Word document file (e.g. C:\\Docs\\Report.docx). " +
                     "Omit or leave empty to create a new blank document.")]
        string? documentPath = null)
    {
        try
        {
            string name = word.OpenDocument(documentPath);
            _logger.LogInformation("Opened Word document: {Name}", name);
            return Ok(new { name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open Word document.");
            return FailSafe(ex);
        }
    }

    [McpServerTool(Name = "word_save_document")]
    [Description(
        "Saves the currently active Word document. " +
        "If saveAsPath is provided, performs Save As to that path. " +
        "Returns JSON with success flag and the saved document's full path.")]
    public string SaveDocument(
        [Description("Optional absolute path to save the document to (Save As). Omit to save in place.")]
        string? saveAsPath = null)
    {
        try
        {
            string fullName = word.SaveDocument(saveAsPath);
            return Ok(new { fullName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save Word document.");
            return FailSafe(ex);
        }
    }

    [McpServerTool(Name = "word_close_document")]
    [Description("Saves (optionally) and closes the currently active Word document. Returns JSON with success flag.")]
    public string CloseDocument(
        [Description("Set to true to save the document before closing (default: true).")]
        bool save = true)
    {
        try
        {
            word.CloseDocument(save);
            return Ok(new { saved = save });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close Word document.");
            return FailSafe(ex);
        }
    }
}

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MQ.Office.MCP.Server.Word;

public sealed partial class WordTools
{
    [McpServerTool(Name = "word_replace_text")]
    [Description(
        "Finds all occurrences of a string in the active Word document and replaces them. " +
        "Pass an empty string for 'replace' to delete the found text. " +
        "Returns JSON with success flag.")]
    public string ReplaceText(
        [Description("The text to find.")] string find,
        [Description("The replacement text. Use empty string to delete the found text.")] string replace = "",
        [Description("Whether the search is case-sensitive (default: false).")] bool matchCase = false)
    {
        try
        {
            word.ReplaceText(find, replace, matchCase);
            return Ok(new { find, replace, matchCase });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to replace text in Word document.");
            return FailSafe(ex);
        }
    }

    [McpServerTool(Name = "word_clear_document")]
    [Description(
        "Deletes all text content from the currently active Word document, leaving it empty. " +
        "Returns JSON with success flag.")]
    public string ClearDocument()
    {
        try
        {
            word.ClearDocument();
            return Ok(new { cleared = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear Word document.");
            return FailSafe(ex);
        }
    }

    [McpServerTool(Name = "word_insert_text")]
    [Description(
        "Inserts text into the currently active Word document. " +
        "By default appends to the end of the document. " +
        "Returns JSON with success flag and the number of characters inserted.")]
    public string InsertText(
        [Description("The text to insert. Use \\n for line breaks (Word converts to paragraphs).")]
        string text,
        [Description("Where to insert: 'end' (default), 'start', or 'cursor' (current selection).")]
        string location = "end")
    {
        try
        {
            int count = word.InsertText(text, location);
            return Ok(new { inserted = count, location });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert text into Word document.");
            return FailSafe(ex);
        }
    }

    [McpServerTool(Name = "word_set_page_margins")]
    [Description(
        "Sets the page margins of the active Word document. " +
        "Any omitted value is left unchanged. " +
        "Returns JSON with success flag.")]
    public string SetPageMargins(
        [Description("Top margin. Omit to leave unchanged.")] double? top = null,
        [Description("Bottom margin. Omit to leave unchanged.")] double? bottom = null,
        [Description("Left margin. Omit to leave unchanged.")] double? left = null,
        [Description("Right margin. Omit to leave unchanged.")] double? right = null,
        [Description("Unit for margin values: 'cm' (default), 'inches', or 'points'.")]
        string unit = "cm")
    {
        try
        {
            word.SetPageMargins(top, bottom, left, right, unit);
            return Ok(new { top, bottom, left, right, unit });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set page margins.");
            return FailSafe(ex);
        }
    }

    [McpServerTool(Name = "word_set_paragraph_format")]
    [Description(
        "Sets paragraph formatting on the current selection of the active Word document. " +
        "Any omitted value is left unchanged. " +
        "Returns JSON with success flag.")]
    public string SetParagraphFormat(
        [Description("Line-spacing multiplier (e.g. 1.0 single, 1.5, 2.0 double). Omit to leave unchanged.")]
        double? lineSpacing = null,
        [Description("Space before paragraph (in chosen unit). Omit to leave unchanged.")]
        double? spaceBefore = null,
        [Description("Space after paragraph (in chosen unit). Omit to leave unchanged.")]
        double? spaceAfter = null,
        [Description("Alignment: 'left', 'center', 'right', or 'justify'. Omit to leave unchanged.")]
        string? alignment = null,
        [Description("First-line indent (in chosen unit). Omit to leave unchanged.")]
        double? firstLineIndent = null,
        [Description("Unit for spacing/indent values: 'cm' (default), 'inches', or 'points'.")]
        string unit = "cm")
    {
        try
        {
            word.SetParagraphFormat(lineSpacing, spaceBefore, spaceAfter, alignment, firstLineIndent, unit);
            return Ok(new { lineSpacing, spaceBefore, spaceAfter, alignment, firstLineIndent, unit });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set paragraph format.");
            return FailSafe(ex);
        }
    }
}

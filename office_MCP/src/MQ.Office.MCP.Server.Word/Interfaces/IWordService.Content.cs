namespace MQ.Office.MCP.Server.Word;

public partial interface IWordService
{
    /// <summary>
    /// Inserts <paramref name="text"/> into the active document at the position
    /// indicated by <paramref name="location"/> ("end", "start", or "cursor").
    /// Returns the number of characters inserted.
    /// </summary>
    int InsertText(string text, string location = "end");

    /// <summary>
    /// Finds all occurrences of <paramref name="find"/> in the active document
    /// and replaces them with <paramref name="replace"/>.
    /// Pass an empty string for <paramref name="replace"/> to delete the found text.
    /// Returns the number of replacements made.
    /// </summary>
    int ReplaceText(string find, string replace, bool matchCase = false);

    /// <summary>
    /// Deletes all text content from the active document, leaving an empty document.
    /// </summary>
    void ClearDocument();

    /// <summary>
    /// Sets the page margins of the active document. Any null value is left
    /// unchanged. <paramref name="unit"/> may be "cm" (default), "inches", or
    /// "points".
    /// </summary>
    void SetPageMargins(
        double? top,
        double? bottom,
        double? left,
        double? right,
        string unit = "cm");

    /// <summary>
    /// Sets paragraph formatting on the current selection. Any null value is
    /// left unchanged.
    /// </summary>
    /// <param name="lineSpacing">Line-spacing multiplier (e.g. 1.0, 1.5, 2.0).</param>
    /// <param name="spaceBefore">Space above paragraph.</param>
    /// <param name="spaceAfter">Space below paragraph.</param>
    /// <param name="alignment">"left", "center", "right", or "justify".</param>
    /// <param name="firstLineIndent">First-line indent.</param>
    /// <param name="unit">Unit for spacing/indent values: "cm" (default), "inches", or "points".</param>
    void SetParagraphFormat(
        double? lineSpacing,
        double? spaceBefore,
        double? spaceAfter,
        string? alignment,
        double? firstLineIndent,
        string unit = "cm");
}

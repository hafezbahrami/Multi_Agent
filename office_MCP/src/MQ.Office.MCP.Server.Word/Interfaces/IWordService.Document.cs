namespace MQ.Office.MCP.Server.Word;

public partial interface IWordService
{
    /// <summary>
    /// Connects to a running Word instance or starts a new one, then opens the
    /// document at <paramref name="documentPath"/>, or creates a new empty
    /// document when <paramref name="documentPath"/> is null or empty.
    /// Returns the document name.
    /// </summary>
    string OpenDocument(string? documentPath = null);

    /// <summary>
    /// Saves the currently active Word document. If <paramref name="saveAsPath"/>
    /// is provided, performs a Save As to that path instead.
    /// </summary>
    string SaveDocument(string? saveAsPath = null);

    /// <summary>
    /// Saves (optionally) and closes the currently active Word document.
    /// </summary>
    void CloseDocument(bool save = true);
}

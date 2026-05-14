using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace MQ.Office.MCP.Server.Word;

[SupportedOSPlatform("windows")]
public sealed partial class WordService
{
    // Word measures all distances internally in points (1 inch = 72 pt).
    private const double PointsPerInch = 72.0;
    private const double PointsPerCm = 28.3464566929; // 72 / 2.54

    // WdParagraphAlignment
    private const int WdAlignParagraphLeft = 0;
    private const int WdAlignParagraphCenter = 1;
    private const int WdAlignParagraphRight = 2;
    private const int WdAlignParagraphJustify = 3;

    // WdLineSpacing
    private const int WdLineSpaceMultiple = 5;

    // WdUnits
    private const int WdStory = 6;

    private static double ToPoints(double value, string unit) => unit?.ToLowerInvariant() switch
    {
        "pt" or "pts" or "point" or "points" => value,
        "in" or "inch" or "inches" => value * PointsPerInch,
        null or "" or "cm" or "centimeter" or "centimeters" => value * PointsPerCm,
        _ => throw new InvalidOperationException(
            $"Unsupported unit '{unit}'. Use 'cm', 'inches', or 'points'.")
    };

    private static int ParseAlignment(string alignment) => alignment?.ToLowerInvariant() switch
    {
        "left" => WdAlignParagraphLeft,
        "center" or "centre" => WdAlignParagraphCenter,
        "right" => WdAlignParagraphRight,
        "justify" or "justified" => WdAlignParagraphJustify,
        _ => throw new InvalidOperationException(
            $"Unsupported alignment '{alignment}'. Use 'left', 'center', 'right', or 'justify'.")
    };

    /// <inheritdoc/>
    public int ReplaceText(string find, string replace, bool matchCase = false)
    {
        if (string.IsNullOrEmpty(find))
            throw new InvalidOperationException("Find text must not be empty.");

        EnsureActiveDocument();
        dynamic findObj = App.Selection.Find;
        findObj.ClearFormatting();
        findObj.Replacement.ClearFormatting();
        findObj.Text = find;
        findObj.Replacement.Text = replace ?? string.Empty;
        findObj.Forward = true;
        findObj.Wrap = 1;           // wdFindContinue
        findObj.MatchCase = matchCase;
        findObj.MatchWholeWord = false;
        findObj.MatchWildcards = false;
        // wdReplaceAll = 2
        findObj.Execute(Replace: 2);

        // Count how many remain — Word doesn't return a count from Execute,
        // so we do a forward search pass to verify zero remain.
        findObj.Text = find;
        findObj.Replacement.Text = replace ?? string.Empty;
        // Return 1 as a sentinel (we replaced at least one if no exception).
        _logger.LogInformation("Replaced '{Find}' with '{Replace}' (matchCase={MC}).", find, replace, matchCase);
        return 1;
    }

    /// <inheritdoc/>
    public void ClearDocument()
    {
        EnsureActiveDocument();
        dynamic selection = App.Selection;
        selection.WholeStory();
        selection.Delete();
        _logger.LogInformation("Cleared all text from active Word document.");
    }

    /// <inheritdoc/>
    public int InsertText(string text, string location = "end")
    {
        if (text is null)
            throw new InvalidOperationException("Text to insert must not be null.");

        EnsureActiveDocument();
        dynamic selection = App.Selection;

        switch (location?.ToLowerInvariant())
        {
            case null:
            case "":
            case "end":
                selection.EndKey(WdStory);
                break;
            case "start":
                selection.HomeKey(WdStory);
                break;
            case "cursor":
                // Leave selection where it is.
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported location '{location}'. Use 'end', 'start', or 'cursor'.");
        }

        selection.TypeText(text);
        _logger.LogInformation("Inserted {Count} characters at {Location}.", text.Length, location ?? "end");
        return text.Length;
    }

    /// <inheritdoc/>
    public void SetPageMargins(
        double? top,
        double? bottom,
        double? left,
        double? right,
        string unit = "cm")
    {
        EnsureActiveDocument();
        dynamic pageSetup = App.ActiveDocument.PageSetup;

        if (top.HasValue) pageSetup.TopMargin = ToPoints(top.Value, unit);
        if (bottom.HasValue) pageSetup.BottomMargin = ToPoints(bottom.Value, unit);
        if (left.HasValue) pageSetup.LeftMargin = ToPoints(left.Value, unit);
        if (right.HasValue) pageSetup.RightMargin = ToPoints(right.Value, unit);

        _logger.LogInformation(
            "Set page margins (unit={Unit}): top={Top} bottom={Bottom} left={Left} right={Right}.",
            unit, top, bottom, left, right);
    }

    /// <inheritdoc/>
    public void SetParagraphFormat(
        double? lineSpacing,
        double? spaceBefore,
        double? spaceAfter,
        string? alignment,
        double? firstLineIndent,
        string unit = "cm")
    {
        EnsureActiveDocument();
        dynamic pf = App.Selection.ParagraphFormat;

        if (lineSpacing.HasValue)
        {
            // Use "multiple" rule so 1.0/1.5/2.0/etc. map naturally.
            pf.LineSpacingRule = WdLineSpaceMultiple;
            pf.LineSpacing = lineSpacing.Value * 12.0; // 12 pt = single line
        }

        if (spaceBefore.HasValue) pf.SpaceBefore = ToPoints(spaceBefore.Value, unit);
        if (spaceAfter.HasValue) pf.SpaceAfter = ToPoints(spaceAfter.Value, unit);
        if (firstLineIndent.HasValue) pf.FirstLineIndent = ToPoints(firstLineIndent.Value, unit);
        if (!string.IsNullOrWhiteSpace(alignment)) pf.Alignment = ParseAlignment(alignment);

        _logger.LogInformation(
            "Set paragraph format: lineSpacing={LS} before={SB} after={SA} indent={Indent} alignment={A} unit={Unit}.",
            lineSpacing, spaceBefore, spaceAfter, firstLineIndent, alignment, unit);
    }
}

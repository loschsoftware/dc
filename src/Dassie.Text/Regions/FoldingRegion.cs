namespace Dassie.Text.Regions;

/// <summary>
/// A region of code that can be folded by an IDE.
/// </summary>
public class FoldingRegion
{
    /// <summary>
    /// The line of code starting the region.
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// The column of code starting the region.
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// The line of code ending the region.
    /// </summary>
    public int EndLine { get; set; }

    private int _endColumn;
    /// <summary>
    /// The column of code ending the region.
    /// </summary>
    public int EndColumn
    {
        get => _endColumn;
        set => _endColumn = value + 2; // To appease AvalonEdit
    }
}
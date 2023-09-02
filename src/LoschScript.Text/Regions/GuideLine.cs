namespace LoschScript.Text.Regions;

/// <summary>
/// Used to represent a structure guide line in an IDE.
/// </summary>
public class GuideLine
{
    /// <summary>
    /// The line where the guide line starts.
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// The line where the guide line ends.
    /// </summary>
    public int EndLine { get; set; }

    /// <summary>
    /// The column of the guide line.
    /// </summary>
    public int Column { get; set; }
}
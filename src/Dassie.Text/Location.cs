namespace Dassie.Text;

/// <summary>
/// Represents a code location.
/// </summary>
public class Location
{
    /// <summary>
    /// The file of the location.
    /// </summary>
    public string File { get; set; }

    /// <summary>
    /// The code line of the location.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// The column of the first character of the location.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// The length of the location.
    /// </summary>
    public int Length { get; set; }
}
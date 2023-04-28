namespace LoschScript.Errors;

/// <summary>
/// Represents an error in the code.
/// </summary>
public class ErrorInfo
{
    /// <summary>
    /// The kind of error that the current <see cref="ErrorInfo"/> instance represents.
    /// </summary>
    public ErrorKind ErrorCode { get; set; }

    /// <summary>
    /// The error message as emitted by the compiler.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// The path to the source file the error occured in.
    /// </summary>
    public string File { get; set; }
    
    /// <summary>
    /// The starting position of the error in the file. The first tuple item represents the row, the second the column of the error.
    /// </summary>
    public (int, int) CodePosition { get; set; }

    /// <summary>
    /// The end position of the error in the file. The first tuple item represents the row, the second the column of the error.
    /// </summary>
    public (int, int) CodeEndPosition { get; set; }
}
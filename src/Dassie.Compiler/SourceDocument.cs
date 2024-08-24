namespace Dassie.Compiler;

/// <summary>
/// Represents a document containing Dassie source code.
/// </summary>
public class SourceDocument
{
    /// <summary>
    /// The actual source code.
    /// </summary>
    public string SourceText { get; }

    /// <summary>
    /// The name associated with the document, displayed in error messages.
    /// </summary>
    public string SymbolicName { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="SourceDocument"/> type.
    /// </summary>
    /// <param name="text">The source code of the document.</param>
    /// <param name="symbolicName">The name of the document.</param>
    public SourceDocument(string text, string symbolicName)
    {
        SourceText = text;
        SymbolicName = symbolicName;
    }
}
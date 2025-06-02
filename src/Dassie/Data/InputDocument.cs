namespace Dassie.Data;

/// <summary>
/// Represents a logical "document" of Dassie source code. Most commonly, this corresponds to a file in a file system.
/// </summary>
/// <param name="Text">The source code content of the document.</param>
/// <param name="Name">The name of the document, most commonly a file name.</param>
internal record InputDocument(string Text, string Name);
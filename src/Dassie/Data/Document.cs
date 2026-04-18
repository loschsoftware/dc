using System.Security.Cryptography;
using System.Text;

namespace Dassie.Data;

/// <summary>
/// Represents the type of a document.
/// </summary>
public enum DocumentKind
{
    /// <summary>
    /// Dassie source code.
    /// </summary>
    SourceText,
    /// <summary>
    /// A Dassie script.
    /// </summary>
    Script,
    /// <summary>
    /// An embedded resource.
    /// </summary>
    Resource,
    /// <summary>
    /// An auxiliary document used during the compilation.
    /// </summary>
    Auxiliary,
    /// <summary>
    /// Raw data not otherwise classified.
    /// </summary>
    Data
}

/// <summary>
/// Represents a logical document of data, such as Dassie source code.
/// Most commonly, this corresponds to a file in a file system.
/// </summary>
public record Document
{
    private readonly string _path;
    private readonly byte[] _data;
    private byte[] _hash;

    /// <summary>
    /// The hash of document data.
    /// </summary>
    public byte[] Hash => _hash ??= SHA256.HashData(_data);

    /// <summary>
    /// The identifier of the document, most commonly a file name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The file path of the document, if available.
    /// </summary>
    public string FilePath => _path ?? Name;

    /// <summary>
    /// The type of the document.
    /// </summary>
    public DocumentKind Kind { get; }

    /// <summary>
    /// The text of the document.
    /// </summary>
    public string Text => Encoding.Unicode.GetString(_data);

    /// <summary>
    /// The raw data of the document.
    /// </summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>
    /// Creates a new instance of the <see cref="Document"/> type.
    /// </summary>
    /// <param name="text">The text of the document.</param>
    /// <param name="name">The identifier of the document.</param>
    public Document(string text, string name)
        : this(text, name, DocumentKind.SourceText) { }

    /// <summary>
    /// Creates a new instance of the <see cref="Document"/> type.
    /// </summary>
    /// <param name="text">The text of the document.</param>
    /// <param name="name">The identifier of the document.</param>
    /// <param name="path">The file path of the document.</param>
    public Document(string text, string name, string path)
        : this(text, name, DocumentKind.SourceText, path) { }

    /// <summary>
    /// Creates a new instance of the <see cref="Document"/> type.
    /// </summary>
    /// <param name="text">The text of the document.</param>
    /// <param name="name">The identifier of the document.</param>
    /// <param name="kind">The type of the document.</param>
    public Document(string text, string name, DocumentKind kind)
        : this(name, Encoding.Unicode.GetBytes(text), kind) { }

    /// <summary>
    /// Creates a new instance of the <see cref="Document"/> type.
    /// </summary>
    /// <param name="text">The text of the document.</param>
    /// <param name="name">The identifier of the document.</param>
    /// <param name="kind">The type of the document.</param>
    /// <param name="path">The file path of the document.</param>
    public Document(string text, string name, DocumentKind kind, string path)
        : this(name, Encoding.Unicode.GetBytes(text), kind, path) { }

    /// <summary>
    /// Creates a new instance of the <see cref="Document"/> type.
    /// </summary>
    /// <param name="name">The identifier of the document.</param>
    /// <param name="data">The data of the document.</param>
    /// <param name="kind">The type of the document.</param>
    public Document(string name, byte[] data, DocumentKind kind)
        : this(name, data, kind, null) { }

    /// <summary>
    /// Creates a new instance of the <see cref="Document"/> type.
    /// </summary>
    /// <param name="name">The identifier of the document.</param>
    /// <param name="data">The data of the document.</param>
    /// <param name="kind">The type of the document.</param>
    /// <param name="path">The file path of the document.</param>
    public Document(string name, byte[] data, DocumentKind kind, string path)
    {
        if (!string.IsNullOrEmpty(path))
            _path = path;

        Name = name;
        _data = data;
        Kind = kind;
    }
}
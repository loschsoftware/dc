using System.Collections.Generic;
using System.Xml;

namespace Dassie.Extensions;

/// <summary>
/// Represents a document source that can inject Dassie source code into compilations.
/// </summary>
public interface IDocumentSource
{
    /// <summary>
    /// The name of the document source.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The name of the document this source represents.
    /// </summary>
    public string DocumentName { get; set; }

    /// <summary>
    /// The method called when the document source is called.
    /// </summary>
    /// <param name="attributes">The XML attributes passed to the document source.</param>
    /// <param name="elements">The XML elements passed to the document source.</param>
    /// <returns>The Dassie source code this document source represents.</returns>
    public string GetText(List<XmlAttribute> attributes, List<XmlNode> elements);
}
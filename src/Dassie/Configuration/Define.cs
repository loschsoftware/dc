using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a macro definition.
/// </summary>
[XmlRoot("Define")]
[Serializable]
public class Define
{
    /// <summary>
    /// Specifies the name of the macro.
    /// </summary>
    [XmlAttribute("Macro")]
    public string Name { get; set; }

    /// <summary>
    /// The parameter list of the macro.
    /// </summary>
    [XmlAttribute]
    public string Parameters { get; set; }

    /// <summary>
    /// Specifies wheter or not to trim leading and trailing whitespace of the expanded value.
    /// </summary>
    [XmlAttribute]
    public bool Trim { get; set; }

    /// <summary>
    /// The string this macro expands to.
    /// </summary>
    [XmlText]
    public string Value { get; set; }
}
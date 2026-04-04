using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a macro definition.
/// </summary>
[XmlRoot]
[Serializable]
public partial class Define : ConfigObject
{
    /// <inheritdoc/>
    public Define(PropertyStore store) : base(store) { }

    /// <summary>
    /// Specifies the name of the macro.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial string Macro { get; set; }

    /// <summary>
    /// The parameter list of the macro.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial string Parameters { get; set; }

    /// <summary>
    /// Specifies wheter or not to trim leading and trailing whitespace of the expanded value.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial bool Trim { get; set; }

    /// <summary>
    /// The string this macro expands to.
    /// </summary>
    [XmlText]
    [ConfigProperty]
    public partial string Value { get; set; }
}
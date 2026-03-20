using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a macro definition.
/// </summary>
[XmlRoot("Define")]
[Serializable]
public class Define : ConfigObject
{
    /// <inheritdoc/>
    public Define(PropertyStore store) : base(store) { }

    /// <summary>
    /// Specifies the name of the macro.
    /// </summary>
    [XmlAttribute("Macro")]
    public string Name
    {
        get => Get<string>(nameof(Name));
        set => Set(nameof(Name), value);
    }

    /// <summary>
    /// The parameter list of the macro.
    /// </summary>
    [XmlAttribute]
    public string Parameters
    {
        get => Get<string>(nameof(Parameters));
        set => Set(nameof(Parameters), value);
    }

    /// <summary>
    /// Specifies wheter or not to trim leading and trailing whitespace of the expanded value.
    /// </summary>
    [XmlAttribute]
    public bool Trim
    {
        get => Get<bool>(nameof(Trim));
        set => Set(nameof(Trim), value);
    }

    /// <summary>
    /// The string this macro expands to.
    /// </summary>
    [XmlText]
    public string Value
    {
        get => Get<string>(nameof(Value));
        set => Set(nameof(Value), value);
    }
}
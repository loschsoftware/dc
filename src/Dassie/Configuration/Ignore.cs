using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents an ignored message.
/// </summary>
[XmlRoot]
[Serializable]
public class Ignore : ConfigObject
{
    /// <inheritdoc/>
    public Ignore(PropertyStore store) : base(store) { }

    /// <summary>
    /// The message code to ignore.
    /// </summary>
    [XmlText]
    public string Code
    {
        get => Get<string>(nameof(Code));
        set => Set(nameof(Code), value);
    }
}

/// <summary>
/// Represents an ignored message.
/// </summary>
[XmlRoot]
[Serializable]
public class Message : Ignore
{
    /// <inheritdoc/>
    public Message(PropertyStore store) : base(store) { }
}

/// <summary>
/// Represents an ignored warning.
/// </summary>
[XmlRoot]
[Serializable]
public class Warning : Ignore
{
    /// <inheritdoc/>
    public Warning(PropertyStore store) : base(store) { }
}
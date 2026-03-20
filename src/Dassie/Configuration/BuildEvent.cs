using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a build event.
/// </summary>
[Serializable]
[XmlRoot]
public class BuildEvent : ConfigObject
{
    /// <inheritdoc/>
    public BuildEvent(PropertyStore store) : base(store) { }

    /// <summary>
    /// The name of the build event.
    /// </summary>
    [DefaultValue("")]
    [XmlAttribute]
    public string Name
    {
        get => Get<string>(nameof(Name));
        set => Set(nameof(Name), value);
    }

    /// <summary>
    /// Specifies wheter or not the build event is critical.
    /// A critical build event will cause a build process to terminate immediately in case of failure.
    /// </summary>
    [DefaultValue(true)]
    [XmlAttribute]
    public bool Critical
    {
        get => Get<bool>(nameof(Critical));
        set => Set(nameof(Critical), value);
    }

    /// <summary>
    /// A list of XML elements representing the actions to execute as part of the build event.
    /// </summary>
    [XmlAnyElement]
    public List<XmlElement> CommandNodes
    {
        get => Get<List<XmlElement>>(nameof(CommandNodes));
        set => Set(nameof(CommandNodes), value);
    }
}
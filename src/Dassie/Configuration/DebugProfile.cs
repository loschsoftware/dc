using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a debug profile.
/// </summary>
[Serializable]
[XmlRoot]
public class DebugProfile : ConfigObject
{
    /// <inheritdoc/>
    public DebugProfile(PropertyStore store) : base(store) { }

    /// <summary>
    /// The name of the profile.
    /// </summary>
    [XmlAttribute]
    public string Name
    {
        get => Get<string>(nameof(Name));
        set => Set(nameof(Name), value);
    }

    /// <summary>
    /// The command-line arguments to pass to the application being debugged.
    /// </summary>
    [XmlElement]
    public string Arguments
    {
        get => Get<string>(nameof(Arguments));
        set => Set(nameof(Arguments), value);
    }

    /// <summary>
    /// The working directory to start the application to debug in.
    /// </summary>
    [XmlElement]
    public string WorkingDirectory
    {
        get => Get<string>(nameof(WorkingDirectory));
        set => Set(nameof(WorkingDirectory), value);
    }
}
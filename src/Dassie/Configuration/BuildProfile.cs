using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a build profile.
/// </summary>
[XmlRoot]
[Serializable]
public class BuildProfile : ConfigObject
{
    /// <inheritdoc/>
    public BuildProfile(PropertyStore store) : base(store) { }

    /// <summary>
    /// The name of the build profile.
    /// </summary>
    [XmlAttribute]
    public string Name
    {
        get => Get<string>(nameof(Name));
        set => Set(nameof(Name), value);
    }

    /// <summary>
    /// The command-line arguments passed to the compiler when the build event is executed.
    /// </summary>
    [XmlElement]
    public string Arguments
    {
        get => Get<string>(nameof(Arguments));
        set => Set(nameof(Arguments), value);
    }

    /// <summary>
    /// Compiler configuration properties to specify for the build.
    /// </summary>
    [XmlElement]
    public DassieConfig Settings
    {
        get => Get<DassieConfig>(nameof(Settings));
        set => Set(nameof(Settings), value);
    }

    /// <summary>
    /// An array of pre-build events.
    /// </summary>
    [XmlArray]
    public BuildEvent[] PreBuildEvents
    {
        get => Get<BuildEvent[]>(nameof(PreBuildEvents));
        set => Set(nameof(PreBuildEvents), value);
    }

    /// <summary>
    /// An array of post-build events.
    /// </summary>
    [XmlArray]
    public BuildEvent[] PostBuildEvents
    {
        get => Get<BuildEvent[]>(nameof(PostBuildEvents));
        set => Set(nameof(PostBuildEvents), value);
    }
}
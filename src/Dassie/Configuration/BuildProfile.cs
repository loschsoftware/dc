using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a build profile.
/// </summary>
[XmlRoot]
[Serializable]
public partial class BuildProfile : ConfigObject
{
    /// <inheritdoc/>
    public BuildProfile(PropertyStore store) : base(store) { }

    /// <summary>
    /// The name of the build profile.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial string Name { get; set; }

    /// <summary>
    /// The command-line arguments passed to the compiler when the build event is executed.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string Arguments { get; set; }

    /// <summary>
    /// Compiler configuration properties to specify for the build.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial DassieConfig Settings { get; set; }

    /// <summary>
    /// An array of pre-build events.
    /// </summary>
    [XmlArray]
    [ConfigProperty]
    public partial BuildEvent[] PreBuildEvents { get; set; }

    /// <summary>
    /// An array of post-build events.
    /// </summary>
    [XmlArray]
    [ConfigProperty]
    public partial BuildEvent[] PostBuildEvents { get; set; }
}
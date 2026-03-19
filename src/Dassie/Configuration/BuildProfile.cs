using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a build profile.
/// </summary>
[XmlRoot]
[Serializable]
public class BuildProfile
{
    /// <summary>
    /// The name of the build profile.
    /// </summary>
    [XmlAttribute]
    public string Name { get; set; }

    /// <summary>
    /// The command-line arguments passed to the compiler when the build event is executed.
    /// </summary>
    [XmlElement]
    public string Arguments { get; set; }

    /// <summary>
    /// Compiler configuration properties to specify for the build.
    /// </summary>
    [XmlElement]
    public DassieConfig Settings { get; set; }

    /// <summary>
    /// An array of pre-build events.
    /// </summary>
    [XmlArray]
    public BuildEvent[] PreBuildEvents { get; set; }

    /// <summary>
    /// An array of post-build events.
    /// </summary>
    [XmlArray]
    public BuildEvent[] PostBuildEvents { get; set; }
}
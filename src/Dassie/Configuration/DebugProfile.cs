using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a debug profile.
/// </summary>
[Serializable]
[XmlRoot]
public partial class DebugProfile : ConfigObject
{
    /// <inheritdoc/>
    public DebugProfile(PropertyStore store) : base(store) { }

    /// <summary>
    /// The name of the profile.
    /// </summary>
    [XmlAttribute]
    [ConfigProperty]
    public partial string Name { get; set; }

    /// <summary>
    /// The command-line arguments to pass to the application being debugged.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string Arguments { get; set; }

    /// <summary>
    /// The working directory to start the application to debug in.
    /// </summary>
    [XmlElement]
    [ConfigProperty]
    public partial string WorkingDirectory { get; set; }
}
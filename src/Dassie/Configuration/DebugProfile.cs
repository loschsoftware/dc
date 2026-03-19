using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a debug profile.
/// </summary>
[Serializable]
[XmlRoot]
public class DebugProfile
{
    /// <summary>
    /// The name of the profile.
    /// </summary>
    [XmlAttribute]
    public string Name { get; set; }

    /// <summary>
    /// The command-line arguments to pass to the application being debugged.
    /// </summary>
    [XmlElement]
    public string Arguments { get; set; }

    /// <summary>
    /// The working directory to start the application to debug in.
    /// </summary>
    [XmlElement]
    public string WorkingDirectory { get; set; }
}
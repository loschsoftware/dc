using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents the serialized data of the tools.xml file in the application data directory of Dassie.
/// </summary>
[Serializable]
[XmlRoot("CompilerTools")]
public class ToolPaths
{
    /// <summary>
    /// The path to the tools.xml file.
    /// </summary>
    public static string ToolPathsFile { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "tools.xml");

    /// <summary>
    /// The saved external tools.
    /// </summary>
    [XmlElement]
    public Tool[] Tools { get; set; }
}

/// <summary>
/// Represents an external component used by the Dassie compiler for certain operations.
/// </summary>
[Serializable]
[XmlRoot("Tool")]
public class Tool
{
    /// <summary>
    /// The file name of the tool.
    /// </summary>
    [XmlAttribute]
    public string Name { get; set; }

    /// <summary>
    /// The full path to the tool.
    /// </summary>
    [XmlText]
    public string Path { get; set; }
}
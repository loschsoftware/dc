using Dassie.Errors;
using System.Collections.Generic;
using System.Xml;

namespace Dassie.Extensions;

/// <summary>
/// Defines a mechanism for redirecting Dassie compiler logs to different outputs.
/// </summary>
public interface IBuildLogDevice
{
    /// <summary>
    /// The name of the build log device.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The method called when the build log device is initialized.
    /// </summary>
    /// <param name="attributes">The XML attributes passed to the log device.</param>
    /// <param name="elements">The XML elements passed to the log device.</param>
    public void Initialize(List<XmlAttribute> attributes, List<XmlNode> elements);

    /// <summary>
    /// The method called when a compiler message is emitted.
    /// </summary>
    /// <param name="error">The compiler message that was emitted.</param>
    public void Log(ErrorInfo error);

    /// <summary>
    /// The method called when a string is written to the message output.
    /// </summary>
    /// <param name="input">The string to write.</param>
    public void WriteString(string input);
}
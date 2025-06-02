using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Dassie.Errors.Devices;

/// <summary>
/// Provides a build log device for logging to files.
/// </summary>
internal class FileBuildLogDevice : IBuildLogDevice, IDisposable
{
    public string Name => "File";
    public BuildLogSeverity SeverityLevel => BuildLogSeverity.All;

    private StreamWriter sw;

    /// <inheritdoc/>
    public void Initialize(List<XmlAttribute> attributes, List<XmlNode> elements)
    {
        if (!attributes.Any(a => a.Name == "Path"))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0171_FileBuildLogDeviceNoPathSpecified,
                $"'Path' attribute is required for <File> build log device.",
                ProjectConfigurationFileName);

            return;
        }

        string path = attributes.First(a => a.Name == "Path").Value;
        sw = new(path);

        TextWriterBuildLogDevice.InfoOut.AddWriter(sw);
        TextWriterBuildLogDevice.WarnOut.AddWriter(sw);
        TextWriterBuildLogDevice.ErrorOut.AddWriter(sw);
    }

    // No logic here, since it is handled by TextWriterBuildLogDevice

    /// <inheritdoc/>
    public void Log(ErrorInfo error) { }
    /// <inheritdoc/>
    public void WriteString(string input) { }

    /// <inheritdoc/>
    public void Dispose()
    {
        sw?.Dispose();
    }
}
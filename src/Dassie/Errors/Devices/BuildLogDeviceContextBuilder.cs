using Dassie.Configuration;
using Dassie.Configuration.Analysis;
using Dassie.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Dassie.Errors.Devices;

/// <summary>
/// Provides methods for instantiating and registering build log devices.
/// </summary>
internal static class BuildLogDeviceContextBuilder
{
    /// <summary>
    /// Instantiates and registers all build log devices enabled in the specified configuration file.
    /// </summary>
    /// <param name="config">A <see cref="DassieConfig"/> object representing a deserialized configuration file.</param>
    /// <param name="configPath">The file path to the configuration file, used for error messages.</param>
    public static void RegisterBuildLogDevices(DassieConfig config, string configPath)
    {
        IEnumerable<IBuildLogDevice> availableDevices = ExtensionLoader.BuildLogDevices;
        List<IBuildLogDevice> devices = [TextWriterBuildLogDevice.Instance];

        if (config.BuildLogOptions == null)
            return;

        foreach (XmlElement element in config.BuildLogOptions.Elements)
        {
            var loc = XmlLocationService.GetElementLocation(configPath, "BuildLogDevices");
            string deviceName = element.Name;

            if (!availableDevices.Any(d => d.Name == deviceName))
            {
                EmitWarningMessage(
                    loc.Row,
                    loc.Column,
                    loc.Length,
                    DS0170_InvalidBuildDeviceName,
                    $"The specified build log device '{deviceName}' is not installed.",
                    ProjectConfigurationFileName);

                continue;
            }

            IBuildLogDevice device = availableDevices.First(d => d.Name == deviceName);
            IEnumerable<XmlAttribute> attribs = element.Attributes.Cast<XmlAttribute>();
            IEnumerable<XmlNode> nodes = element.ChildNodes.Cast<XmlNode>();

            device.Initialize(attribs.ToList(), nodes.ToList());
            devices.Add(device);
        }

        BuildLogDevices = devices;
    }
}
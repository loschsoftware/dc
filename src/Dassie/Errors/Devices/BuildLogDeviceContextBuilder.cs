using Dassie.Configuration;
using Dassie.Configuration.Analysis;
using Dassie.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Dassie.Errors.Devices;

internal static class BuildLogDeviceContextBuilder
{
    public static void RegisterBuildLogDevices(DassieConfig config, string configPath)
    {
        IEnumerable<IBuildLogDevice> availableDevices = ExtensionLoader.BuildLogDevices;
        List<IBuildLogDevice> devices = [TextWriterBuildLogDevice.Instance];

        if (config.BuildLogOptions == null)
            return;

        foreach (XmlElement element in config.BuildLogOptions.Elements)
        {
            string deviceName = element.Name;

            if (!availableDevices.Any(d => d.Name == deviceName))
            {
                var loc = XmlLocationService.GetElementLocation(configPath, "BuildLogDevices");
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
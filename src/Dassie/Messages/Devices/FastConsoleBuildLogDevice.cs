using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Dassie.Messages.Devices;

/// <summary>
/// Provides a singleton build log device for performant error logging to the console.
/// </summary>
internal class FastConsoleBuildLogDevice : IBuildLogDevice
{
    public string Name => "FastConsole";
    public BuildLogSeverity SeverityLevel => BuildLogSeverity.All;

    public void Initialize(List<XmlAttribute> attributes, List<XmlNode> elements)
    {
        BuildLogDevices.RemoveAll(b => b.Name == "Default");
    }

    public void Log(MessageInfo error)
    {
        string msg = error.ToString();

        if (error.Severity == Severity.Error)
            Console.Error.Write(msg);
        else
            Console.Write(msg);
    }

    public void WriteString(string input) => Console.Write(input);
}

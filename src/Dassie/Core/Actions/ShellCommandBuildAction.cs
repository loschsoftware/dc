using Dassie.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using SDProcess = System.Diagnostics.Process;

namespace Dassie.Core.Actions;

internal class ShellCommandBuildAction : IBuildAction
{
    public string Name => "ShellCommand";
    public ActionModes SupportedModes => ActionModes.All;

    public int Execute(ActionContext context)
    {
        bool elevate = false;
        bool waitForExit = false;
        bool hidden = false;

        if (context.XmlAttributes is not null and not [])
        {
            string[] knownProperties = ["RunAsAdministrator", "WaitForExit", "Hidden"];

            foreach (XmlAttribute attrib in context.XmlAttributes)
            {
                if (!knownProperties.Contains(attrib.Name))
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0090_InvalidDSConfigProperty,
                        $"Build event '{context.BuildEventName}': Invalid property '{attrib.Name}'.",
                        ProjectConfigurationFileName);

                    continue;
                }

                if (!bool.TryParse(attrib.Value, out bool value))
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0090_InvalidDSConfigProperty,
                        $"Build event '{context.BuildEventName}': Invalid value for option '{attrib.Name}'. Expected 'true' or 'false'.",
                        ProjectConfigurationFileName);

                    continue;
                }

                if (attrib.Name == knownProperties[0])
                    elevate = value;
                else if (attrib.Name == knownProperties[1])
                    waitForExit = value;
                else
                    hidden = value;
            }
        }

        if (string.IsNullOrWhiteSpace(context.Text))
            return 0;

        ProcessStartInfo psi = new()
        {
            UseShellExecute = false,
            CreateNoWindow = hidden
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            psi.FileName = Environment.GetEnvironmentVariable("COMSPEC") ?? "cmd.exe";
            psi.Arguments = "/c " + context.Text;

            psi.WindowStyle = hidden
                ? ProcessWindowStyle.Hidden
                : ProcessWindowStyle.Normal;

            if (elevate)
            {
                psi.UseShellExecute = true;
                psi.Verb = "runas";
            }
        }
        else
        {
            psi.FileName = "/bin/bash";
            psi.Arguments = "-c \"" + context.Text.Replace("\"", "\\\"") + "\"";
        }
        
        using SDProcess proc = SDProcess.Start(psi);

        if (waitForExit)
        {
            proc.WaitForExit();
            return proc.ExitCode;
        }

        return 0;
    }
}
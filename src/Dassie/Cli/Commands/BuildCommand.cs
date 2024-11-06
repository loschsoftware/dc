using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Errors;
using Dassie.Extensions;
using Dassie.Validation;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Dassie.Cli.Commands;

internal class BuildCommand : ICompilerCommand
{
    private static BuildCommand _instance;
    public static BuildCommand Instance => _instance ??= new();

    public string Command => "build";

    public string UsageString => "build [BuildProfile]";

    public string Description => "Executes the specified build profile, or compiles all .ds source files in the current directory if none is specified.";

    public int Invoke(string[] args)
    {
        DassieConfig config = null;

        if (File.Exists(ProjectConfigurationFileName))
        {
            foreach (ErrorInfo error in ConfigValidation.Validate(ProjectConfigurationFileName))
                EmitGeneric(error);

            XmlSerializer xmls = new(typeof(DassieConfig));
            using StreamReader sr = new(ProjectConfigurationFileName);
            config = (DassieConfig)xmls.Deserialize(sr);
        }

        config ??= new();
        config.BuildProfiles ??= [];

        MacroParser parser = new();
        parser.ImportMacros(MacroGenerator.GenerateMacrosForProject(config));
        parser.Normalize(config);

        if (args.Length > 0 && args.TakeWhile(a => !a.StartsWith('-')).Any())
        {
            string profileName = args.First(a => !a.StartsWith('-'));

            if (config.BuildProfiles.Any(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase)))
            {
                BuildProfile profile = config.BuildProfiles.First(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
                return ExecuteBuildProfile(profile, config);
            }

            EmitErrorMessage(
                0, 0, 0,
                DS0087_InvalidProfile,
                $"The build profile '{profileName}' could not be found.",
                ProjectConfigurationFileName);

            return -1;
        }
        else if (config.BuildProfiles.Any(p => p.Name.ToLowerInvariant() == "default"))
            return ExecuteBuildProfile(config.BuildProfiles.First(p => p.Name.ToLowerInvariant() == "default"), config);

        string[] filesToCompile = Directory.EnumerateFiles(".\\", "*.ds", SearchOption.AllDirectories).ToArray();
        filesToCompile = filesToCompile.Where(f => Path.GetDirectoryName(f).Split(Path.DirectorySeparatorChar).Last() != TemporaryBuildDirectoryName).ToArray();

        if (filesToCompile.Length < 1)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0072_NoSourceFilesFound,
                "No source files present.",
                "build");

            return -1;
        }

        return CompileCommand.Instance.Invoke(filesToCompile.Concat(args).ToArray());
    }

    private static int ExecuteBuildProfile(BuildProfile profile, DassieConfig config)
    {
        if (profile.Settings != null)
        {
            foreach (PropertyInfo property in profile.Settings.GetType().GetProperties())
            {
                object val = property.GetValue(profile.Settings);

                if (val != null)
                    config.GetType().GetProperty(property.Name).SetValue(config, val);
            }
        }

        if (profile.PreBuildEvents != null && profile.PreBuildEvents.Any())
        {
            foreach (BuildEvent preEvent in profile.PreBuildEvents)
            {
                if (string.IsNullOrEmpty(preEvent.Command))
                    continue;

                ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden;

                if (!preEvent.Hidden)
                    windowStyle = ProcessWindowStyle.Normal;

                ProcessStartInfo psi = new()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {preEvent.Command}",
                    CreateNoWindow = false,
                    WindowStyle = windowStyle
                };

                if (preEvent.RunAsAdministrator)
                    psi.Verb = "runas";

                Process proc = Process.Start(psi);

                if (preEvent.WaitForExit)
                    proc.WaitForExit();

                string errMsg = $"The command '{preEvent.Command}' ended with a non-zero exit code.";

                if (preEvent.WaitForExit)
                {
                    if (proc.ExitCode != 0 && preEvent.Critical)
                    {
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0087_InvalidProfile,
                            errMsg,
                            ProjectConfigurationFileName);

                        return -1;
                    }
                    else if (proc.ExitCode != 0)
                    {
                        EmitWarningMessage(
                            0, 0, 0,
                            DS0087_InvalidProfile,
                            errMsg,
                            ProjectConfigurationFileName);
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(profile.Arguments))
            CompileCommand.Instance.Invoke(profile.Arguments.Split(' '), config);

        if (profile.PostBuildEvents != null && profile.PostBuildEvents.Any())
        {
            foreach (BuildEvent postEvent in profile.PostBuildEvents)
            {
                if (string.IsNullOrEmpty(postEvent.Command))
                    continue;

                ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden;

                if (!postEvent.Hidden)
                    windowStyle = ProcessWindowStyle.Normal;

                ProcessStartInfo psi = new()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {postEvent.Command}",
                    CreateNoWindow = false,
                    WindowStyle = windowStyle
                };

                if (postEvent.RunAsAdministrator)
                    psi.Verb = "runas";

                Process proc = Process.Start(psi);

                if (postEvent.WaitForExit)
                    proc.WaitForExit();

                string errMsg = $"The command '{postEvent.Command}' ended with a non-zero exit code.";

                if (postEvent.WaitForExit)
                {
                    if (proc.ExitCode != 0 && postEvent.Critical)
                    {
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0087_InvalidProfile,
                            errMsg,
                            ProjectConfigurationFileName);

                        return -1;
                    }
                    else if (proc.ExitCode != 0)
                    {
                        EmitWarningMessage(
                            0, 0, 0,
                            DS0087_InvalidProfile,
                            errMsg,
                            ProjectConfigurationFileName);
                    }
                }
            }
        }

        return 0;
    }
}
using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dassie.Cli.Commands;

internal class BuildCommand : ICompilerCommand
{
    private static BuildCommand _instance;
    public static BuildCommand Instance => _instance ??= new();

    public string Command => "build";

    public string Description => "Executes the specified build profile, or compiles all source files in the current folder structure if none is specified.";

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = Description,
        Usage = ["dc build [BuildProfile] [Options]"],
        Remarks = "This is the primary command for building Dassie projects. By default, this command will compile all Dassie source files in the current directory as well as all subdirectories. If no project file is present in the root directory, the default configuration is used.",
        Options =
        [
            ("BuildProfile", "Specifies the build profile to execute. If not set, the default profile is executed."),
            ("Options", "Additional options to pass to the compiler. For a list of available options, use 'dc help -o'.")
        ],
        Examples =
        [
            ("dc build", "Builds the current project with the default build profile."),
            ("dc build CustomProfile", "Builds the current project with the 'CustomProfile' build profile."),
            ("dc build CustomProfile -r Aot", "Builds the current project with the 'CustomProfile' build profile using the AOT compiler.")
        ]
    };

    public int Invoke(string[] args)
        => Invoke(args, null);

    internal int Invoke(string[] args, DassieConfig overrideConfig)
    {
        DassieConfig config = ProjectFileDeserializer.DassieConfig;

        if (overrideConfig != null)
            config = overrideConfig;

        config ??= new();
        config.BuildProfiles ??= [];

        MacroParser parser = new();
        parser.ImportMacros(MacroGenerator.GenerateMacrosForProject(config));
        parser.Normalize(config);

        if (config.ProjectGroup != null)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0132_DCBuildCalledOnProjectGroup,
                $"'dc build' can only be called on single projects. Use 'dc deploy' to build and deploy a project group.",
                ProjectConfigurationFileName);

            return -1;
        }

        if (args.Length > 0 && args.TakeWhile(a => !a.StartsWith('-')).Any())
        {
            string profileName = args.First(a => !a.StartsWith('-'));
            string[] additionalArgs = args.Skip(args.ToList().IndexOf(profileName) + 1).ToArray();

            if (config.BuildProfiles.Any(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase)))
            {
                BuildProfile profile = config.BuildProfiles.First(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
                return ExecuteBuildProfile(profile, config, additionalArgs);
            }

            EmitErrorMessage(
                0, 0, 0,
                DS0088_InvalidProfile,
                $"The build profile '{profileName}' could not be found.",
                ProjectConfigurationFileName);

            return -1;
        }
        else if (config.BuildProfiles.Any(p => p.Name.ToLowerInvariant() == "default"))
            return ExecuteBuildProfile(config.BuildProfiles.First(p => p.Name.ToLowerInvariant() == "default"), config, args);

        string[] filesToCompile = [];

        try
        {
            filesToCompile = Directory.EnumerateFiles("./", "*.ds", SearchOption.AllDirectories).ToArray();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0030_FileAccessDenied,
                $"Files to compile could not be determined: {ex.Message}",
                CompilerExecutableName);
        }

        filesToCompile = filesToCompile.Where(f => Path.GetDirectoryName(f).Split(Path.DirectorySeparatorChar).Last() != TemporaryBuildDirectoryName).ToArray();

        if (filesToCompile.Length < 1)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0073_NoSourceFilesFound,
                "No source files present.",
                "build");

            return -1;
        }

        string asmName = File.Exists(ProjectConfigurationFileName) ? ProjectConfigurationFileName : filesToCompile[0];
        asmName = Path.GetDirectoryName(Path.GetFullPath(asmName)).Split(Path.DirectorySeparatorChar)[^1];
        return CompileCommand.Instance.Invoke(filesToCompile.Concat(args).ToArray(), null, config.AssemblyName != null ? null : asmName);
    }

    private static int ExecuteBuildProfile(BuildProfile profile, DassieConfig config, string[] args)
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
                            DS0088_InvalidProfile,
                            errMsg,
                            ProjectConfigurationFileName);

                        return -1;
                    }
                    else if (proc.ExitCode != 0)
                    {
                        EmitWarningMessage(
                            0, 0, 0,
                            DS0088_InvalidProfile,
                            errMsg,
                            ProjectConfigurationFileName);
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(profile.Arguments))
            CompileCommand.Instance.Invoke(profile.Arguments.Split(' ').Concat(args).ToArray(), config);

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
                            DS0088_InvalidProfile,
                            errMsg,
                            ProjectConfigurationFileName);

                        return -1;
                    }
                    else if (proc.ExitCode != 0)
                    {
                        EmitWarningMessage(
                            0, 0, 0,
                            DS0088_InvalidProfile,
                            errMsg,
                            ProjectConfigurationFileName);
                    }
                }
            }
        }

        return 0;
    }
}
using Dassie.Build;
using Dassie.Configuration;
using Dassie.Extensions;
using System;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dassie.Core.Commands;

internal class BuildCommand : CompilerCommand
{
    private static BuildCommand _instance;
    public static BuildCommand Instance => _instance ??= new();

    public override string Command => "build";

    public override string Description => StringHelper.BuildCommand_Description;

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage = ["dc build [BuildProfile] [Options]"],
        Remarks = StringHelper.BuildCommand_Remarks,
        Options =
        [
            ("BuildProfile", StringHelper.BuildCommand_BuildProfileOptionDescription),
            ("Options", StringHelper.BuildCommand_OptionsOptionDescription)
        ],
        Examples =
        [
            ("dc build", StringHelper.BuildCommand_Example1),
            ("dc build CustomProfile", StringHelper.BuildCommand_Example2),
            ("dc build CustomProfile -r Aot", StringHelper.BuildCommand_Example3)
        ]
    };

    public override int Invoke(string[] args)
        => Invoke(args, null);

    internal int Invoke(string[] args, DassieConfig overrideConfig)
    {
        DassieConfig config = ProjectFileSerializer.DassieConfig;

        if (overrideConfig != null)
            config = overrideConfig;

        config ??= DassieConfig.Default;
        config.BuildProfiles ??= [];

        if (config.ProjectGroup != null)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0132_DCBuildCalledOnProjectGroup,
                nameof(StringHelper.BuildCommand_ProjectGroupNotSupported), [],
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

            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0088_InvalidProfile,
                nameof(StringHelper.BuildCommand_BuildProfileNotFound), [profileName],
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
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0030_FileAccessDenied,
                nameof(StringHelper.BuildCommand_FailedToCollectFiles), [ex.Message],
                CompilerExecutableName);
        }

        filesToCompile = filesToCompile.Where(f => Path.GetDirectoryName(f).Split(Path.DirectorySeparatorChar).Last() != TemporaryBuildDirectoryName).ToArray();

        if (filesToCompile.Length < 1)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0073_NoSourceFilesFound,
                nameof(StringHelper.BuildCommand_NoSourceFilesPresent), [],
                "build");

            return -1;
        }

        string asmName = File.Exists(ProjectConfigurationFileName) ? ProjectConfigurationFileName : filesToCompile[0];
        asmName = Path.GetDirectoryName(Path.GetFullPath(asmName)).Split(Path.DirectorySeparatorChar)[^1];
        return CompileCommand.Instance.Invoke(filesToCompile.Concat(args).ToArray(), null, config.AssemblyFileName != null ? null : asmName);
    }

    private static int ExecuteBuildProfile(BuildProfile profile, DassieConfig config, string[] args)
    {
        if (profile.Settings != null)
        {
            foreach (Property prop in profile.Settings.Store.Properties)
            {
                object val = profile.Settings.Store.Get(prop.Name);

                if (val != null)
                    config.Store.Set(prop.Name, val);
            }
        }

        if (profile.PreBuildEvents != null && profile.PreBuildEvents.Any())
        {
            foreach (BuildEvent preEvent in profile.PreBuildEvents)
            {
                if (string.IsNullOrEmpty(preEvent.Name))
                    preEvent.Name = profile.PreBuildEvents.IndexOf(preEvent).ToString();

                BuildEventHandler.ExecuteBuildEvent(preEvent, config, true);
            }
        }

        if (!string.IsNullOrEmpty(profile.Arguments))
            CompileCommand.Instance.Invoke(CommandLineParser.SplitCommandLine(profile.Arguments).Concat(args).ToArray(), config);

        if (profile.PostBuildEvents != null && profile.PostBuildEvents.Any())
        {
            foreach (BuildEvent postEvent in profile.PostBuildEvents)
            {
                if (string.IsNullOrEmpty(postEvent.Name))
                    postEvent.Name = profile.PostBuildEvents.IndexOf(postEvent).ToString();

                BuildEventHandler.ExecuteBuildEvent(postEvent, config, false);
            }
        }

        return 0;
    }
}
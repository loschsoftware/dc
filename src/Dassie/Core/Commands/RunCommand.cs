using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Extensions;
using Dassie.Messages;
using Dassie.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SDProcess = System.Diagnostics.Process;

namespace Dassie.Core.Commands;

internal class RunCommand : CompilerCommand
{
    private static RunCommand _instance;
    public static RunCommand Instance => _instance ??= new();

    public override string Command => "run";

    public override string Description => "Compiles a project or project group and then runs the output executable with the specified arguments.";

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage =
        [
            "dc run [Arguments]",
            "dc run -p|--profile=<Profile> -- [Arguments]"
        ],
        Remarks = "This command requires the presence of a project or project group. If it is executed on a project group, the project that is executed is determined by the <Executable> property in the project group definition."
                 + $"{Environment.NewLine}This command only recompiles the project if the source files have been updated since the last compilation or the output files have been deleted. Otherwise, the executable is launched immediately.",
        Options =
        [
            ("Arguments", "Command-line arguments passed to the program that is executed."),
            ("-p|--profile=<Profile>", "The build profile to use for compilation. If not specified, the default profile is used.")
        ],
        Examples =
        [
            ("dc run", "Compiles the current project and runs the resulting executable without any arguments."),
            ("dc run arg1 arg2", "Compiles the current project and runs the resulting executable with the arguments 'arg1' and 'arg2'."),
            ("dc run -p=CustomProfile", "Compiles the project with a specific build profile and runs the resulting executable without any arguments.")
        ]
    };

    public override int Invoke(string[] args)
    {
        string[] _args = args;
        string profile = null;
        if (args.Any(a => a.StartsWith("-p=") || a.StartsWith("--profile=")))
        {
            profile = string.Join('=', args.First(a => a.StartsWith("-p=") || a.StartsWith("--profile=")).Split('=')[1..]);
            _args = [];

            if (args.Any(a => a == "--"))
                _args = [.. args.SkipWhile(a => a != "--").Skip(1)];
            else
                _args = [.. args.Where(a => a != $"-p={profile}" && a != $"--profile={profile}")];
        }

        (int status, string assemblyPath, bool isNative, ISubsystem appType, _) = Compile(buildProfile: profile);

        if (status != 0)
            return status;

        if (!appType.IsExecutable)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0125_DCRunInvalidProjectType,
                $"The current project cannot be executed. Projects with an application type of '{appType.Name}' are not executable.",
                CompilerExecutableName);

            return -1;
        }

        string process = "dotnet";
        string arglist = string.Join(' ', (string[])[$"\"{assemblyPath}\"", .. _args]);

        if (isNative)
        {
            process = assemblyPath;
            arglist = string.Join(' ', arglist.Split(' ').Skip(1));
        }

        ProcessStartInfo psi = new()
        {
            FileName = process,
            Arguments = arglist
        };
        
        SDProcess.Start(psi).WaitForExit();
        return 0;
    }

    internal static (int Status, string AssemblyPath, bool IsNative, ISubsystem Type, bool IsProjectGroup) Compile(bool ignoreDS0031 = false, bool isProjectGroup = false, string buildProfile = null)
    {
        DassieConfig config = null;

        if (File.Exists(ProjectConfigurationFileName))
        {
            foreach (MessageInfo error in ConfigValidation.Validate(ProjectConfigurationFileName))
            {
                if (error.Severity == Severity.Error)
                {
                    Emit(error);
                    return (-1, null, false, default, isProjectGroup);
                }
            }

            XmlSerializer xmls = new(typeof(DassieConfig));
            using StreamReader sr = new(ProjectConfigurationFileName);
            config = (DassieConfig)xmls.Deserialize(sr);
        }

        if (ignoreDS0031)
            IgnoredCodes.Add(DS0031_NoEntryPoint);

        string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "build", $"{Directory.GetCurrentDirectory().Split(Path.DirectorySeparatorChar).Last()}.dll");
        string assemblyPath;
        bool isNative = false;

        if (config == null)
        {
            if (!File.Exists(defaultPath))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0106_DCRunInsufficientInfo,
                    "Insufficient information: The files to execute could not be determined. Create a project file (dsconfig.xml) and set the required properties 'BuildDirectory' and 'AssemblyFileName' to enable this command.",
                    CompilerExecutableName);

                return (-1, null, false, default, isProjectGroup);
            }

            assemblyPath = defaultPath;
        }
        else
        {
            MacroParser parser = new();
            parser.ImportMacros(MacroGenerator.GenerateMacrosForProject(config));
            parser.Normalize(config);

            if (config.ProjectGroup != null)
            {
                (int code, string path) = ProjectGroupHelpers.GetExecutableProject(config);

                if (code != 0)
                    return (code, null, false, default, isProjectGroup || config.ProjectGroup != null);

                Directory.SetCurrentDirectory(path);
                return Compile(isProjectGroup: true);
            }

            string assemblyName = Directory.GetCurrentDirectory().Split(Path.DirectorySeparatorChar).Last();
            if (!string.IsNullOrEmpty(config.AssemblyFileName))
                assemblyName = config.AssemblyFileName;

            string dir = Path.Combine(Directory.GetCurrentDirectory(), "build");
            if (!string.IsNullOrEmpty(config.BuildDirectory))
                dir = config.BuildDirectory;

            if (config.Runtime == Configuration.Runtime.Aot)
            {
                isNative = true;
                dir = Path.Combine(dir, AotBuildDirectoryName);
            }

            string extension = ".dll";
            if (config.Runtime == Configuration.Runtime.Aot)
            {
                extension = "";
                if (OperatingSystem.IsWindows())
                    extension = ".exe";
            }

            assemblyPath = Path.Combine(dir, $"{assemblyName}{extension}");
        }

        assemblyPath = Path.GetFullPath(assemblyPath);

        bool recompile = !File.Exists(assemblyPath);
        List<FileInfo> sourceFiles = [];

        foreach (string dir in ((config ??= new()).References ?? []).Where(r => r is ProjectReference).Cast<ProjectReference>().Select(p => Path.GetDirectoryName(p.ProjectFile)).Append(Directory.GetCurrentDirectory()))
        {
            sourceFiles.AddRange(Directory.GetFiles(dir, "*.ds", SearchOption.AllDirectories).Select(p => new FileInfo(p)));

            string projectFile = Path.Combine(dir, ProjectConfigurationFileName);
            if (File.Exists(projectFile))
                sourceFiles.Add(new(projectFile));
        }

        if (!recompile)
        {
            DateTime assemblyModifiedTime = new FileInfo(assemblyPath).LastWriteTime;
            recompile = sourceFiles.Any(f => f.LastWriteTime > assemblyModifiedTime);
        }

        if (recompile)
        {
            int ret = BuildCommand.Instance.Invoke(buildProfile == null ? [] : [buildProfile]);
            if (ret != 0 || EmittedMessages.Where(m => m.Severity == Severity.Error).Any())
                return (-1, null, false, default, isProjectGroup || config.ProjectGroup != null);
        }

        ISubsystem subsystem = Configuration.Subsystems.Console.Instance;

        if (config != null && config.ApplicationType != null && ExtensionLoader.Subsystems.Any(s => s.Name == config.ApplicationType))
            subsystem = ExtensionLoader.Subsystems.First(s => s.Name == config.ApplicationType);

        return (0, assemblyPath, isNative, subsystem, isProjectGroup || config.ProjectGroup != null);
    }
}
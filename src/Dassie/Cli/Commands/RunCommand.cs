﻿using Dassie.Configuration;
using Dassie.Configuration.Macros;
using Dassie.Errors;
using Dassie.Extensions;
using Dassie.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Dassie.Cli.Commands;

internal class RunCommand : ICompilerCommand
{
    private static RunCommand _instance;
    public static RunCommand Instance => _instance ??= new();

    public string Command => "run";

    public string Description => "Automatically compiles using the default profile and then runs the output executable with the specified arguments.";

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = Description,
        Usage = ["dc run [Arguments]"],
        Remarks = "This command requires the presence of a project or project group. If it is executed on a project group, the project that is executed is determined by the <Executable> property in the project group definition.",
        Options =
        [
            ("Arguments", "Command-line arguments passed to the program that is executed.")
        ],
        Examples =
        [
            ("dc run", "Compiles the current project and runs the resulting executable without any arguments."),
            ("dc run arg1 arg2", "Compiles the current project and runs the resulting executable with the arguments 'arg1' and 'arg2'.")
        ]
    };

    public int Invoke(string[] args)
    {
        (int status, string assemblyPath, bool isNative, ApplicationType appType, _) = Compile();

        if (status != 0)
            return status;

        if (appType == ApplicationType.Library)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0125_DCRunInvalidProjectType,
                "The current project cannot be executed. Projects with an application type of 'Library' are not executable.",
                CompilerExecutableName);

            return -1;
        }

        string process = "dotnet";
        string arglist = string.Join(' ', (string[])[$"\"{assemblyPath}\"", .. args]);

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

        Process.Start(psi).WaitForExit();
        return 0;
    }

    internal static (int Status, string AssemblyPath, bool IsNative, ApplicationType Type, bool IsProjectGroup) Compile(bool ignoreDS0031 = false, bool isProjectGroup = false)
    {
        DassieConfig config = null;

        if (File.Exists(ProjectConfigurationFileName))
        {
            foreach (ErrorInfo error in ConfigValidation.Validate(ProjectConfigurationFileName))
            {
                if (error.Severity == Severity.Error)
                {
                    EmitGeneric(error);
                    return (-1, null, false, 0, isProjectGroup);
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

                return (-1, null, false, 0, isProjectGroup);
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
                    return (code, null, false, 0, isProjectGroup || config.ProjectGroup != null);

                Directory.SetCurrentDirectory(path);
                return Compile(isProjectGroup: true);
            }

            string assemblyName = Directory.GetCurrentDirectory().Split(Path.DirectorySeparatorChar).Last();
            if (!string.IsNullOrEmpty(config.AssemblyName))
                assemblyName = config.AssemblyName;

            string dir = Path.Combine(Directory.GetCurrentDirectory(), "build");
            if (!string.IsNullOrEmpty(config.BuildOutputDirectory))
                dir = config.BuildOutputDirectory;

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

        foreach (string dir in (config.References ?? []).Where(r => r is ProjectReference).Cast<ProjectReference>().Select(p => Path.GetDirectoryName(p.ProjectFile)).Append(Directory.GetCurrentDirectory()))
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
            int ret = BuildCommand.Instance.Invoke([]);
            if (ret != 0 || Messages.Where(m => m.Severity == Severity.Error).Any())
                return (-1, null, false, 0, isProjectGroup || config.ProjectGroup != null);
        }

        return (0, assemblyPath, isNative, ApplicationType.Console, isProjectGroup || config.ProjectGroup != null);
    }
}
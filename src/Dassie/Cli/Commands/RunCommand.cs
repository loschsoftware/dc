using Dassie.Configuration;
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
    public string Command => "run";

    public string UsageString => "run [Arguments]";

    public string Description => "Automatically compiles using the default profile and then runs the output executable with the specified arguments.";
    
    public RunCommand()
    {
        _help = CommandHelpStringBuilder.GenerateHelpString(this);
    }

    private readonly string _help;
    public string Help() => _help;

    public int Invoke(string[] args)
    {
        if (args.Length > 0 && args[0] == "run")
            args = args[1..];

        DassieConfig config = null;

        if (File.Exists("dsconfig.xml"))
        {
            foreach (ErrorInfo error in ConfigValidation.Validate("dsconfig.xml"))
            {
                if (error.Severity == Severity.Error)
                {
                    EmitGeneric(error);
                    return -1;
                }
            }

            XmlSerializer xmls = new(typeof(DassieConfig));
            using StreamReader sr = new("dsconfig.xml");
            config = (DassieConfig)xmls.Deserialize(sr);
        }

        string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "build", $"{Directory.GetCurrentDirectory().Split(Path.DirectorySeparatorChar).Last()}.dll");
        bool isNative = false;
        string assemblyPath;

        if (config == null)
        {
            if (!File.Exists(defaultPath))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0105_DCRunInsufficientInfo,
                    "Insufficient information for 'dc run': The files to execute could not be determined. Create a project file (dsconfig.xml) and set the required properties 'BuildDirectory' and 'AssemblyFileName' to enable this command.",
                    "dc");

                return -1;
            }

            assemblyPath = defaultPath;
        }
        else
        {
            MacroParser parser = new();
            parser.ImportMacros(MacroGenerator.GenerateMacrosForProject(config));
            parser.Normalize(config);

            if (config.ApplicationType == ApplicationType.Library)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0124_DCRunInvalidProjectType,
                    "The current project is not executable. Projects with an application type of 'Library' cannot be executed.",
                    "dc");

                return -1;
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
                dir = Path.Combine(dir, "aot");
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
        List<FileInfo> sourceFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.ds", SearchOption.AllDirectories).Select(p => new FileInfo(p)).ToList();

        if (File.Exists("dsconfig.xml"))
            sourceFiles.Add(new("dsconfig.xml"));

        if (!recompile)
        {
            DateTime assemblyModifiedTime = new FileInfo(assemblyPath).LastWriteTime;
            recompile = sourceFiles.Any(f => f.LastWriteTime > assemblyModifiedTime);
        }

        if (recompile)
            DefaultCommandManager.CompileAllCommand.Invoke([]);

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
}
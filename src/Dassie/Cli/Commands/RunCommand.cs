using Dassie.Configuration;
using Dassie.Errors;
using Dassie.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;

namespace Dassie.Cli.Commands;

internal static partial class CliCommands
{
    public static int Run(string[] args)
    {
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
            CompileAll([]);

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
using Dassie.Configuration;
using Dassie.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Dassie.Cli.Commands;

internal static partial class CliCommands
{
    public static int Check(string[] args)
    {
        DassieConfig config = null;

        if (File.Exists("dsconfig.xml"))
        {
            XmlSerializer xmls = new(typeof(DassieConfig));
            using StreamReader sr = new("dsconfig.xml");
            config = (DassieConfig)xmls.Deserialize(sr);
        }

        config ??= new();

        IEnumerable<ErrorInfo> errors = CompileSource(args, config).SelectMany(e => e);

        if (errors.Count() == 0)
            Console.WriteLine("No errors found.");

        else
            Console.WriteLine($"{Environment.NewLine}{errors.Count()} error{(errors.Count() == 1 ? "" : "s")} found.");

        return errors.Any() ? -1 : 0;
    }

    public static int CheckAll()
    {
        string[] filesToCompile = Directory.EnumerateFiles(".\\", "*.ds", SearchOption.AllDirectories).ToArray();
        filesToCompile = filesToCompile.Where(f => Path.GetDirectoryName(f).Split(Path.DirectorySeparatorChar).Last() != ".temp").ToArray();

        if (filesToCompile.Length < 1)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0072_NoSourceFilesFound,
                "No source files present.",
                "check");

            return -1;
        }

        return Check(filesToCompile);
    }
}
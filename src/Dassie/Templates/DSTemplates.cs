﻿using Dassie.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Dassie.Templates;

/// <summary>
/// Provides templates for commonly used Dassie project types.
/// </summary>
public static class DSTemplates
{
    /// <summary>
    /// Contains a template for a console application.
    /// </summary>
    public static readonly string Console =

@"import System

Console.WriteLine ""Hello World!""";

    /// <summary>
    /// Contains a template for a library application.
    /// </summary>
    public static readonly string Library =
@"import System

type Library = {
    .
}";

    /// <summary>
    /// Contains a template for a class file.
    /// </summary>
    public static readonly string Class =
@"import System
export $$NAMESPACE$$

type Class1 = {
    .
}";

    /// <summary>
    /// Contains a template for an embedded script file.
    /// </summary>
    public static readonly string EmbeddedScript =
@"@script
@v481

WriteLine ""Hello World!""";

    /// <summary>
    /// Contains a template for a script host.
    /// </summary>
    public static readonly string EmbeddedScriptHost =
@"import Dassie.Scripting

ScriptHost.Run ""script.dsx""";

    internal static int CreateStructure(string[] args)
    {
        if (args.Length < 3)
        {
            System.Console.WriteLine("Specify an application type and name.");
            return -1;
        }

        string sourceDir = Path.Combine(Directory.GetCurrentDirectory(), args[2]);
        Directory.CreateDirectory(sourceDir);

        FileStream configFile = File.Create(Path.Combine(sourceDir, "dsconfig.xml"));

        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        XmlSerializer xmls = new(typeof(DassieConfig));

        DassieConfig config = new()
        {
            AssemblyName = args[2],
            RootNamespace = args[2][0].ToString().ToUpper() + string.Join("", args[2].Skip(1)),
            VersionInfo = new()
            {
                Version = "1.0.0.0"
            },
            BuildOutputDirectory = ".\\build"
        };

        switch (args[1])
        {
            case "console":
                {
                    var fs = File.Create(Path.Combine(sourceDir, "main.ds"));
                    fs.Write(Encoding.UTF8.GetBytes(Console.ToCharArray()), 0, Console.Length);

                    config.ApplicationType = ApplicationType.Console;
                    xmls.Serialize(configFile, config, ns);

                    break;
                }

            case "script":
                {
                    FileStream fs = File.Create(Path.Combine(sourceDir, "host.ds"));
                    fs.Write(Encoding.UTF8.GetBytes(EmbeddedScriptHost.ToCharArray()), 0, EmbeddedScriptHost.Length);

                    FileStream _fs = File.Create(Path.Combine(sourceDir, "script.dsx"));
                    _fs.Write(Encoding.UTF8.GetBytes(EmbeddedScript.ToCharArray()), 0, EmbeddedScript.Length);

                    config.ApplicationType = ApplicationType.Console;
                    xmls.Serialize(configFile, config, ns);
                    break;
                }
        }

        System.Console.WriteLine($"Built new project in {sourceDir} based on template '{args[1]}'.");

        return 0;
    }
}
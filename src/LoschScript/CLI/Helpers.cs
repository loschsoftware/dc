using Losch.LoschScript.Configuration;
using LoschScript.Meta;
using LoschScript.Templates;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LoschScript.CLI;

internal static class Helpers
{
    private static void HandleAllExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            EmitErrorMessage(0, 0, LS0000_UnexpectedError, "Unhandled exception.", "lsc.exe");
            Console.ForegroundColor = ConsoleColor.Gray;
        };
    }

    public static int HandleArgs(string[] args)
    {
        HandleAllExceptions();

        LSConfig config = null;

        if (File.Exists("lsconfig.xml"))
        {
            XmlSerializer xmls = new(typeof(LSConfig));
            using StreamReader sr = new("lsconfig.xml");
            config = (LSConfig)xmls.Deserialize(sr);
        }

        config ??= new();

        if (args.Where(s => (s.StartsWith('-') || s.StartsWith('/') || s.StartsWith("--")) && s.EndsWith("diagnostics")).Any())
            GlobalConfig.AdvancedDiagnostics = true;

        if (args.Where(s => !s.StartsWith('-') && !s.StartsWith("--") && !s.StartsWith('/')).Where(f => !File.Exists(f)).Any())
            LogOut.WriteLine($"Skipping non-existent files.{Environment.NewLine}");

        return CompileSource(args.Where(File.Exists).ToArray(), config).Any() ? -1 : 0;
    }

    public static int CompileAll()
    {
        HandleAllExceptions();

        LSConfig config = null;

        if (File.Exists("lsconfig.xml"))
        {
            XmlSerializer xmls = new(typeof(LSConfig));
            using StreamReader sr = new("lsconfig.xml");
            config = (LSConfig)xmls.Deserialize(sr);
        }

        config ??= new();

        return CompileSource(Directory.EnumerateFiles(".\\", "*.ls", SearchOption.AllDirectories).ToArray(), config).Any() ? -1 : 0;
    }

    public static int InterpretFiles(string[] args)
    {
        return 0;
    }

    public static int BuildLSConfig()
    {
        if (File.Exists("lsconfig.xml"))
        {
            LogOut.Write("The file lsconfig.xml already exists. Overwrite [Y/N]? ");
            string input = Console.ReadLine();

            if (input.ToLowerInvariant() != "y")
                return -1;
        }

        using StreamWriter configWriter = new("lsconfig.xml");

        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        XmlSerializer xmls = new(typeof(LSConfig));
        xmls.Serialize(configWriter, new LSConfig(), ns);

        LogOut.WriteLine("Created lsconfig.xml using default values.");
        return 0;
    }

    public static int StartReplSession()
    {
        return 0;
    }
}
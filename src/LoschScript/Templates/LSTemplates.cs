using Losch.LoschScript.Configuration;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace LoschScript.Templates;

/// <summary>
/// Provides templates for commonly used LoschScript project types.
/// </summary>
public static class LSTemplates
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

type Library =
    Main =
        Console.WriteLine ""Hello World!""";

    /// <summary>
    /// Contains a template for a class file.
    /// </summary>
    public static readonly string Class =
@"import System
export $$NAMESPACE$$

type Class1 =
    .";

    /// <summary>
    /// Contains a template for an embedded script file.
    /// </summary>
    public static readonly string EmbeddedScript =
@"@script
@v48

WriteLine ""Hello World!""";

    /// <summary>
    /// Contains a template for a script host.
    /// </summary>
    public static readonly string EmbeddedScriptHost =
@"import LoschScript.Scripting

ScriptHost.Run ""script.els""";
    
    internal static int CreateStructure(string[] args)
    {
        if (args.Length < 3)
        {
            System.Console.WriteLine("Specify an application type and name.");
            return -1;
        }

        string sourceDir = Path.Combine(Directory.GetCurrentDirectory(), args[2]);

        if (!Directory.Exists(sourceDir))
            Directory.CreateDirectory(sourceDir);

        switch (args[1])
        {
            case "console":
                var fs = File.Create(Path.Combine(sourceDir, "app.ls"));
                byte[] buffer = Encoding.UTF8.GetBytes(LSTemplates.Console.ToCharArray());
                fs.Write(buffer, 0, LSTemplates.Console.Length);

                var configFile = File.Create(Path.Combine(sourceDir, "lsconfig.xml"));

                XmlSerializerNamespaces ns = new();
                ns.Add("", "");

                var xmls = new XmlSerializer(typeof(LSConfig));
                xmls.Serialize(configFile, new LSConfig()
                {
                    ApplicationType = ApplicationType.Console,
                    AssemblyName = "app",
                    DefaultNamespace = "Application",
                    Version = "1.0.0.0",
                    BuildOutputDirectory = ".\\build"
                }, ns);
                break;
            case "script":
                var _fs = File.Create(Path.Combine(sourceDir, "host.ls"));
                byte[] _buffer = Encoding.UTF8.GetBytes(LSTemplates.EmbeddedScriptHost.ToCharArray());
                _fs.Write(_buffer, 0, LSTemplates.EmbeddedScriptHost.Length);

                var __fs = File.Create(Path.Combine(sourceDir, "script.els"));
                byte[] __buffer = Encoding.UTF8.GetBytes(LSTemplates.EmbeddedScript.ToCharArray());
                __fs.Write(__buffer, 0, LSTemplates.EmbeddedScript.Length);

                var _configFile = File.Create(Path.Combine(sourceDir, "lsconfig.xml"));

                XmlSerializerNamespaces _ns = new();
                _ns.Add("", "");

                var _xmls = new XmlSerializer(typeof(LSConfig));
                _xmls.Serialize(_configFile, new LSConfig()
                {
                    ApplicationType = ApplicationType.Console,
                    AssemblyName = "app",
                    DefaultNamespace = "Application",
                    Version = "1.0.0.0",
                    BuildOutputDirectory = ".\\build"
                }, _ns);
                break;
        }

        System.Console.WriteLine($"Built new project in {sourceDir} based on template '{args[1]}'.");

        return 0;
    }
}
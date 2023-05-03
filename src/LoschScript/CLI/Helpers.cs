using Losch.LoschScript.Configuration;
using LoschScript.Errors;
using LoschScript.Meta;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
        config.AssemblyName ??= Path.GetFileNameWithoutExtension(args.Where(File.Exists).First());

        if (args.Where(s => (s.StartsWith("-") || s.StartsWith("/") || s.StartsWith("--")) && s.EndsWith("diagnostics")).Any())
            GlobalConfig.AdvancedDiagnostics = true;

        if (args.Where(s => !s.StartsWith("-") && !s.StartsWith("--") && !s.StartsWith("/")).Where(f => !File.Exists(f)).Any())
            LogOut.WriteLine($"Skipping non-existent files.{Environment.NewLine}");
        
        string assembly = $"{config.AssemblyName}{(config.ApplicationType == ApplicationType.Library ? ".dll" : ".exe")}";

        IEnumerable<ErrorInfo[]> errors = CompileSource(args.Where(File.Exists).ToArray(), config);

        Context.Assembly.Save(assembly);

        if (errors.Select(e => e.Length).Sum() == 0)
        {
            Console.WriteLine($"\r\nCompilation successful, generated assembly {assembly}.");
            return 0;
        }

        Console.WriteLine($"\r\nCompilation failed with {errors.Select(e => e.Length).Sum()} errors.");
        return -1;
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

    public static Type ResolveTypeName(string name)
    {
        Type type = Type.GetType(name);

        if (type == null)
        {
            foreach (string ns in CurrentFile.Imports)
            {
                string n = $"{ns}.{name}";

                type = Type.GetType(n);

                if (type != null)
                    goto FoundType;

                List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetType(n) != null).ToList();
                if (assemblies.Any())
                    return assemblies.First().GetType(n);

                if (type != null)
                    goto FoundType;
            }

            foreach (string originalName in CurrentFile.Aliases.Where(a => a.Alias == name).Select(a => a.Name))
            {
                type = Type.GetType(originalName);

                if (type != null)
                    goto FoundType;
            }
        }

    FoundType:

        return type;
    }

    public static OpCode GetCallOpCode(Type type) => type.IsValueType ? OpCodes.Call : OpCodes.Callvirt;

    public static bool IsNumericType(Type type)
    {
        Type[] numerics =
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(nint),
            typeof(nuint)
        };

        return numerics.Contains(type);
    }

    public static int CallMethod(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Specify an assembly name, a method name and optional command line arguments.");
            return -1;
        }

        if (!File.Exists(args[1]))
        {
            Console.WriteLine("The specified assembly does not exist.");
            return -1;
        }

        Assembly a = Assembly.LoadFile(Path.GetFullPath(args[1]));

        string type = string.Join(".", args[2].Split('.')[0..^1]);
        Type t = a.GetType(type);

        if (t == null)
        {
            Console.WriteLine($"The type '{type}' does not exist.");
            return -1;
        }

        MethodInfo m;

        try
        {
            m = t.GetMethod(args[2].Split('.').Last(), BindingFlags.NonPublic | BindingFlags.Static);
        }
        catch (Exception)
        {
            Console.WriteLine("An error occured.");
            return -1;
        }

        if (m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string[]) && args.Length > 3)
        {
            m.Invoke(null, new object[] { args[3..] });
            return 0;
        }
        else if (m.GetParameters().Length == 0)
        {
            m.Invoke(null, Array.Empty<object>());
            return 0;
        }
        else
        {
            m.Invoke(null, new object[] { Array.Empty<string>() });
            return 0;
        }
    }
}
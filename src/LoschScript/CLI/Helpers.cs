using Losch.LoschScript.Configuration;
using LoschScript.CodeGeneration;
using LoschScript.Errors;
using LoschScript.Meta;
using LoschScript.Shell;
using LoschScript.Text;
using LoschScript.Text.Tooltips;
using LoschScript.Unmanaged;
using Microsoft.Build.Utilities;
using Microsoft.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Serialization;

namespace LoschScript.CLI;

internal static class Helpers
{
    public static int ViewFragments(string[] args)
    {
        LSConfig cfg = new();

        if (File.Exists("lsconfig.xml"))
        {
            using StreamReader sr = new("lsconfig.xml");
            XmlSerializer xmls = new(typeof(LSConfig));
            cfg = (LSConfig)xmls.Deserialize(sr);
        }

        foreach (string file in args.Where(File.Exists))
        {
            Console.WriteLine($"File: {Path.GetFileName(file)}");

            FileCompiler.GetEditorInfo(File.ReadAllText(file), cfg);

            Console.WriteLine();
            Console.WriteLine("Fragments:");

            foreach (Fragment frag in CurrentFile.Fragments)
                Console.WriteLine($"Line: {frag.Line}, Column: {frag.Column}, Length: {frag.Length}, Color: {frag.Color}");
        }

        return 0;
    }

    public static int HandleArgs(string[] args)
    {
        Stopwatch sw = new();
        sw.Start();

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

        Context.Assembly.DefineVersionInfoResource(
            Context.Configuration.Product,
            Context.Configuration.Version,
            Context.Configuration.Company,
            Context.Configuration.Copyright,
            Context.Configuration.Trademark);

        Context.Assembly.Save(assembly);

        if (File.Exists(Context.Configuration.ApplicationIcon))
        {
            if (!Win32Helpers.SetIcon(assembly, Context.Configuration.ApplicationIcon))
                EmitWarningMessage(0, 0, 0, LS0000_UnexpectedError, "The compilation was successful, but the assembly icon could not be set.", Path.GetFileName(assembly));
        }

        sw.Stop();

        if (args.Any(a => a == "-ilout"))
        {
            string ildasm = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("ildasm.exe");

            DirectoryInfo dir = Directory.CreateDirectory("cil");

            ProcessStartInfo psi = new()
            {
                FileName = ildasm,
                Arguments = $"{assembly} /out={Path.Combine(dir.FullName, Path.GetFileNameWithoutExtension(assembly) + ".il")}",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi);
        }

        if (errors.Select(e => e.Length).Sum() == 0)
        {
            Console.WriteLine($"\r\nCompilation successful, generated assembly {assembly}.");

            if (args.Any(a => a == "-elapsed") || Context.Configuration.MeasureElapsedTime)
                Console.WriteLine($"\r\nElapsed time: {sw.Elapsed.TotalMilliseconds} ms");

            return 0;
        }

        int count = errors.Select(e => e.Length).Sum();

        Console.WriteLine($"\r\nCompilation failed with {count} error{(count > 1 ? "s" : "")}.");

        if (args.Any(a => a == "-elapsed") || Context.Configuration.MeasureElapsedTime)
            Console.WriteLine($"\r\nElapsed time: {sw.Elapsed.TotalMilliseconds} ms");

        return -1;
    }

    public static int CompileAll(string[] args)
    {
        string[] filesToCompile = Directory.EnumerateFiles(".\\", "*.ls", SearchOption.AllDirectories).ToArray();
        return HandleArgs(filesToCompile.Concat(args).ToArray());
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
        LShell.Start();
        return 0;
    }

    public static (Type Type, MethodInfo[] Methods) ResolveGlobalMethod(string name, int row, int col, int len)
    {
        foreach (string type in CurrentFile.ImportedTypes)
        {
            Type t = ResolveTypeName(type, row, col, len);

            if (t.GetMethods().Where(m => m.Name == name).Any())
                return (t, t.GetMethods().Where(m => m.Name == name).ToArray());
        }

        return (null, Array.Empty<MethodInfo>());
    }

    public static Type ResolveTypeName(string name, int row, int col, int len)
    {
        Type type = Type.GetType(name);

        if (type == null)
        {
            List<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            allAssemblies.AddRange(Context.ReferencedAssemblies);

            List<Assembly> assemblies = allAssemblies.Where(_a => _a.GetType(name) != null).ToList();
            if (assemblies.Any())
            {
                type = assemblies.First().GetType(name);

                CurrentFile.Fragments.Add(new()
                {
                    Line = row,
                    Column = col,
                    Length = name.Length,
                    Color = TooltipGenerator.ColorForType(type.GetTypeInfo()),
                    ToolTip = TooltipGenerator.Type(type.GetTypeInfo(), true, true, false)
                });

                return type;
            }

            foreach (string ns in CurrentFile.Imports.Concat(Context.GlobalImports))
            {
                string n = $"{ns}.{name}";

                type = Type.GetType(n);

                if (type != null)
                    goto FoundType;

                List<Assembly> _allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                _allAssemblies.AddRange(Context.ReferencedAssemblies);

                List<Assembly> _assemblies = _allAssemblies.Where(a => a.GetType(n) != null).ToList();
                if (_assemblies.Any())
                    return _assemblies.First().GetType(n);

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

        if (type == null)
        {
            EmitErrorMessage(
                row,
                col,
                len,
                LS0009_TypeNotFound,
                $"The name '{name}' could not be resolved.");
        }
        else
        {
            CurrentFile.Fragments.Add(new()
            {
                Line = row,
                Column = col,
                Length = name.Length,
                Color = TooltipGenerator.ColorForType(type.GetTypeInfo()),
                ToolTip = TooltipGenerator.Type(type.GetTypeInfo(), true, true, false)
            });
        }

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

    public static bool IsIntegerType(Type type)
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
            typeof(nint),
            typeof(nuint)
        };

        return numerics.Contains(type);
    }

    public static bool IsUnsignedIntegerType(Type type)
    {
        Type[] numerics =
        {
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(nuint)
        };

        return numerics.Contains(type);
    }

    public static bool IsFloatingPointType(Type type)
    {
        Type[] floats =
        {
            typeof(float),
            typeof(double),
        };

        return floats.Contains(type);
    }

    public static Type GetEnumeratedType(this Type type) =>
        (type?.GetElementType() ?? (typeof(IEnumerable).IsAssignableFrom(type)
            ? type.GenericTypeArguments.FirstOrDefault()
            : null))!;

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

    public static void SetEntryPoint(AssemblyBuilder ab, MethodInfo m)
    {
        ab.SetEntryPoint(m);
    }

    public static void SetLocalSymInfo(LocalBuilder lb, string name)
    {
        lb.SetLocalSymInfo(name);
    }
}
using Losch.LoschScript.Configuration;
using LoschScript.CodeGeneration;
using LoschScript.Configuration;
using LoschScript.Errors;
using LoschScript.Meta;
using LoschScript.Parser;
using LoschScript.Runtime;
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

        Stopwatch sw = new();
        sw.Start();

        foreach (string file in args.Where(File.Exists))
        {
            Console.WriteLine($"File: {Path.GetFileName(file)}");

            FileCompiler.GetEditorInfo(File.ReadAllText(file), cfg);

            Console.WriteLine();
            Console.WriteLine("Fragments:");

            foreach (Fragment frag in CurrentFile.Fragments)
                Console.WriteLine($"Line: {frag.Line}, Column: {frag.Column}, Length: {frag.Length}, Color: {frag.Color}");
        }

        sw.Stop();
        Console.WriteLine($"\r\nElapsed time: {sw.Elapsed.TotalMilliseconds} ms");

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

        if (args.Where(s => !s.StartsWith("-") && !s.StartsWith("--") && !s.StartsWith("/")).Any(f => !File.Exists(f)))
        {
            foreach (string file in args.Where(s => !s.StartsWith("-") && !s.StartsWith("--") && !s.StartsWith("/")).Where(f => !File.Exists(f)))
            {
                EmitErrorMessage(
                    0,
                    0,
                    0,
                    LS0048_SourceFileNotFound,
                    $"The source file '{Path.GetFileName(file)}' could not be found.",
                    Path.GetFileName(file));
            }

            return -1;
        }

        if (!string.IsNullOrEmpty(config.BuildOutputDirectory))
        {
            Directory.CreateDirectory(config.BuildOutputDirectory);
            Directory.SetCurrentDirectory(config.BuildOutputDirectory);
        }

        string assembly = $"{config.AssemblyName}{(config.ApplicationType == ApplicationType.Library ? ".dll" : ".exe")}";

        // Step 1
        CompileSource(args.Where(File.Exists).ToArray(), config);
        VisitorStep1 = Context;

        // Step 2
        IEnumerable<ErrorInfo[]> errors = CompileSource(args.Where(File.Exists).ToArray(), config);

        if (!(Context.Configuration.Resources ?? Array.Empty<Resource>()).Any(r => r is UnmanagedResource))
        {
            Context.Configuration.VersionInfo ??= new();

            EmitMessage(
                0, 0, 0,
                LS0070_AvoidVersionInfoTag,
                $"Using the 'VersionInfo' tag in lsconfig.xml worsens compile performance. Consider precompiling your version info and including it as an unmanaged resource.",
                "lsconfig.xml");

            string rc = WinSdkHelper.GetToolPath("rc.exe");

            if (string.IsNullOrEmpty(rc))
            {
                EmitWarningMessage(
                    0, 0, 0,
                    LS0069_WinSdkToolNotFound,
                    $"The Windows SDK tool 'rc.exe' could not be located. Setting assembly icon failed.",
                    "lsconfig.xml");

                return -1;
            }

            Guid guid = Guid.NewGuid();
            string baseDir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "lsc")).FullName;
            string rcPath = Path.Combine(baseDir, $"{guid}.rc");

            ResourceScriptWriter rsw = new(rcPath);

            //Context.Assembly.DefineVersionInfoResource(
            //    Context.Configuration.Product,
            //    Context.Configuration.Version,
            //    Context.Configuration.Company,
            //    Context.Configuration.Copyright,
            //    Context.Configuration.Trademark);

            if (!string.IsNullOrEmpty(Context.Configuration.VersionInfo.ApplicationIcon) && !File.Exists(Context.Configuration.VersionInfo.ApplicationIcon))
            {
                EmitWarningMessage(
                   0, 0, 0,
                   LS0069_WinSdkToolNotFound,
                   $"The specified icon file '{Context.Configuration.VersionInfo.ApplicationIcon}' could not be found.",
                   "lsconfig.xml");

                return -1;
            }

            if (File.Exists(Context.Configuration.VersionInfo.ApplicationIcon ?? ""))
                rsw.AddMainIcon(Context.Configuration.VersionInfo.ApplicationIcon);

            rsw.Dispose();

            ProcessStartInfo psi = new()
            {
                FileName = rc,
                Arguments = rcPath,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi).WaitForExit();

            if (File.Exists(Path.ChangeExtension(rcPath, ".res")))
                Context.Assembly.DefineUnmanagedResource(Path.ChangeExtension(rcPath, ".res"));
        }

        foreach (Resource res in Context.Configuration.Resources ?? Array.Empty<Resource>())
            AddResource(res, Directory.GetCurrentDirectory());

        if (Context.Files.All(f => f.Errors.Count == 0) && VisitorStep1.Files.All(f => f.Errors.Count == 0))
            Context.Assembly.Save(assembly);

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

        if (args.Any(a => a == "-elapsed") || Context.Configuration.MeasureElapsedTime)
            Console.WriteLine($"\r\nElapsed time: {sw.Elapsed.TotalMilliseconds} ms");

        return errors.Select(e => e.Length).Sum() == 0 ? 0 : -1;
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

    public static (Type Type, MethodInfo[] Methods) ResolveGlobalMethod(string name, int row, int col, int len)
    {
        foreach (string type in CurrentFile.ImportedTypes)
        {
            Type t = ResolveTypeName(type, row, col, len, true);

            if (t.GetMethods().Where(m => m.Name == name).Any())
                return (t, t.GetMethods().Where(m => m.Name == name).ToArray());
        }

        return (null, Array.Empty<MethodInfo>());
    }

    public static Type ResolveTypeName(LoschScriptParser.Type_nameContext name, bool noEmitFragments = false)
    {
        if (name.identifier_atom() != null)
        {
            if (name.identifier_atom().Identifier() != null)
                return ResolveTypeName(name.identifier_atom().Identifier().GetText(), name.Start.Line, name.Start.Column, name.identifier_atom().Identifier().GetText().Length, noEmitFragments);

            return ResolveTypeName(name.identifier_atom().full_identifier().GetText(), name.Start.Line, name.Start.Column, name.identifier_atom().full_identifier().GetText().Length, noEmitFragments);
        }

        if (name.Open_Paren() != null)
            return typeof(UnionValue);

        if (name.type_arg_list() != null)
        {
            Type[] typeParams = name.type_arg_list().type_name().Select(t => ResolveTypeName(t, noEmitFragments)).ToArray();

            if (name.identifier_atom().Identifier() != null)
                return ResolveTypeName(name.identifier_atom().Identifier().GetText(), name.Start.Line, name.Start.Column, name.identifier_atom().Identifier().GetText().Length, noEmitFragments, typeParams);
        }

        // TODO: Implement other kinds of types
        return null;
    }

    public static Type ResolveTypeName(string name, int row, int col, int len, bool noEmitFragments = false, Type[] typeParams = null)
    {
        if (typeParams != null)
        {
            name += $"`{typeParams.Length}[";

            foreach (Type param in typeParams[0..^1])
                name += $"[{param.AssemblyQualifiedName}], ";

            name += $"[{typeParams.Last().AssemblyQualifiedName}]]";
        }

        Type type = Type.GetType(name);

        if (type == null)
        {
            List<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            allAssemblies.AddRange(Context.ReferencedAssemblies);

            List<Assembly> assemblies = allAssemblies.Where(_a => _a.GetType(name) != null).ToList();
            if (assemblies.Any())
            {
                type = assemblies.First().GetType(name);

                if (!noEmitFragments)
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
                {
                    type = _assemblies.First().GetType(n);
                    goto FoundType;
                }

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
            if (!noEmitFragments)
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
        }

        return type;
    }

    public static (int Type, int Index) GetLocalOrParameterIndex(string name)
    {
        if (CurrentMethod.Locals.Any(l => l.Name == name))
            return (0, CurrentMethod.Locals.First(l => l.Name == name).Index);

        else if (CurrentMethod.Parameters.Any(p => p.Name == name))
            return (1, CurrentMethod.Parameters.First(p => p.Name == name).Index);

        return (-1, -1);
    }

    public static SymbolInfo GetSymbol(string name)
    {
        if (CurrentMethod.Locals.Any(l => l.Name == name))
        {
            return new()
            {
                SymbolType = SymbolInfo.SymType.Local,
                Local = CurrentMethod.Locals.First(l => l.Name == name)
            };
        }

        else if (CurrentMethod.Parameters.Any(p => p.Name == name))
        {
            return new()
            {
                SymbolType = SymbolInfo.SymType.Parameter,
                Parameter = CurrentMethod.Parameters.First(p => p.Name == name)
            };
        }

        else if (TypeContext.Current.Fields.Any(f => f.Name == name))
        {
            return new()
            {
                SymbolType = SymbolInfo.SymType.Field,
                Field = TypeContext.Current.Fields.First(f => f.Name == name)
            };
        }

        return null;
    }

    public static void LoadSymbolAddress(SymbolInfo sym)
    {
        switch (sym.SymbolType)
        {
            case SymbolInfo.SymType.Local:
                EmitLdloca(sym.Local.Index);
                break;

            case SymbolInfo.SymType.Parameter:
                EmitLdarga(sym.Parameter.Index);
                break;

            default:
                if (!sym.Field.Builder.IsStatic)
                    EmitLdarg0IfCurrentType(sym.Field.Builder.FieldType);

                CurrentMethod.IL.Emit(sym.Field.Builder.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda, sym.Field.Builder);
                break;
        }
    }

    public static void LoadSymbol(SymbolInfo sym)
    {
        switch (sym.SymbolType)
        {
            case SymbolInfo.SymType.Local:
                EmitLdloc(sym.Local.Index);
                break;

            case SymbolInfo.SymType.Parameter:
                EmitLdarg(sym.Parameter.Index);
                break;

            default:
                if (!sym.Field.Builder.IsStatic)
                    EmitLdarg0IfCurrentType(sym.Field.Builder.FieldType);

                CurrentMethod.IL.Emit(sym.Field.Builder.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, sym.Field.Builder);
                break;
        }
    }

    public static void SetupBogusAssembly()
    {
        AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(new("Bogus"), AssemblyBuilderAccess.Run);
        Context.BogusAssembly = ab;

        ModuleBuilder mb = ab.DefineDynamicModule("Bogus");
        Context.BogusModule = mb;
    }

    static int bogusCounter = 0;
    public static void CreateFakeMethod()
    {
        TypeBuilder tb = Context.BogusModule.DefineType($"Bogus{bogusCounter++}");
        Context.BogusType = tb;

        MethodBuilder bogus = tb.DefineMethod("x", MethodAttributes.Public);
        CurrentMethod = new()
        {
            Builder = bogus,
            IL = bogus.GetILGenerator()
        };
    }

    public static FieldAttributes GetFieldAttributes(LoschScriptParser.Member_access_modifierContext accessModifier, LoschScriptParser.Member_oop_modifierContext oopModifier, LoschScriptParser.Member_special_modifierContext[] specialModifiers)
    {
        FieldAttributes baseAttributes;

        if (accessModifier == null || accessModifier.Global() != null)
            baseAttributes = FieldAttributes.Public;

        else if (accessModifier.Internal() != null)
            baseAttributes = FieldAttributes.Assembly;

        else
            baseAttributes = FieldAttributes.Private;

        if (oopModifier != null && oopModifier.Virtual() != null)
        {
            EmitErrorMessage(
                oopModifier.Start.Line,
                oopModifier.Start.Column,
                oopModifier.GetText().Length,
                LS0052_InvalidAccessModifier,
                "The modifier 'virtual' is not supported by this element.");
        }

        foreach (var modifier in specialModifiers)
        {
            if (modifier.Static() != null)
                baseAttributes |= FieldAttributes.Static;
            else
            {
                EmitErrorMessage(
                modifier.Start.Line,
                modifier.Start.Column,
                modifier.GetText().Length,
                LS0052_InvalidAccessModifier,
                $"The modifier '{modifier.GetText()}' is not supported by this element.");
            }
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && baseAttributes.HasFlag(FieldAttributes.Static))
        {
            EmitMessage(
                specialModifiers.First(s => s.GetText() == "static").Start.Line,
                specialModifiers.First(s => s.GetText() == "static").Start.Column,
                specialModifiers.First(s => s.GetText() == "static").GetText().Length,
                LS0058_RedundantModifier,
                "The 'static' modifier is implicit for module members and can be omitted.");
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && !baseAttributes.HasFlag(FieldAttributes.Static))
            baseAttributes |= FieldAttributes.Static;

        return baseAttributes;
    }

    public static MethodAttributes GetMethodAttributes(LoschScriptParser.Member_access_modifierContext accessModifier, LoschScriptParser.Member_oop_modifierContext oopModifier, LoschScriptParser.Member_special_modifierContext[] specialModifiers)
    {
        MethodAttributes baseAttributes;

        if (accessModifier == null || accessModifier.Global() != null)
            baseAttributes = MethodAttributes.Public;

        else if (accessModifier.Internal() != null)
            baseAttributes = MethodAttributes.Assembly;

        else
            baseAttributes = MethodAttributes.Private;

        if (oopModifier != null && oopModifier.Virtual() != null)
            baseAttributes |= MethodAttributes.Virtual;

        foreach (var modifier in specialModifiers)
        {
            if (modifier.Static() != null)
                baseAttributes |= MethodAttributes.Static;

            if (modifier.Extern() != null)
                baseAttributes |= MethodAttributes.PinvokeImpl;
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && baseAttributes.HasFlag(MethodAttributes.Static))
        {
            EmitMessage(
                specialModifiers.First(s => s.GetText() == "static").Start.Line,
                specialModifiers.First(s => s.GetText() == "static").Start.Column,
                specialModifiers.First(s => s.GetText() == "static").GetText().Length,
                LS0058_RedundantModifier,
                "The 'static' modifier is implicit for module members and can be omitted.");
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && !baseAttributes.HasFlag(MethodAttributes.Static))
            baseAttributes |= MethodAttributes.Static;

        return baseAttributes;
    }

    public static ParameterAttributes GetParameterAttributes(LoschScriptParser.Parameter_modifierContext modifier, bool hasDefault)
    {
        ParameterAttributes baseAttributes = ParameterAttributes.None;

        if (modifier != null && modifier.Ampersand_Greater() != null)
            baseAttributes = ParameterAttributes.In;
        else if (modifier != null && modifier.Less_Ampersand() != null)
            baseAttributes = ParameterAttributes.Out;

        if (hasDefault)
            baseAttributes |= ParameterAttributes.Optional;

        return baseAttributes;
    }

    public static TypeAttributes GetTypeAttributes(LoschScriptParser.Type_kindContext typeKind, LoschScriptParser.Type_access_modifierContext typeAccess, LoschScriptParser.Nested_type_access_modifierContext nestedTypeAccess, LoschScriptParser.Type_special_modifierContext modifiers, bool isNested)
    {
        TypeAttributes baseAttributes = TypeAttributes.Class;

        if (isNested)
            baseAttributes |= TypeAttributes.NestedPublic;
        else
            baseAttributes |= TypeAttributes.Public;

        if (typeKind.Template() != null)
            baseAttributes = TypeAttributes.Interface | TypeAttributes.Abstract;

        if (typeKind.Module() != null)
            baseAttributes |= TypeAttributes.Abstract | TypeAttributes.Sealed;

        else if (modifiers == null || modifiers.Open() == null)
            baseAttributes |= TypeAttributes.Sealed;

        if (typeAccess != null)
        {
            if (typeAccess.Global() != null)
                baseAttributes |= TypeAttributes.Public;
            else
                baseAttributes |= TypeAttributes.NotPublic;
        }
        else if (nestedTypeAccess != null)
        {
            if (nestedTypeAccess.Local() != null)
                baseAttributes |= TypeAttributes.NestedPrivate;

            else if (nestedTypeAccess.Protected() != null && nestedTypeAccess.Internal() != null)
                baseAttributes |= TypeAttributes.NestedFamORAssem;

            else if (nestedTypeAccess.Protected() != null)
                baseAttributes |= TypeAttributes.NestedFamily;

            else if (nestedTypeAccess.type_access_modifier().Global() != null)
                baseAttributes |= TypeAttributes.NestedPublic;

            else
                baseAttributes |= TypeAttributes.NestedAssembly;
        }

        return baseAttributes;
    }

    public static List<Type> GetInheritedTypes(LoschScriptParser.Inheritance_listContext context)
    {
        List<Type> types = new();

        int classCount = 0;

        foreach (LoschScriptParser.Type_nameContext typeName in context.type_name())
        {
            Type t = ResolveTypeName(typeName);

            if (t != null)
            {
                types.Add(t);

                if (t.IsClass)
                    classCount++;
            }

            if (classCount > 1)
            {
                EmitErrorMessage(
                    typeName.Start.Line,
                    typeName.Start.Column,
                    typeName.GetText().Length,
                    LS0051_MoreThanOneClassInInheritanceList,
                    "A type can only extend one base type."
                    );
            }
        }

        return types;
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
#if !NET7_COMPATIBLE
        ab.SetEntryPoint(m);
#endif
    }

    public static void SetLocalSymInfo(LocalBuilder lb, string name)
    {
        if (Context.Configuration.Configuration != ApplicationConfiguration.Debug)
            return;

#if !NET7_COMPATIBLE
        try
        {
            lb.SetLocalSymInfo(name);
        }
        catch (IndexOutOfRangeException) { }
#endif
    }

    public static bool HandleSpecialFunction(string name, LoschScriptParser.ArglistContext args, int line, int column, int length)
    {
        if (typeof(CompilerServices.CodeGeneration).GetMethod(name) == null)
            return false;

        CurrentFile.Fragments.Add(new()
        {
            Line = line,
            Column = column,
            Length = length,
            Color = Color.IntrinsicFunction,
            ToolTip = TooltipGenerator.Function(typeof(CompilerServices.CodeGeneration).GetMethod(name), true)
        });

        switch (name)
        {
            case "il":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                    line,
                        column,
                        length,
                        LS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'il'. Expected 1 argument."
                        );
                }

                string arg = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                EmitInlineIL(arg, args.expression()[0].Start.Line, args.expression()[0].Start.Column + 1, args.expression()[0].GetText().Length);

                return true;

            case "importNamespace":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                    line,
                        column,
                        length,
                        LS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'importNamespace'. Expected 1 argument."
                        );
                }

                string ns = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                CurrentFile.Imports.Add(ns);

                return true;
        }

        return false;
    }

    public static void AddResource(Resource res, string basePath)
    {
        if (!File.Exists(res.Path))
        {
            EmitErrorMessage(
                0, 0, 0,
                LS0067_ResourceFileNotFound,
                $"The resource file '{res.Path}' could not be located.",
                "lsconfig.xml");
        }

        else if (res is UnmanagedResource)
        {
            try
            {
                Context.Assembly.DefineUnmanagedResource(File.ReadAllBytes(res.Path));
            }
            catch (ArgumentException)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    LS0068_MultipleUnmanagedResources,
                    "An assembly can only contain one unmanaged resource file.",
                    "lsconfig.xml");
            }
        }

        else
        {
            ManagedResource mres = (ManagedResource)res;
            string resFile = Path.Combine(basePath, Path.GetFileName(mres.Path));

            File.Copy(mres.Path, resFile, true);
            Context.Assembly.AddResourceFile(mres.Name, resFile);
        }
    }
}
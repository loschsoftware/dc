using Antlr4.Runtime.Tree;
using Dassie.CodeGeneration;
using Dassie.Configuration;
using Dassie.Errors;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Runtime;
using Dassie.Text;
using Dassie.Text.Tooltips;
using Dassie.Unmanaged;
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

namespace Dassie.CLI;

internal static class Helpers
{
    public static int ViewFragments(string[] args)
    {
        DassieConfig cfg = new();

        if (File.Exists("dsconfig.xml"))
        {
            using StreamReader sr = new("dsconfig.xml");
            XmlSerializer xmls = new(typeof(DassieConfig));
            cfg = (DassieConfig)xmls.Deserialize(sr);
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

        DassieConfig config = null;

        if (File.Exists("dsconfig.xml"))
        {
            XmlSerializer xmls = new(typeof(DassieConfig));
            using StreamReader sr = new("dsconfig.xml");
            config = (DassieConfig)xmls.Deserialize(sr);
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
                    DS0048_SourceFileNotFound,
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

        string resFile = "";

        if (!(Context.Configuration.Resources ?? Array.Empty<Resource>()).Any(r => r is UnmanagedResource) && Context.Configuration.VersionInfo != null)
        {
            EmitMessage(
                0, 0, 0,
                DS0070_AvoidVersionInfoTag,
                $"Using the 'VersionInfo' tag in DassieConfig.xml worsens compilation performance. Consider precompiling your version info and including it as an unmanaged resource.",
                "dsconfig.xml");

            string rc = WinSdkHelper.GetToolPath("rc.exe");

            if (string.IsNullOrEmpty(rc))
            {
                EmitWarningMessage(
                    0, 0, 0,
                    DS0069_WinSdkToolNotFound,
                    $"The Windows SDK tool 'rc.exe' could not be located. Setting version information failed. Consider precompiling your version info and including it as an unmanaged resource.",
                    "dsconfig.xml");

                return -1;
            }

            Guid guid = Guid.NewGuid();

            string rcPath = Path.ChangeExtension(config.AssemblyName, "rc");
            ResourceScriptWriter rsw = new(rcPath);

            rsw.BeginVersionInfo();
            rsw.AddFileVersion(Context.Configuration.VersionInfo.FileVersion);
            rsw.AddProductVersion(Context.Configuration.VersionInfo.Version);

            rsw.Begin();
            rsw.AddStringFileInfo(
                Context.Configuration.VersionInfo.Company,
                Context.Configuration.VersionInfo.Description,
                Context.Configuration.VersionInfo.FileVersion,
                Context.Configuration.VersionInfo.InternalName,
                Context.Configuration.VersionInfo.Copyright,
                Context.Configuration.VersionInfo.Trademark,
                Context.Configuration.VersionInfo.Product,
                Context.Configuration.VersionInfo.Version
                );

            rsw.End();

            if (!string.IsNullOrEmpty(Context.Configuration.VersionInfo.ApplicationIcon) && !File.Exists(Context.Configuration.VersionInfo.ApplicationIcon))
            {
                EmitWarningMessage(
                   0, 0, 0,
                   DS0069_WinSdkToolNotFound,
                   $"The specified icon file '{Context.Configuration.VersionInfo.ApplicationIcon}' could not be found.",
                   "dsconfig.xml");

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

            resFile = Path.ChangeExtension(rcPath, ".res");

            if (File.Exists(resFile))
                Context.Assembly.DefineUnmanagedResource(resFile);

            if (!args.Where(s => (s.StartsWith("-") || s.StartsWith("/") || s.StartsWith("--")) && s.EndsWith("rc")).Any())
                File.Delete(rcPath);
        }

        if (!string.IsNullOrEmpty(config.AssemblyManifest) && File.Exists(config.AssemblyManifest))
        {
            // TODO: Include .manifest file
        }

        foreach (Resource res in Context.Configuration.Resources ?? Array.Empty<Resource>())
            AddResource(res, Directory.GetCurrentDirectory());

        if (Context.Files.All(f => f.Errors.Count == 0) && VisitorStep1.Files.All(f => f.Errors.Count == 0))
            Context.Assembly.Save(assembly);

        string coreLib = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dassie.Core.dll");

        if (Path.GetFullPath(Directory.GetCurrentDirectory()) != Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory))
        {
            try
            {
                File.Copy(coreLib, Path.Combine(Directory.GetCurrentDirectory(), "Dassie.Core.dll"), true);
            }
            catch (IOException) { }
        }

        sw.Stop();

        if (File.Exists(resFile) && !Context.Configuration.PersistentResourceFile)
            File.Delete(resFile);

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
        string[] filesToCompile = Directory.EnumerateFiles(".\\", "*.ds", SearchOption.AllDirectories).ToArray();

        if (filesToCompile.Length < 1)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0072_NoSourceFilesFound,
                "No source files present.",
                "build");

            return -1;
        }

        return HandleArgs(filesToCompile.Concat(args).ToArray());
    }

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

        return errors.Count() > 0 ? -1 : 0;
    }

    public static int CheckAll()
    {
        string[] filesToCompile = Directory.EnumerateFiles(".\\", "*.ds", SearchOption.AllDirectories).ToArray();

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

    public static int InterpretFiles(string[] args)
    {
        return 0;
    }

    public static int BuildDassieConfig()
    {
        if (File.Exists("dsconfig.xml"))
        {
            LogOut.Write("The file DassieConfig.xml already exists. Overwrite [Y/N]? ");
            string input = Console.ReadLine();

            if (input.ToLowerInvariant() != "y")
                return -1;
        }

        using StreamWriter configWriter = new("dsconfig.xml");

        XmlSerializerNamespaces ns = new();
        ns.Add("", "");

        XmlSerializer xmls = new(typeof(DassieConfig));
        xmls.Serialize(configWriter, new DassieConfig(), ns);

        LogOut.WriteLine("Created DassieConfig.xml using default values.");
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

    public static Type ResolveTypeName(DassieParser.Type_nameContext name, bool noEmitFragments = false)
    {
        int arrayDims = 0;

        if (name.array_type_specifier() != null)
        {
            arrayDims = (name.array_type_specifier().Comma() ?? Array.Empty<ITerminalNode>()).Length + 1;
            arrayDims += (name.array_type_specifier().Double_Comma() ?? Array.Empty<ITerminalNode>()).Length * 2;
        }

        if (arrayDims > 32)
        {
            EmitErrorMessage(
                name.Start.Line,
                name.Start.Column,
                name.GetText().Length,
                DS0079_ArrayTooManyDimensions,
                $"An array cannot have more than 32 dimensions.");
        }

        if (name.type_name() != null && name.type_name().Length > 0)
        {
            Type child = ResolveTypeName(name.type_name().First(), noEmitFragments);
            return ResolveTypeName(child.AssemblyQualifiedName, name.Start.Line, name.Start.Column, name.GetText().Length, noEmitFragments, arrayDimensions: arrayDims);
        }

        if (name.identifier_atom() != null)
        {
            if (name.identifier_atom().Identifier() != null)
                return ResolveTypeName(name.identifier_atom().Identifier().GetText(), name.Start.Line, name.Start.Column, name.identifier_atom().Identifier().GetText().Length, noEmitFragments, arrayDimensions: arrayDims);

            return ResolveTypeName(name.identifier_atom().full_identifier().GetText(), name.Start.Line, name.Start.Column, name.identifier_atom().full_identifier().GetText().Length, noEmitFragments, arrayDimensions: arrayDims);
        }

        if (name.Open_Paren() != null)
            return typeof(UnionValue);

        if (name.type_arg_list() != null)
        {
            Type[] typeParams = name.type_arg_list().type_name().Select(t => ResolveTypeName(t, noEmitFragments)).ToArray();

            if (name.identifier_atom().Identifier() != null)
                return ResolveTypeName(name.identifier_atom().Identifier().GetText(), name.Start.Line, name.Start.Column, name.identifier_atom().Identifier().GetText().Length, noEmitFragments, typeParams, arrayDimensions: arrayDims);
        }

        // TODO: Implement other kinds of types
        return null;
    }

    public static Type ResolveTypeName(string name, int row, int col, int len, bool noEmitFragments = false, Type[] typeParams = null, int arrayDimensions = 0)
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

                if (arrayDimensions > 0)
                    type = type.MakeArrayType(arrayDimensions);

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
                DS0009_TypeNotFound,
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

        if (arrayDimensions > 1)
            type = type.MakeArrayType(arrayDimensions);

        else if (arrayDimensions == 1)
            type = type.MakeArrayType();

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

    public static FieldAttributes GetFieldAttributes(DassieParser.Member_access_modifierContext accessModifier, DassieParser.Member_oop_modifierContext oopModifier, DassieParser.Member_special_modifierContext[] specialModifiers)
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
                DS0052_InvalidAccessModifier,
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
                DS0052_InvalidAccessModifier,
                $"The modifier '{modifier.GetText()}' is not supported by this element.");
            }
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && baseAttributes.HasFlag(FieldAttributes.Static))
        {
            EmitMessage(
                specialModifiers.First(s => s.GetText() == "static").Start.Line,
                specialModifiers.First(s => s.GetText() == "static").Start.Column,
                specialModifiers.First(s => s.GetText() == "static").GetText().Length,
                DS0058_RedundantModifier,
                "The 'static' modifier is implicit for module members and can be omitted.");
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && !baseAttributes.HasFlag(FieldAttributes.Static))
            baseAttributes |= FieldAttributes.Static;

        return baseAttributes;
    }

    public static MethodAttributes GetMethodAttributes(DassieParser.Member_access_modifierContext accessModifier, DassieParser.Member_oop_modifierContext oopModifier, DassieParser.Member_special_modifierContext[] specialModifiers)
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
                DS0058_RedundantModifier,
                "The 'static' modifier is implicit for module members and can be omitted.");
        }

        if (TypeContext.Current.Builder.IsSealed && TypeContext.Current.Builder.IsAbstract && !baseAttributes.HasFlag(MethodAttributes.Static))
            baseAttributes |= MethodAttributes.Static;

        return baseAttributes;
    }

    public static ParameterAttributes GetParameterAttributes(DassieParser.Parameter_modifierContext modifier, bool hasDefault)
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

    public static TypeAttributes GetTypeAttributes(DassieParser.Type_kindContext typeKind, DassieParser.Type_access_modifierContext typeAccess, DassieParser.Nested_type_access_modifierContext nestedTypeAccess, DassieParser.Type_special_modifierContext modifiers, bool isNested)
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

    public static List<Type> GetInheritedTypes(DassieParser.Inheritance_listContext context)
    {
        List<Type> types = new();

        int classCount = 0;

        foreach (DassieParser.Type_nameContext typeName in context.type_name())
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
                    DS0051_MoreThanOneClassInInheritanceList,
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

    public static bool HandleSpecialFunction(string name, DassieParser.ArglistContext args, int line, int column, int length)
    {
        if (typeof(Dassie.CompilerServices.CodeGeneration).GetMethod(name) == null)
            return false;

        if (args == null)
        {
            EmitErrorMessage(
                line,
                column,
                length,
                DS0080_ReservedIdentifier,
                $"The identifier '{name}' is reserved and cannot be used as a function or variable name."
                );

            return true;
        }

        CurrentFile.Fragments.Add(new()
        {
            Line = line,
            Column = column,
            Length = length,
            Color = Color.IntrinsicFunction,
            ToolTip = TooltipGenerator.Function(typeof(Dassie.CompilerServices.CodeGeneration).GetMethod(name), true)
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
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'il'. Expected 1 argument."
                        );

                    return true;
                }

                string arg = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                EmitInlineIL(arg, args.expression()[0].Start.Line, args.expression()[0].Start.Column + 1, args.expression()[0].GetText().Length);

                return true;

            case "localImport":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'localImport'. Expected 1 argument."
                        );

                    return true;
                }

                string ns = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                if (Type.GetType(ns) != null)
                {
                    CurrentFile.ImportedTypes.Add(ns);
                    return true;
                }

                CurrentFile.Imports.Add(ns);

                return true;

            case "globalImport":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'globalImport'. Expected 1 argument."
                        );

                    return true;
                }

                string _ns = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                if (Type.GetType(_ns) != null)
                {
                    Context.GlobalTypeImports.Add(_ns);
                    return true;
                }

                Context.GlobalImports.Add(_ns);

                return true;

            case "localAlias":

                if (args.expression().Length != 2)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'localAlias'. Expected 2 arguments."
                        );

                    return true;
                }

                string localAlias = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string localAliasedNS = args.expression()[1].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                CurrentFile.Aliases.Add((localAliasedNS, localAlias));

                return true;

            case "globalAlias":

                if (args.expression().Length != 2)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function 'globalAlias'. Expected 2 arguments."
                        );

                    return true;
                }

                string globalAlias = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string globalAliasedNS = args.expression()[1].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                CurrentFile.Aliases.Add((globalAliasedNS, globalAlias));

                return true;

            case "error":
            case "warn":
            case "msg":

                if (args.expression().Length != 2)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function '{name}'. Expected 2 arguments."
                        );

                    return true;
                }

                string code = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string err = args.expression()[1].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');

                ErrorInfo errInfo = new()
                {
                    CodePosition = (line, column),
                    Length = length,
                    CustomErrorCode = code,
                    ErrorCode = CustomError,
                    ErrorMessage = err,
                    File = Path.GetFileName(CurrentFile.Path),
                    Severity = name == "error" ? Severity.Error : name == "warn" ? Severity.Warning : Severity.Information
                };

                EmitGeneric(errInfo);

                return true;

            case "todo":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function '{name}'. Expected 1 argument."
                        );

                    return true;
                }
                
                string todoMsg = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string todoStr = $"TODO ({line}): {todoMsg}";

                CurrentMethod.IL.EmitWriteLine(todoStr);

                return true;

            case "ptodo":

                if (args.expression().Length != 1)
                {
                    EmitErrorMessage(
                        line,
                        column,
                        length,
                        DS0002_MethodNotFound,
                        $"Invalid number of arguments for special function '{name}'. Expected 1 argument."
                        );

                    return true;
                }

                string ptodoMsg = args.expression()[0].GetText().TrimStart('"').TrimEnd('\r', '\n').TrimEnd('"');
                string ptodoStr = $"TODO ({line}): {ptodoMsg}";

                CurrentMethod.IL.Emit(OpCodes.Ldstr, ptodoStr);
                CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(new[] { typeof(string) }));
                CurrentMethod.IL.Emit(OpCodes.Throw);

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
                DS0067_ResourceFileNotFound,
                $"The resource file '{res.Path}' could not be located.",
                "dsconfig.xml");
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
                    DS0068_MultipleUnmanagedResources,
                    "An assembly can only contain one unmanaged resource file.",
                    "dsconfig.xml");
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
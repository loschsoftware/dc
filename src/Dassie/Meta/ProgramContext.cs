﻿using Dassie.Configuration;
using Dassie.Errors;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class ProgramContext
{
    public static ProgramContext Context { get; set; }

    public static ProgramContext VisitorStep1 { get; set; }

    public List<FileContext> Files { get; } = [];

    public List<string> FilePaths { get; } = [];

    public List<TypeContext> Types { get; } = [];

    public List<string> GlobalImports { get; } = [];

    public List<string> GlobalTypeImports { get; } = [];

    public List<Assembly> ReferencedAssemblies { get; } = [];

    public List<(string Name, string Alias)> GlobalAliases { get; } = [];

    public PersistedAssemblyBuilder Assembly { get; set; }

    public MethodBuilder EntryPoint { get; set; }

    public AssemblyBuilder BogusAssembly { get; set; }

    public ModuleBuilder Module { get; set; }

    public ModuleBuilder BogusModule { get; set; }

    public TypeBuilder BogusType { get; set; }

    public bool EntryPointIsSet { get; set; } = false;

    public FileContext GetFile(string path)
    {
        if (Files.Any(f => f.Path == path))
            return Files.First(f => f.Path == path);

        FileContext fc = new(path);
        Files.Add(fc);
        return fc;
    }

    public TypeContext GetType(string name) => Types.Where(t => t.FullName == name).First();

    public DassieConfig Configuration { get; set; }

    public string ConfigurationPath { get; set; }

    public bool ShouldThrowDS0027 { get; set; } = false;

    public List<ErrorKind> CompilerSuppressedMessages { get; } = [];

    public List<(ConstructorInfo Constructor, object[] Data)> Attributes { get; set; } = [];

    public List<MethodInfo> ModuleInitializerParts { get; set; } = [];

    public TypeBuilder GlobalTopLevelFunctionContainerType { get; set; }
    public List<(FileContext DeclaringFile, DassieParser.Type_memberContext Function)> GlobalTopLevelFunctions { get; set; } = [];
}
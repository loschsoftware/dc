﻿using Dassie.Configuration;
using Dassie.Errors;
using Dassie.Text.FragmentStore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.Meta;

internal class ProgramContext
{
    public static ProgramContext Context { get; set; }
    
    public static ProgramContext VisitorStep1 { get; set; }

    public List<FileContext> Files { get; } = new();

    public List<TypeContext> Types { get; } = new();

    public List<string> GlobalImports { get; } = new();

    public List<string> GlobalTypeImports { get; } = new();

    public List<Assembly> ReferencedAssemblies { get; } = new();

    public List<(string Name, string Alias)> GlobalAliases { get; } = new();

    public PersistedAssemblyBuilder Assembly { get; set; }

    public MethodBuilder EntryPoint { get; set; }

    public AssemblyBuilder BogusAssembly { get; set; }

    public ModuleBuilder Module { get; set; }

    public ModuleBuilder BogusModule { get; set; }

    public TypeBuilder BogusType { get; set; }

    public bool EntryPointIsSet { get; set; } = false;

    public FileContext GetFile(string path) => Files.Where(f => f.Path == path).First();

    public TypeContext GetType(string name) => Types.Where(t => t.FullName == name).First();

    public DassieConfig Configuration { get; set; }

    public string ConfigurationPath { get; set; }

    public bool ShouldThrowDS0027 { get; set; } = false;

    public List<ErrorKind> CompilerSuppressedMessages { get; } = new();
}
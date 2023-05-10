using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace LoschScript.Meta;

internal class ProgramContext
{
    public static ProgramContext Context { get; set; }

    public List<FileContext> Files { get; } = new();

    public List<TypeContext> Types { get; } = new();

    public List<string> GlobalImports { get; } = new();

    public List<string> GlobalTypeImports { get; } = new();

    public List<Assembly> ReferencedAssemblies { get; } = new();

    public List<(string Name, string Alias)> GlobalAliases { get; } = new();

    public AssemblyBuilder Assembly { get; set; }

    public ModuleBuilder Module { get; set; }

    public FileContext GetFile(string path) => Files.Where(f => f.Path == path).First();

    public TypeContext GetType(string name) => Types.Where(t => t.FullName == name).First();
}
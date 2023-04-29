using System.Collections.Generic;
using System.Linq;

namespace LoschScript.Meta;

internal class ProgramContext
{
    public static ProgramContext Context { get; set; }

    public List<FileContext> Files { get; } = new();

    public List<TypeContext> Types { get; } = new();

    public List<string> GlobalImports { get; } = new();

    public List<string> GlobalTypeImports { get; } = new();

    public List<(string Name, string Alias)> GlobalAliases { get; } = new();

    public FileContext GetFile(string path) => Files.Where(f => f.Path == path).First();

    public TypeContext GetType(string name) => Types.Where(t => t.FullName == name).First();
}
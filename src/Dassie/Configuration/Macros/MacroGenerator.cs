using System.Collections.Generic;
using System.Linq;

namespace Dassie.Configuration.Macros;

internal static class MacroGenerator
{
    public static Dictionary<string, string> GenerateMacrosForProject(DassieConfig cfg)
    {
        Dictionary<string, string> macros = new()
        {
            ["ProjectName"] = Path.GetDirectoryName(Path.GetFullPath(ProjectConfigurationFileName)).Split(Path.DirectorySeparatorChar).Last(),
            ["ProjectDir"] = Path.GetDirectoryName(Path.GetFullPath(ProjectConfigurationFileName)) + Path.DirectorySeparatorChar,
            ["OutputDir"] = Path.GetFullPath(cfg.BuildDirectory ?? Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar,
            ["TargetPath"] = Path.GetFullPath(Path.Combine(cfg.BuildDirectory, $"{cfg.AssemblyFileName}.dll"))
        };

        if (cfg.MacroDefinitions != null)
        {
            foreach (Define macro in cfg.MacroDefinitions)
            {
                if (macro.Name == null)
                    continue;

                macros.Add(macro.Name, macro.Value ?? "");
            }
        }

        return macros;
    }
}
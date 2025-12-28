using System.Collections.Generic;
using System.Linq;

namespace Dassie.Configuration.Macros;

internal static class MacroGenerator
{
    public static Dictionary<string, string> GenerateMacrosForProject(DassieConfig cfg)
    {
        Dictionary<string, string> macros = [];

        macros.Add("ProjectDir", Path.GetDirectoryName(Path.GetFullPath(ProjectConfigurationFileName)) + Path.DirectorySeparatorChar);
        macros.Add("ProjectName", Path.GetDirectoryName(Path.GetFullPath(ProjectConfigurationFileName)).Split(Path.DirectorySeparatorChar).Last());
        macros.Add("OutputDir", Path.GetFullPath(cfg.BuildDirectory ?? Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar);

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
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Configuration.Macros;

internal static class MacroGenerator
{
    public static Dictionary<string, string> GenerateMacrosForProject(DassieConfig cfg)
    {
        Dictionary<string, string> macros = [];

        macros.Add("projectname", Path.GetDirectoryName(Path.GetFullPath("dsconfig.xml")).Split(Path.DirectorySeparatorChar).Last());

        if (cfg.MacroDefinitions != null)
        {
            foreach (Define macro in cfg.MacroDefinitions)
            {
                if (macro.Name == null)
                    continue;

                macros.Add(macro.Name.ToLowerInvariant(), macro.Value ?? "");
            }
        }

        return macros;
    }
}
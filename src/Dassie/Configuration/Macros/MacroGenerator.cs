using System.Collections.Generic;
using System.Linq;

namespace Dassie.Configuration.Macros;

internal static class MacroGenerator
{
    public static void AddDefinedMacros(DassieConfig cfg, Dictionary<string, string> macros)
    {
        if (cfg == null)
            return;

        if (cfg.MacroDefinitions == null || cfg.MacroDefinitions.Length == 0)
            return;

        foreach (Define macro in cfg.MacroDefinitions)
        {
            if (macro.Name == null)
                continue;

            string value = macro.Value ?? "";
            if (!macros.TryAdd(macro.Name, value))
                macros[macro.Name] = value;
        }
    }

    public static void AddPredefinedMacros(DassieConfig cfg, Dictionary<string, string> macros)
    {
        // TODO: What if cfg.BuildDirectory is itself constructed from a macro?

        macros.Add("ProjectName", Path.GetDirectoryName(Path.GetFullPath(ProjectConfigurationFileName)).Split(Path.DirectorySeparatorChar).Last());
        macros.Add("ProjectDir", Path.GetDirectoryName(Path.GetFullPath(ProjectConfigurationFileName)) + Path.DirectorySeparatorChar);
        macros.Add("OutputDir", Path.GetFullPath(cfg.BuildDirectory ?? Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar);
        macros.Add("TargetPath", Path.GetFullPath(Path.Combine(cfg.BuildDirectory, $"{cfg.AssemblyFileName}.dll")));
    }

    public static Dictionary<string, string> GenerateMacrosForProject(DassieConfig cfg)
    {
        Dictionary<string, string> macros = [];
        AddPredefinedMacros(cfg, macros);
        AddDefinedMacros(cfg, macros);
        return macros;
    }
}
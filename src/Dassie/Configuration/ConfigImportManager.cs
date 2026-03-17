using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Dassie.Configuration;

/// <summary>
/// Handles project files that use the '<c>Base</c>' attribute.
/// </summary>
internal static class ConfigImportManager
{
    private static List<XElement> ExtractImportedMacros(XElement root, bool skipRoot = false)
    {
        List<XElement> macros = [];
        IEnumerable<XElement> imports = root.Descendants("Import");

        if (imports != null)
        {
            foreach (XElement import in imports)
            {
                XAttribute pathAttribute = import.Attribute("Path");
                if (pathAttribute == null)
                {
                    IXmlLineInfo li = import;
                    EmitErrorMessageFormatted(
                        li.LineNumber,
                        li.LinePosition,
                        import.ToString().Length,
                        DS0198_ImportedConfigFileNotFound,
                        "Missing required attribute 'Path' for <Import/> definition.", [],
                        ProjectConfigurationFileName);
                }

                XElement importedRoot = ProjectFileDeserializer.Load(pathAttribute.Value).Root;
                macros.AddRange(ExtractImportedMacros(importedRoot));
            }
        }

        XElement macroDefs = root.Element("MacroDefinitions");
        if (!skipRoot && macroDefs != null)
        {
            IEnumerable<XElement> defines = macroDefs.Elements("Define");
            if (defines != null && defines.Any())
                macros.AddRange(defines);
        }

        return macros;
    }

    public static void ImportMacroDefinitions(XDocument baseDocument)
    {
        if (baseDocument == null)
            return;

        XElement root = baseDocument.Root;
        IEnumerable<XElement> importedMacros = ExtractImportedMacros(root, true);
        
        XElement macroDefs = root.Element("MacroDefinitions");
        if (macroDefs == null)
        {
            macroDefs = new("MacroDefinitions");
            root.AddFirst(macroDefs);
        }

        macroDefs.Add(importedMacros);
    }

    /// <summary>
    /// Handles the '<c>Base</c>' attribute if it is set for the specified project configuration.
    /// </summary>
    /// <param name="config">The configuration to merge.</param>
    public static void Merge(DassieConfig config)
    {
        string workingDir = Directory.GetCurrentDirectory();
        Merge(config, null);
        Directory.SetCurrentDirectory(workingDir);
    }
    
    private static void Merge(DassieConfig config, HashSet<string> visitedFiles, string fileName = null)
    {
        if (config == null || string.IsNullOrEmpty(config.Base)) return;

        fileName ??= ProjectConfigurationFileName;
        string importPath = Path.GetFullPath(config.Base);

        DassieConfig importedConfig = null;

        if (ExtensionLoader.ConfigurationProviders.Any(p => p.Name == config.Base))
            importedConfig = ExtensionLoader.ConfigurationProviders.First(p => p.Name == config.Base).Configuration;
        else
        {
            if (!File.Exists(importPath))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0198_ImportedConfigFileNotFound,
                    nameof(StringHelper.ConfigImportManager_ImportNotFound), [importPath],
                    fileName);

                return;
            }

            visitedFiles ??= new(StringComparer.OrdinalIgnoreCase);

            if (!visitedFiles.Add(importPath))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0199_ImportedConfigFileCircularDependency,
                    nameof(StringHelper.ConfigImportManager_CircularImport), [importPath],
                    fileName);

                return;
            }

            importedConfig = ProjectFileDeserializer.Deserialize(importPath, false);
        }

        Directory.SetCurrentDirectory(Path.GetDirectoryName(importPath));
        Merge(importedConfig, visitedFiles, importPath);
        ApplySettings(config, importedConfig);
    }

    private static void ApplySettings(DassieConfig target, DassieConfig source)
    {
        foreach (PropertyInfo property in typeof(DassieConfig).GetProperties())
        {
            if (!property.CanWrite || !property.CanRead) continue;

            DefaultValueAttribute defaultValueAttr = (DefaultValueAttribute)Attribute.GetCustomAttribute(property, typeof(DefaultValueAttribute));
            object defaultValue = defaultValueAttr?.Value;

            object targetValue = property.GetValue(target);
            object sourceValue = property.GetValue(source);

            if (targetValue == null || targetValue.Equals(defaultValue))
                property.SetValue(target, sourceValue);
        }
    }
}
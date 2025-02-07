using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Reflection;

namespace Dassie.Configuration;

/// <summary>
/// Handles project files that use the '<c>Import</c>' attribute.
/// </summary>
internal static class ConfigImportManager
{
    /// <summary>
    /// Handles the '<c>Import</c>' attribute if it is set for the specified project configuration.
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
        if (config == null || string.IsNullOrEmpty(config.Import)) return;

        fileName ??= ProjectConfigurationFileName;
        string importPath = Path.GetFullPath(config.Import);

        if (!File.Exists(importPath))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0197_ImportedConfigFileNotFound,
                $"The imported configuration file '{importPath}' could not be found.",
                fileName);

            return;
        }

        visitedFiles ??= new(StringComparer.OrdinalIgnoreCase);

        if (!visitedFiles.Add(importPath))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0198_ImportedConfigFileCircularDependency,
                $"Importing the configuration file '{importPath}' would lead to a circular dependency.",
                fileName);

            return;
        }

        DassieConfig importedConfig = Deserialize(importPath);
        Directory.SetCurrentDirectory(Path.GetDirectoryName(importPath));
        Merge(importedConfig, visitedFiles, importPath);
        ApplySettings(config, importedConfig);
    }

    private static DassieConfig Deserialize(string filePath)
    {
        XmlSerializer serializer = new(typeof(DassieConfig));
        using StreamReader reader = new(filePath);
        return (DassieConfig)serializer.Deserialize(reader);
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
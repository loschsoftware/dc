﻿using Dassie.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Dassie.CLI.Helpers;

internal static class ReferenceHandler
{
    public static bool HandleProjectReference(ProjectReference reference, DassieConfig currentConfig, string destDir)
    {
        if (!File.Exists(reference.ProjectFile))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0081_InvalidProjectReference,
                $"The referenced project file '{reference.ProjectFile}' could not be found.",
                "dsconfig.xml");

            return false;
        }

        string dir = Directory.GetCurrentDirectory();

        Directory.SetCurrentDirectory(Path.GetDirectoryName(reference.ProjectFile));
        int errCode = CliHelpers.CompileAll(Array.Empty<string>());

        if (errCode != 0)
            return false;

        using StreamReader sr = new(reference.ProjectFile);
        XmlSerializer xmls = new(typeof(DassieConfig));
        DassieConfig projConfig = (DassieConfig)xmls.Deserialize(sr);

        if (string.IsNullOrEmpty(projConfig.AssemblyName))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0081_InvalidProjectReference,
                $"The referenced project '{reference.ProjectFile}' does not specify an assembly name.",
                reference.ProjectFile);

            return false;
        }

        if (string.IsNullOrEmpty(projConfig.BuildOutputDirectory) && reference.CopyToOutput)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0081_InvalidProjectReference,
                $"The referenced project '{reference.ProjectFile}' does not specify an output directory, which is invalid if 'CopyToOutput' is set to 'true'.",
                reference.ProjectFile);

            return false;
        }

        string outFile = $"{projConfig.AssemblyName}{(projConfig.ApplicationType == ApplicationType.Library ? ".dll" : ".exe")}";

        if (!string.IsNullOrEmpty(projConfig.BuildOutputDirectory))
            outFile = Path.Combine(projConfig.BuildOutputDirectory, outFile);

        if (!reference.CopyToOutput)
        {
            currentConfig.References = currentConfig.References.Append(new AssemblyReference()
            {
                AssemblyPath = outFile
            }).ToArray();
        }
        else
        {
            foreach (string file in Directory.GetFiles(".\\"))
            {
                try
                {
                    File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
                }
                catch (IOException)
                {
                    // File in use -> already exists at output
                    continue;
                }

                if (Path.GetFileName(file) == Path.GetFileName(outFile))
                {
                    currentConfig.References = currentConfig.References.Append(new AssemblyReference()
                    {
                        AssemblyPath = Path.Combine(destDir, Path.GetFileName(file))
                    }).ToArray();
                }
            }
        }

        Directory.SetCurrentDirectory(dir);
        return true;
    }
}
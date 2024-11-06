using Dassie.Cli;
using Dassie.Cli.Commands;
using Dassie.Configuration;
using Dassie.Packages;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Dassie.Helpers;

/// <summary>
/// Provides tools for handling project and package references.
/// </summary>
internal static class ReferenceHandler
{
    /// <summary>
    /// Converts a project reference into an assembly reference by compiling the referenced project and referencing the generated executable.
    /// </summary>
    /// <param name="reference">The project reference to handle.</param>
    /// <param name="currentConfig">Compiler configuration for the current project.</param>
    /// <param name="destDir">The directory to copy build output files to.</param>
    /// <returns>Wheter or not the compilation of the project reference was successful.</returns>
    public static bool HandleProjectReference(ProjectReference reference, DassieConfig currentConfig, string destDir)
    {
        if (!File.Exists(reference.ProjectFile))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0081_InvalidProjectReference,
                $"The referenced project file '{reference.ProjectFile}' could not be found.",
                ProjectConfigurationFileName);

            return false;
        }

        string dir = Directory.GetCurrentDirectory();

        Directory.SetCurrentDirectory(Path.GetDirectoryName(reference.ProjectFile));
        int errCode = BuildCommand.Instance.Invoke([]);

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

    /// <summary>
    /// Converts a package reference into an assembly reference by downloading the package and referencing all contained assemblies.
    /// </summary>
    /// <param name="package">The package reference to handle.</param>
    /// <param name="config">The compiler configuration for the current project.</param>
    /// <returns>Wheter or not the operation was successful.</returns>
    public static bool HandlePackageReference(PackageReference package, DassieConfig config)
    {
        string version = PackageDownloader.DownloadPackage(package.PackageId, package.Version);
        string pkgDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", package.PackageId, version);
        string libDir = Path.Combine(pkgDir, "lib");

        if (Directory.Exists(libDir))
        {
            Version coreLibVersion = typeof(object).Assembly.GetName().Version;
            string target = coreLibVersion.ToString(2);
            string assembliesDir = Path.Combine(libDir, target);

            if (!Directory.Exists(assembliesDir))
                assembliesDir = Directory.GetDirectories(libDir).First();

            foreach (string asm in Directory.GetFiles(assembliesDir, "*.dll", SearchOption.AllDirectories))
            {
                config.References = [
                    .. config.References,
                    new AssemblyReference() {
                        AssemblyPath = asm,
                        CopyToOutput = true
                    }
                ];
            }
        }

        return true;
    }
}
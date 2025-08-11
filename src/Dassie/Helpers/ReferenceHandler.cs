using Dassie.Cli.Commands;
using Dassie.Configuration;
using Dassie.Packages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileSystem = Microsoft.VisualBasic.FileIO.FileSystem;

namespace Dassie.Helpers;

/// <summary>
/// Provides tools for handling project and package references.
/// </summary>
internal static class ReferenceHandler
{
    private static void ResolveProjectReference(ProjectReference reference, string referenceResolverBaseDir)
    {
        reference.ProjectFile = Path.GetFullPath(Path.Combine(referenceResolverBaseDir ?? Directory.GetCurrentDirectory(), reference.ProjectFile));
        if (Directory.Exists(reference.ProjectFile))
            reference.ProjectFile = Path.Combine(reference.ProjectFile, ProjectConfigurationFileName);
    }

    private static readonly List<string> _referencedProjectPaths = [];

    /// <summary>
    /// Converts a project reference into an assembly reference by compiling the referenced project and referencing the generated executable.
    /// </summary>
    /// <param name="reference">The project reference to handle.</param>
    /// <param name="currentConfig">Compiler configuration for the current project.</param>
    /// <param name="destDir">The directory to copy build output files to.</param>
    /// <param name="referenceResolverBaseDir">The directory used as a reference point to resolve relative paths. By default, is the current directory.</param>
    /// <returns>Wheter or not the compilation of the project reference was successful.</returns>
    public static bool HandleProjectReference(ProjectReference reference, DassieConfig currentConfig, string destDir, string referenceResolverBaseDir = null)
    {
        ResolveProjectReference(reference, referenceResolverBaseDir);

        if (!File.Exists(reference.ProjectFile))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0082_InvalidProjectReference,
                $"The referenced project file '{reference.ProjectFile}' could not be found.",
                ProjectConfigurationFileName);

            return false;
        }

        MessagePrefix = Path.GetDirectoryName(reference.ProjectFile).Split(Path.DirectorySeparatorChar).Last();

        string dir = Directory.GetCurrentDirectory();
        DassieConfig prevConfig = ProjectFileDeserializer.DassieConfig;

        Directory.SetCurrentDirectory(Path.GetDirectoryName(reference.ProjectFile));
        ProjectFileDeserializer.Reload();

        _referencedProjectPaths.Add(ProjectFileDeserializer.Path);

        if (ProjectFileDeserializer.DassieConfig.References != null && ProjectFileDeserializer.DassieConfig.References.Any(r => r is ProjectReference))
        {
            IEnumerable<ProjectReference> refs = ProjectFileDeserializer.DassieConfig.References.Where(r => r is ProjectReference).Cast<ProjectReference>();
            foreach (ProjectReference projRef in refs)
            {
                ResolveProjectReference(projRef, Directory.GetCurrentDirectory());

                if (_referencedProjectPaths.Contains(projRef.ProjectFile))
                {
                    if (ProjectFileDeserializer.Path == projRef.ProjectFile)
                    {
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0205_CircularProjectDependency,
                            $"Project '{Path.GetDirectoryName(ProjectFileDeserializer.Path).Split(Path.DirectorySeparatorChar)[^1]}' references itself.",
                            ProjectConfigurationFileName);
                    }
                    else
                    {
                        EmitErrorMessage(
                            0, 0, 0,
                            DS0205_CircularProjectDependency,
                            $"Circular project dependency between '{Path.GetDirectoryName(ProjectFileDeserializer.Path).Split(Path.DirectorySeparatorChar)[^1]}' and '{Path.GetDirectoryName(projRef.ProjectFile).Split(Path.DirectorySeparatorChar)[^1]}'.",
                            ProjectConfigurationFileName);
                    }

                    return false;
                }

                if (!_referencedProjectPaths.Contains(projRef.ProjectFile))
                    _referencedProjectPaths.Add(projRef.ProjectFile);
            }
        }

        int errCode = BuildCommand.Instance.Invoke([]);

        if (errCode != 0)
            return false;

        if (string.IsNullOrEmpty(ProjectFileDeserializer.DassieConfig.AssemblyName))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0082_InvalidProjectReference,
                $"The referenced project '{reference.ProjectFile}' does not specify an assembly name.",
                reference.ProjectFile);

            return false;
        }

        if (string.IsNullOrEmpty(ProjectFileDeserializer.DassieConfig.BuildOutputDirectory) && reference.CopyToOutput)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0082_InvalidProjectReference,
                $"The referenced project '{reference.ProjectFile}' does not specify an output directory, which is invalid if 'CopyToOutput' is set to 'true'.",
                reference.ProjectFile);

            return false;
        }

        string outFile = $"{ProjectFileDeserializer.DassieConfig.AssemblyName}.dll";

        if (!string.IsNullOrEmpty(ProjectFileDeserializer.DassieConfig.BuildOutputDirectory))
            outFile = Path.Combine(ProjectFileDeserializer.DassieConfig.BuildOutputDirectory, outFile);

        if (!reference.CopyToOutput)
        {
            currentConfig.References = currentConfig.References.Append(new AssemblyReference()
            {
                AssemblyPath = outFile
            }).ToArray();
        }
        else
        {
            foreach (string fsEntry in Directory.GetFileSystemEntries("./"))
            {
                try
                {
                    if (Directory.Exists(fsEntry))
                        FileSystem.CopyDirectory(fsEntry, Path.Combine(destDir, Path.GetDirectoryName(fsEntry)), true);
                    else
                        File.Copy(fsEntry, Path.Combine(destDir, Path.GetFileName(fsEntry)), true);
                }
                catch (IOException)
                {
                    // File in use -> already exists at output
                    continue;
                }

                if (Path.GetFileName(fsEntry) == Path.GetFileName(outFile))
                {
                    currentConfig.References = currentConfig.References.Append(new AssemblyReference()
                    {
                        AssemblyPath = Path.Combine(destDir, Path.GetFileName(fsEntry))
                    }).ToArray();
                }
            }
        }

        Directory.SetCurrentDirectory(dir);
        ProjectFileDeserializer.Set(prevConfig);
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
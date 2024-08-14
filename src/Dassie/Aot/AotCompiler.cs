using Dassie.Configuration;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Dassie.Aot;

/// <summary>
/// Obtains the necessary resources to enable ahead of time compilation and provides tools for compiling .NET assemblies.
/// </summary>
// TODO: Support systems other than Windows
internal class AotCompiler
{
    private readonly AotConfig _config;
    private AotCommandLineBuilder _cmdLineBuilder;

    public AotCompiler(DassieConfig cfg, string projectFilePath)
    {
        if (string.IsNullOrEmpty(cfg.RuntimeIdentifier))
            cfg.RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier;

        _config = new()
        {
            Config = cfg,
            ProjectFile = projectFilePath
        };
    }

    public bool Compile()
    {
        if (Context.Configuration.Verbosity >= 1)
            EmitBuildLogMessage($"Executing AOT compiler.");

        // Seems kind of wasteful to download the whole runtime just to use one file of it as an argument for ilc ...
        // ... But then again, storage is abundant anyway in 2024.
        DownloadRuntime();
        DownloadILCompiler();
        DownloadPlatformDependentILCompiler();
        _cmdLineBuilder = new(_config);

        InvokeCompiler();
        InvokeLinker();
        return true;
    }

    private string DownloadPackage(string packageId)
    {
        SourceCacheContext cache = new();
        SourceRepository repo = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource package = repo.GetResource<FindPackageByIdResource>();

        IEnumerable<NuGetVersion> versions = package.GetAllVersionsAsync(
            packageId,
            cache,
            NullLogger.Instance,
            CancellationToken.None).Result;

        if (!versions.Any())
        {
            string dir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", packageId)).FullName;
            string[] subDirs = Directory.GetDirectories(dir);

            if (subDirs.Length == 0)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0103_NetworkError,
                    $"Could not download package '{packageId}'.",
                    "dc");

                return "";
            }

            return subDirs.Last().Split(Path.DirectorySeparatorChar).Last();
        }

        NuGetVersion targetVersion = versions.Last();
        using MemoryStream ms = new();

        string packageDir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", packageId, targetVersion.ToFullString())).FullName;
        if (File.Exists(Path.Combine(packageDir, "Icon.png"))) // Just check if any file belonging to the package exists
            return targetVersion.ToFullString();

        InfoOut.WriteLine($"Downloading package '{packageId}'...");

        package.CopyNupkgToStreamAsync(
            packageId,
            targetVersion,
            ms,
            cache,
            NullLogger.Instance,
            CancellationToken.None).Wait();

        using PackageArchiveReader reader = new(ms);
        NuspecReader nuspecReader = reader.GetNuspecReaderAsync(CancellationToken.None).Result;

        ZipFile.ExtractToDirectory(ms, packageDir);
        return targetVersion.ToFullString();
    }

    private void DownloadRuntime()
    {
        string packageId = $"Microsoft.NETCore.App.Runtime.{_config.Config.RuntimeIdentifier}";
        string version = DownloadPackage(packageId);
        _config.RuntimePackageRootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", packageId, version);
    }

    private void DownloadILCompiler()
    {
        string packageId = "Microsoft.DotNet.ILCompiler";
        string version = DownloadPackage(packageId);
        _config.RuntimeIndependentILCompilerPackageRootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", packageId, version);
    }

    private void DownloadPlatformDependentILCompiler()
    {
        string packageId = $"runtime.{_config.Config.RuntimeIdentifier}.Microsoft.DotNet.ILCompiler";
        string version = DownloadPackage(packageId);
        _config.ILCompilerPackageRootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", packageId, version);
    }

    private void InvokeCompiler()
    {
        string args = _cmdLineBuilder.GenerateIlcArgumentList();

        if (Context.Configuration.Verbosity >= 2)
            EmitBuildLogMessage($"Invoking IL compiler with following arguments: {args}");

        Process.Start(Path.Combine(_config.ILCompilerPackageRootDirectory, "tools", "ilc.exe"), args).WaitForExit();
    }

    private void InvokeLinker()
    {
        string args = _cmdLineBuilder.GenerateLinkerArgumentList(out string linkerPath);

        if (Context.Configuration.Verbosity >= 2)
            EmitBuildLogMessage($"Invoking linker with following arguments: {args}");

        Process.Start(linkerPath, args).WaitForExit();
    }
}
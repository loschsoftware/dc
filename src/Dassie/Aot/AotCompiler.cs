using Dassie.Configuration;
using Dassie.Packages;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dassie.Aot;

/// <summary>
/// Obtains the necessary resources to enable ahead of time compilation and provides tools for compiling .NET assemblies.
/// </summary>
// TODO: Support systems other than Windows
internal class AotCompiler
{
    private readonly AotConfig _config;
    private AotCommandLineBuilder _cmdLineBuilder;

    /// <summary>
    /// Creates a new instance of the <see cref="AotCompiler"/> type.
    /// </summary>
    /// <param name="cfg">The compiler configuration for the project to be compiled.</param>
    /// <param name="projectFilePath">A path to the configuration file represented by <paramref name="cfg"/>.</param>
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

    /// <summary>
    /// Compiles a .NET assembly using the .NET ahead-of-time compiler.
    /// </summary>
    /// <returns></returns>
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

    private void DownloadRuntime()
    {
        string packageId = $"Microsoft.NETCore.App.Runtime.{_config.Config.RuntimeIdentifier}";
        string version = PackageDownloader.DownloadPackage(packageId);
        _config.RuntimePackageRootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", packageId, version);
    }

    private void DownloadILCompiler()
    {
        string packageId = "Microsoft.DotNet.ILCompiler";
        string version = PackageDownloader.DownloadPackage(packageId);
        _config.RuntimeIndependentILCompilerPackageRootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Packages", packageId, version);
    }

    private void DownloadPlatformDependentILCompiler()
    {
        string packageId = $"runtime.{_config.Config.RuntimeIdentifier}.Microsoft.DotNet.ILCompiler";
        string version = PackageDownloader.DownloadPackage(packageId);
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
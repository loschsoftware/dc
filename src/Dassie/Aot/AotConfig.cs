using Dassie.Configuration;

namespace Dassie.Aot;

/// <summary>
/// Configuration for ahead-of-time compilation consumed by <see cref="AotCommandLineBuilder"/>.
/// </summary>
internal class AotConfig
{
    /// <summary>
    /// The root directory of the platform-dependent IL compiler NuGet package, e.g. runtime.win-x64.Microsoft.DotNet.ILCompiler.
    /// </summary>
    public string ILCompilerPackageRootDirectory { get; set; }

    /// <summary>
    /// The root directory of the Microsoft.DotNet.ILCompiler NuGet package.
    /// </summary>
    public string RuntimeIndependentILCompilerPackageRootDirectory { get; set; }

    /// <summary>
    /// The root directory of the Microsoft.NETCore.App.Runtime.NativeAOT package.
    /// </summary>
    public string RuntimePackageRootDirectory { get; set; }

    /// <summary>
    /// The path to the dsconfig.xml project file of the project to be AOT compiled.
    /// </summary>
    public string ProjectFile { get; set; }

    /// <summary>
    /// The deserialized project configuration which contains settings dealing with AOT compilation.
    /// </summary>
    public DassieConfig Config { get; set; }
}
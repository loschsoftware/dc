using Dassie.Configuration;
using Dassie.Configuration.Subsystems;
using Dassie.Core.Actions;
using Dassie.Core.Commands;
using Dassie.Core.Macros;
using Dassie.Core.Properties;
using Dassie.Deployment;
using Dassie.Extensions;
using Dassie.Messages.Devices;
using Dassie.Meta.Directives;
using Dassie.Templates;

namespace Dassie.Core;

/// <summary>
/// Acts as an extension package for all built-in commands, project templates, build log devices, compiler directives, subsystems and actions.
/// </summary>
public class CorePackage : IPackage
{
    private static CorePackage _instance;
    internal static CorePackage Instance => _instance ??= new();

    /// <inheritdoc/>
    public bool Hidden() => false;

    /// <inheritdoc/>
    public PackageMetadata Metadata => new()
    {
        Author = StringHelper.CorePackage_Author,
        Description = StringHelper.CorePackage_Description,
        Name = "Core",
        Version = VersionCommand.AssemblyFriendlyVersion
    };

    /// <inheritdoc/>
    public ICompilerCommand[] Commands() =>
    [
        AnalyzeCommand.Instance,
        BuildCommand.Instance,
        CleanCommand.Instance,
        CompileCommand.Instance,
        ConfigCommand.Instance,
        DbgCommand.Instance,
        DeployCommand.Instance,
        HelpCommand.Instance,
        IdCommand.Instance,
        NewCommand.Instance,
        PackageCommand.Instance,
        RunCommand.Instance,
        ScratchpadCommand.Instance,
        TestCommand.Instance,
        VersionCommand.Instance,
        WatchCommand.Instance
    ];

    /// <inheritdoc/>
    public GlobalConfigProperty[] GlobalProperties() =>
    [
        EditorProperty.Instance,
        ExtensionLocationProperty.Instance,
        EnableExtensionsProperty.Instance,
        LanguageProperty.Instance,
        EnableCorePackageProperty.Instance,
        MsvcRootPathProperty.Instance,
        ILDasmPathProperty.Instance
    ];

    /// <inheritdoc/>
    public IProjectTemplate[] ProjectTemplates() => [LibraryProject.Instance, ConsoleProject.Instance];

    /// <inheritdoc/>
    public IBuildLogDevice[] BuildLogDevices() =>
    [
        TextWriterBuildLogDevice.Instance,
        new FileBuildLogDevice(),
        new FastConsoleBuildLogDevice()
    ];

    /// <inheritdoc/>
    public ICompilerDirective[] CompilerDirectives() =>
    [
        LineDirective.Instance,
        SourceDirective.Instance,
        TodoDirective.Instance,
        ILDirective.Instance,
        ImportDirective.Instance,
        TypeOfDirective.Instance
    ];

    /// <inheritdoc/>
    public IDeploymentTarget[] DeploymentTargets() =>
    [
        new DirectoryTarget()
    ];

    /// <inheritdoc/>
    public ISubsystem[] Subsystems() =>
    [
        Console.Instance,
        Library.Instance,
        WinExe.Instance
    ];

    /// <inheritdoc/>
    public IBuildAction[] BuildActions() =>
    [
        new CompilerCommandBuildAction(),
        new ShellCommandBuildAction(),
        new PrintBuildAction(),
        new LogBuildAction(),
        new SetEnvironmentVariableBuildAction(),
        new InputBuildAction(),
        new AssignBuildAction(),
        new ReadFileBuildAction(),
        new CopyBuildAction(),
        new MoveBuildAction(),
        new DeleteBuildAction(),
        new CodeBuildAction()
    ];

    /// <inheritdoc/>
    public IResourceProvider<string>[] LocalizationResourceProviders() =>
    [
        DefaultStrings.Instance
    ];

    /// <inheritdoc/>
    public IMacro[] Macros() =>
    [
        EvalMacro.Instance
    ];

    /// <inheritdoc/>
    public Property[] Properties() => [.. DassieConfig.GetDefaultPropertyRegistrations()];
}
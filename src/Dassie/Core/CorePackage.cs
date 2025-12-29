using Dassie.Configuration.Subsystems;
using Dassie.Core.Actions;
using Dassie.Core.Commands;
using Dassie.Core.Properties;
using Dassie.Deployment;
using Dassie.Messages.Devices;
using Dassie.Extensions;
using Dassie.Meta.Directives;
using Dassie.Templates;
using System.Reflection;

namespace Dassie.Core;

/// <summary>
/// Acts as an extension package for all built-in commands, project templates, build log devices, compiler directives, subsystems and actions.
/// </summary>
internal class CorePackage : IPackage
{
    private static CorePackage _instance;
    public static CorePackage Instance => _instance ??= new();

    public bool Hidden() => true;

    public PackageMetadata Metadata => new()
    {
        Author = "Losch",
        Description = "Contains core functionality of the Dassie compiler.",
        Name = "Core",
        Version = Assembly.GetCallingAssembly().GetName().Version
    };

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

    public GlobalConfigProperty[] GlobalProperties() =>
    [
        EditorProperty.Instance,
        ExtensionLocationProperty.Instance
    ];

    public IProjectTemplate[] ProjectTemplates() => [LibraryProject.Instance, ConsoleProject.Instance];

    public IBuildLogDevice[] BuildLogDevices() =>
    [
        TextWriterBuildLogDevice.Instance,
        new FileBuildLogDevice(),
        new FastConsoleBuildLogDevice()
    ];

    public ICompilerDirective[] CompilerDirectives() =>
    [
        LineDirective.Instance,
        SourceDirective.Instance,
        TodoDirective.Instance,
        ILDirective.Instance,
        ImportDirective.Instance,
        TypeOfDirective.Instance
    ];

    public IDeploymentTarget[] DeploymentTargets() =>
    [
        new DirectoryTarget()
    ];

    public ISubsystem[] Subsystems() =>
    [
        Console.Instance,
        Library.Instance,
        WinExe.Instance
    ];

    public IBuildAction[] BuildActions() =>
    [
        new CompilerCommandBuildAction(),
        new ShellCommandBuildAction(),
        new PrintAction(),
        new LogAction()
    ];
}
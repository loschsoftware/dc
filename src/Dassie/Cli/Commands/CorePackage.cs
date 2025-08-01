using Dassie.Errors.Devices;
using Dassie.Extensions;
using Dassie.Meta.Directives;
using Dassie.Templates;
using System;
using System.Linq;
using System.Reflection;

namespace Dassie.Cli.Commands;

/// <summary>
/// Acts as an extension package for all builtin commands, project templates, build log devices and compiler directives.
/// </summary>
internal class CorePackage : IPackage
{
    private static CorePackage _instance;
    public static CorePackage Instance => _instance ??= new();

    public PackageMetadata Metadata => new()
    {
        Author = "Losch",
        Description = "Contains core functionality of the Dassie compiler.",
        Name = "Core",
        Version = Assembly.GetCallingAssembly().GetName().Version
    };

    private ICompilerCommand[] _commands;
    public ICompilerCommand[] Commands() => _commands ??= [..Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.GetInterfaces().Contains(typeof(ICompilerCommand)))
        .Select(t => t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null))
        .Cast<ICompilerCommand>()];

    public IProjectTemplate[] ProjectTemplates() => [LibraryProject.Instance, ConsoleProject.Instance];

    public IBuildLogDevice[] BuildLogDevices() => [TextWriterBuildLogDevice.Instance, new FileBuildLogDevice()];

    public ICompilerDirective[] CompilerDirectives() =>
    [
        LineDirective.Instance,
        SourceDirective.Instance,
        TodoDirective.Instance,
        ILDirective.Instance,
        ImportDirective.Instance
    ];
}
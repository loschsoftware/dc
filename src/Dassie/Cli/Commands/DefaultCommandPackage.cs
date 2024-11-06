using Dassie.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Dassie.Cli.Commands;

/// <summary>
/// Acts as a package for all predefined commands.
/// </summary>
internal class DefaultCommandPackage : IPackage
{
    private static DefaultCommandPackage _instance;
    public static DefaultCommandPackage Instance => _instance ??= new();

    public PackageMetadata Metadata => new()
    {
        Author = "Losch",
        Description = "Predefined default commands of the Dassie compiler.",
        Name = "Default",
        Version = Assembly.GetCallingAssembly().GetName().Version
    };

    private Type[] _commands;
    public Type[] Commands => _commands ??= Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.FullName.StartsWith("Dassie.Cli.Commands"))
        .Where(t => t.GetInterfaces().Contains(typeof(ICompilerCommand)))
        .ToArray();
}
using Dassie.Configuration;

namespace Dassie.Extensions;

/// <summary>
/// Represents the application type and subsystem of a Dassie project.
/// </summary>
public interface ISubsystem
{
    /// <summary>
    /// The name of the subsystem.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Specifies wheter the application type is executable.
    /// </summary>
    public bool IsExecutable { get; }

    /// <summary>
    /// Specifies wheter the application type is a Windows GUI application.
    /// </summary>
    public bool WindowsGui { get; }

    /// <summary>
    /// A list of references that are automatically included when this subsystem is used.
    /// </summary>
    public Reference[] References { get; }

    /// <summary>
    /// A list of namespaces and modules that are automatically imported when this subsystem is used.
    /// </summary>
    public string[] Imports { get; }
}
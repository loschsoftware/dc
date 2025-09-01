using Dassie.Extensions;

namespace Dassie.Configuration.Subsystems;

/// <summary>
/// The default subsystem for library applications.
/// </summary>
internal class Library : ISubsystem
{
    private Library() { }
    private static Library _instance;
    public static Library Instance => _instance ??= new();

    public string Name => "Library";
    public bool IsExecutable => false;
    public Reference[] References => [];
    public string[] Imports => [];
    public bool WindowsGui => false;
}
using Dassie.Extensions;

namespace Dassie.Configuration.Subsystems;

/// <summary>
/// The default subsystem for console applications.
/// </summary>
internal class Console : ISubsystem
{
    private Console() { }
    private static Console _instance;
    public static Console Instance => _instance ??= new();

    public string Name => "Console";
    public bool IsExecutable => true;
    public Reference[] References => [];
    public string[] Imports => [];
    public bool WindowsGui => false;
}
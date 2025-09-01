using Dassie.Extensions;

namespace Dassie.Configuration.Subsystems;

/// <summary>
/// The default subsystem for console applications.
/// </summary>
internal class WinExe : ISubsystem
{
    private WinExe() { }
    private static WinExe _instance;
    public static WinExe Instance => _instance ??= new();

    public string Name => "WinExe";
    public bool IsExecutable => true;
    public Reference[] References => [];
    public string[] Imports => [];
    public bool WindowsGui => true;
}
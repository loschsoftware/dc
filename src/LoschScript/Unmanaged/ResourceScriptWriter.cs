using System;
using System.IO;

namespace LoschScript.Unmanaged;

internal class ResourceScriptWriter : IDisposable
{
    readonly StreamWriter sw;

    public ResourceScriptWriter(string path) => sw = new(path);

    public void AddIcon(string iconPath, int id)
    {
        sw.WriteLine($"{id} ICON {iconPath}");
        sw.Flush();
    }

    public void AddMainIcon(string iconPath)
    {
        sw.WriteLine($"MAINICON ICON LOADONCALL MOVEABLE DISCARDABLE IMPURE {iconPath}");
        sw.Flush();
    }

    public void Dispose()
    {
        sw.Dispose();
    }
}
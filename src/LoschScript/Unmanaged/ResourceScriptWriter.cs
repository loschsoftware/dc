using System;
using System.IO;

namespace LoschScript.Unmanaged;

internal class ResourceScriptWriter : IDisposable
{
    readonly StreamWriter sw;

    public ResourceScriptWriter(string path) => sw = new(path)
    {
        AutoFlush = true
    };

    public void AddIcon(string iconPath, int id)
    {
        sw.WriteLine($"{id} ICON {iconPath}");
    }

    public void AddMainIcon(string iconPath)
    {
        sw.WriteLine($"MAINICON ICON LOADONCALL MOVEABLE DISCARDABLE IMPURE {iconPath}");
    }

    public void AddFileVersion(int major, int minor, int patch = 0, int build = 0)
    {
        sw.WriteLine($"FILEVERSION {major},{minor},{patch},{build}");
    }

    public void AddProductVersion(int major, int minor, int patch = 0, int build = 0)
    {
        sw.WriteLine($"PRODUCTVERSION {major},{minor},{patch},{build}");
    }

    public void Dispose()
    {
        sw.Dispose();
    }
}
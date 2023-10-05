using System;
using System.IO;

namespace LoschScript.Unmanaged;

internal class ResourceScriptWriter : IDisposable
{
    readonly StreamWriter sw;

    public ResourceScriptWriter(string path)
    {
        sw = new(path)
        {
            AutoFlush = true
        };

        //sw.WriteLine("#include <winver.h>");
        //sw.WriteLine("#include <ntdef.h>");
    }

    public void AddIcon(string iconPath, int id)
    {
        sw.WriteLine($"{id} ICON {iconPath}");
    }

    public void AddMainIcon(string iconPath)
    {
        sw.WriteLine($"MAINICON ICON LOADONCALL MOVEABLE DISCARDABLE IMPURE {iconPath}");
    }

    public void AddFileVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return;

        string[] parts = version.Split('.');

        if (parts.Length < 2)
            return;

        AddFileVersion(
            int.Parse(parts[0]),
            int.Parse(parts[1]),
            parts.Length > 2 ? int.Parse(parts[2]) : 0,
            parts.Length > 3 ? int.Parse(parts[3]) : 0);
    }

    public void AddProductVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return;

        string[] parts = version.Split('.');

        if (parts.Length < 2)
            return;

        AddProductVersion(
            int.Parse(parts[0]),
            int.Parse(parts[1]),
            parts.Length > 2 ? int.Parse(parts[2]) : 0,
            parts.Length > 3 ? int.Parse(parts[3]) : 0);
    }

    public void BeginVersionInfo()
    {
        sw.WriteLine("VS_VERSION_INFO VERSIONINFO");
    }

    public void AddFileVersion(int major, int minor, int patch = 0, int build = 0)
    {
        sw.WriteLine($"FILEVERSION {major},{minor},{patch},{build}");
    }

    public void AddProductVersion(int major, int minor, int patch = 0, int build = 0)
    {
        sw.WriteLine($"PRODUCTVERSION {major},{minor},{patch},{build}");
    }

    public void End()
    {
        sw.WriteLine("END");
    }

    public void Dispose()
    {
        sw.Dispose();
    }
}
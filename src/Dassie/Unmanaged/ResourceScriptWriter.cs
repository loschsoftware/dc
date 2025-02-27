﻿using System;
using System.IO;
using System.Text;

namespace Dassie.Unmanaged;

internal class ResourceScriptWriter : IDisposable
{
    readonly StreamWriter sw;

    public ResourceScriptWriter(string path)
    {
        sw = new(new FileStream(path, FileMode.Create), Encoding.Unicode)
        {
            AutoFlush = true
        };
    }

    public void AddIcon(string iconPath, string id)
    {
        iconPath = iconPath.Trim('"');
        sw.WriteLine($"{id} ICON \"{iconPath.Replace("\\", "\\\\")}\"");
    }

    public void AddMainIcon(string iconPath) => AddIcon(iconPath, "MAINICON");

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

        sw.WriteLine(@"FILEFLAGSMASK 0x3fL
FILEFLAGS 0x0
FILESUBTYPE 0x0");
    }

    public void AddFileVersion(int major, int minor, int patch = 0, int build = 0)
    {
        sw.WriteLine($"FILEVERSION {major},{minor},{patch},{build}");
    }

    public void AddProductVersion(int major, int minor, int patch = 0, int build = 0)
    {
        sw.WriteLine($"PRODUCTVERSION {major},{minor},{patch},{build}");
    }

    public void Begin()
    {
        sw.WriteLine("BEGIN");
    }

    public void AddStringFileInfo(
        string company,
        string description,
        string fileVersion,
        string internalName,
        string legalCopyright,
        string legalTrademarks,
        string productName,
        string productVersion)
    {
        company ??= "";
        description ??= "";
        fileVersion ??= "";
        internalName ??= "";
        legalCopyright ??= "";
        legalTrademarks ??= "";
        productName ??= "";
        productVersion ??= "";

        company = company.Replace("\"", "\"\"");
        description = description.Replace("\"", "\"\"");
        fileVersion = fileVersion.Replace("\"", "\"\"");
        internalName = internalName.Replace("\"", "\"\"");
        legalCopyright = legalCopyright.Replace("\"", "\"\"");
        legalTrademarks = legalTrademarks.Replace("\"", "\"\"");
        productName = productName.Replace("\"", "\"\"");
        productVersion = productVersion.Replace("\"", "\"\"");

        sw.WriteLine($@"BLOCK ""StringFileInfo""
    BEGIN
        BLOCK ""040904b0""
        BEGIN
            VALUE ""CompanyName"", L""{company}\0""
            VALUE ""FileDescription"", L""{description}""
            VALUE ""FileVersion"", L""{fileVersion}\0""
            VALUE ""InternalName"", L""{internalName}""
            VALUE ""LegalCopyright"", L""{legalCopyright}\0""
            VALUE ""LegalTrademarks"", L""{legalTrademarks}\0""
            VALUE ""ProductName"", L""{productName}""
            VALUE ""ProductVersion"", L""{productVersion}\0""
        END
    END
    BLOCK ""VarFileInfo""
    BEGIN
        VALUE ""Translation"", 0x409, 1200
    END");
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
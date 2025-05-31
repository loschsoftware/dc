using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Dassie.Unmanaged;

internal class ResourceScriptWriter : IDisposable
{
    readonly StreamWriter _sw;
    readonly int[] _lcids;
    int _currentId;

    public ResourceScriptWriter(string path, int[] lcids)
    {
        _lcids = lcids;
        _sw = new(new FileStream(path, FileMode.Create), Encoding.Unicode)
        {
            AutoFlush = true
        };
    }

    public void AddIcon(string iconPath, string id)
    {
        _sw.WriteLine("LANGUAGE 0, 0");
        iconPath = iconPath.Trim('"');
        _sw.WriteLine($"{id} ICON \"{iconPath.Trim('"').Replace("\\", "\\\\")}\"");
    }

    public void AddMainIcon(string iconPath) => AddIcon(iconPath, "MAINICON");

    public void AddManifest(string manifestFilePath)
    {
        _sw.WriteLine("LANGUAGE 0, 0");
        _sw.WriteLine($"{++_currentId} RT_MANIFEST \"{manifestFilePath.Trim('"').Replace("\\", "\\\\")}\"");
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

    public void SetLanguage(int lcid)
    {
        _sw.WriteLine($"LANGUAGE {lcid}, {(lcid == 0 ? 0 : 1)}");
    }

    public void BeginVersionInfo(int fileType)
    {
        _sw.WriteLine($"{++_currentId} VERSIONINFO");

        _sw.WriteLine($@"FILEFLAGSMASK 0x3fL
FILEFLAGS 0x0
FILEOS 0x4
FILETYPE {fileType}
FILESUBTYPE 0x0");
    }

    public void AddFileVersion(int major, int minor, int patch = 0, int build = 0)
    {
        _sw.WriteLine($"FILEVERSION {major},{minor},{patch},{build}");
    }

    public void AddProductVersion(int major, int minor, int patch = 0, int build = 0)
    {
        _sw.WriteLine($"PRODUCTVERSION {major},{minor},{patch},{build}");
    }

    public void Begin()
    {
        _sw.WriteLine("BEGIN");
    }

    public void AddStringFileInfo(
        int lcid,
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

        _sw.WriteLine($@"BLOCK ""StringFileInfo""
    BEGIN
        BLOCK ""{string.Format("{0:X}", lcid).PadLeft(4, '0')}04b0""
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
    END");
    }

    public void AddVarFileInfo()
    {
        _sw.WriteLine($@"BLOCK ""VarFileInfo""
    BEGIN
        VALUE ""Translation"", {string.Join(" ", _lcids.Select(l => $"{l}, 1200,"))[..^1]}
    END");
    }

    public void End()
    {
        _sw.WriteLine("END");
    }

    public void Dispose()
    {
        _sw.Dispose();
    }
}
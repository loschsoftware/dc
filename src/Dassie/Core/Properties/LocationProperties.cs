using Dassie.Configuration.Global;
using Dassie.Extensions;
using Dassie.Resources;
using System;

namespace Dassie.Core.Properties;

internal class ExtensionLocationProperty : GlobalConfigProperty
{
    private static ExtensionLocationProperty _instance;
    public static ExtensionLocationProperty Instance => _instance ??= new();
    private ExtensionLocationProperty() { }

    public override string Name => "Locations.Extensions";
    public override GlobalConfigDataType Type => new(GlobalConfigBaseType.String, false);
    public override object DefaultValue => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Extensions");

    public override Func<object, bool>[] Validators => [value =>
    {
        try
        {
            _ = Path.GetFullPath(value.ToString());
            return true;
        }
        catch
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0261_ExtensionsLocationPropertyInvalidPath,
                nameof(StringHelper.LocationProperties_InvalidPath), [value],
                CompilerExecutableName);

            return false;
        }
    }];
}

internal class MsvcRootPathProperty : GlobalConfigProperty
{
    private static MsvcRootPathProperty _instance;
    public static MsvcRootPathProperty Instance => _instance ??= new();
    private MsvcRootPathProperty() { }

    public override string Name => "Locations.MsvcRootPath";
    public override GlobalConfigDataType Type => new(GlobalConfigBaseType.String, false);

    public override Func<object, bool>[] Validators => [value =>
    {
        if (!Directory.Exists(value.ToString()))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0261_ExtensionsLocationPropertyInvalidPath,
                nameof(StringHelper.LocationProperties_DirectoryNotExist), [value]);

            return false;
        }

        return true;
    }];
}

internal class ILDasmPathProperty : GlobalConfigProperty
{
    private static ILDasmPathProperty _instance;
    public static ILDasmPathProperty Instance => _instance ??= new();
    private ILDasmPathProperty() { }

    public override string Name => "Locations.ILDasmPath";
    public override GlobalConfigDataType Type => new(GlobalConfigBaseType.String, false);

    public override Func<object, bool>[] Validators => [value =>
    {
        if (!File.Exists(value.ToString()))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0261_ExtensionsLocationPropertyInvalidPath,
                nameof(StringHelper.LocationProperties_FileNotExist), [value]);

            return false;
        }

        return true;
    }];
}
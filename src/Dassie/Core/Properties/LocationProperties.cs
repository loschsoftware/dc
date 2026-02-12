using Dassie.Configuration.Global;
using Dassie.Extensions;
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
            EmitErrorMessage(
                0, 0, 0,
                DS0261_ExtensionsLocationPropertyInvalidPath,
                $"The specified value '{value}' is not a valid path.",
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
            EmitErrorMessage(
                0, 0, 0,
                DS0261_ExtensionsLocationPropertyInvalidPath,
                $"The specified directory '{value}' does not exist.");

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
            EmitErrorMessage(
                0, 0, 0,
                DS0261_ExtensionsLocationPropertyInvalidPath,
                $"The specified file '{value}' does not exist.");

            return false;
        }

        return true;
    }];
}
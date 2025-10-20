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
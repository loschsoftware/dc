using Dassie.Configuration.Global;
using Dassie.Extensions;
using System;
using System.Linq;

namespace Dassie.Core.Properties;

internal class EnableCorePackageProperty : GlobalConfigProperty
{
    private static EnableCorePackageProperty _instance;
    public static EnableCorePackageProperty Instance => _instance ??= new();
    private EnableCorePackageProperty() { }

    public override string Name => "EnableCorePackage";
    public override GlobalConfigDataType Type => new(GlobalConfigBaseType.Boolean, false);
    public override object DefaultValue => true;
}

internal class EnableExtensionsProperty : GlobalConfigProperty
{
    private static EnableExtensionsProperty _instance;
    public static EnableExtensionsProperty Instance => _instance ??= new();
    private EnableExtensionsProperty() { }

    public override string Name => "EnableExtensions";
    public override GlobalConfigDataType Type => new(GlobalConfigBaseType.Boolean, false);
    public override object DefaultValue => true;
}

internal class LanguageProperty : GlobalConfigProperty
{
    private static LanguageProperty _instance;
    public static LanguageProperty Instance => _instance ??= new();
    private LanguageProperty() { }

    public override string Name => "Language";
    public override GlobalConfigDataType Type => new(GlobalConfigBaseType.String, false);
    public override object DefaultValue => "en-US";

    public override Func<object, bool>[] Validators => [v =>
    {
        if (!ExtensionLoader.LocalizationResourceProviders.Any(l => l.Culture == v.ToString())
            && !File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dassie", "Localization", $"strings.{v.ToString()}.json")))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0277_LanguageNotFound,
                nameof(StringHelper.CoreProperties_NoResourceProviderForSpecifiedLanguage), [v],
                CompilerExecutableName);

            return false;
        }

        return true;
    }];
}
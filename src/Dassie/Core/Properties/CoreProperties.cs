using Dassie.Configuration.Global;
using Dassie.Extensions;

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
}
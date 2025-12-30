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
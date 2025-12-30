using Dassie.Configuration.Global;
using Dassie.Extensions;

namespace Dassie.Core.Properties;

internal class EditorProperty : GlobalConfigProperty
{
    private static EditorProperty _instance;
    public static EditorProperty Instance => _instance ??= new();
    private EditorProperty() { }
    
    public override string Name => "Scratchpad.Editor";
    public override GlobalConfigDataType Type => new(GlobalConfigBaseType.String, false);
    public override object DefaultValue => "default";
}
using Dassie.Extensions;

namespace Dassie.Resources;

internal partial class DefaultStrings : IResourceProvider<string>
{
    private static DefaultStrings _instance;
    public static DefaultStrings Instance => _instance ??= new();

    public string Culture => "en-US";

    // String table is source-generated from strings.en-US.json
}
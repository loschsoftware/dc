using Dassie.Extensions;
using System.Reflection;

namespace Dassie.Language;

public class GermanLanguageExtension : Extension
{
    public override PackageMetadata Metadata { get; } = new()
    {
        Name = "Dassie.Language.de-DE",
        Description = "Deutsches Sprachpaket für Dassie",
        Author = "Losch",
        Version = Assembly.GetExecutingAssembly().GetName().Version ?? new(1, 0)
    };

    private readonly IResourceProvider<string>[] _providers = [new ResourceProvider()];
    public override IResourceProvider<string>[] LocalizationResourceProviders() => _providers;
}
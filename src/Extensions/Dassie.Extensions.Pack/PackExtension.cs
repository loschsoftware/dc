using Dassie.Core.Commands;
using Dassie.Extensions.Pack.Commands;
using Dassie.Extensions.Pack.Resources;
using Dassie.Resources;
using System.Reflection;

namespace Dassie.Extensions.Pack;

public class PackExtension : Extension
{
    private readonly PackageMetadata _metadata = new()
    {
        Name = "Dassie.Pack",
        Description = "Provides tools for building and packing .dcx extension packages.",
        Author = "Losch",
        Version = VersionCommand.GetFriendlyVersion(Assembly.GetExecutingAssembly().GetName().Version ?? new(1, 0))
    };

    public override PackageMetadata Metadata => _metadata;

    private IEnvironmentInfo _env;
    private ICompilerCommand _packCommand;
    private LocalizationProvider _provider;

    public override int InitializeGlobal(IEnvironmentInfo environment)
    {
        _env = environment;
        _provider = new(Metadata.PackageIdentity);
        _packCommand = new PackCommand(_env, _provider);

        return 0;
    }

    public override ICompilerCommand[] Commands() => [_packCommand];

    private readonly IResourceProvider<string>[] _stringProviders = [new Strings_deDE(), new Strings_enUS()];
    public override IResourceProvider<string>[] LocalizationResourceProviders() => _stringProviders;
}
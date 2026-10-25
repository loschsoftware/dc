using Dassie.Core.Commands;
using Dassie.Extensions.Pack.Commands;
using Dassie.Extensions.Pack.Resources;
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

    public override int InitializeGlobal(IEnvironmentInfo environment)
    {
        _env = environment;
        _packCommand = new PackCommand(_env, LocalizationProvider);

        return 0;
    }

    public override ICompilerCommand[] Commands() => [_packCommand];

    private readonly IResourceProvider<string>[] _stringProviders = [new Strings_deDE(), new Strings_enUS()];
    public override IResourceProvider<string>[] LocalizationResourceProviders() => _stringProviders;

    // TODO: Subsytem & Project template for extension package
    // PackCommand handles a project, also supporting .csproj
}
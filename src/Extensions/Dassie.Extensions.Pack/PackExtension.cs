using Dassie.Core.Commands;
using Dassie.Extensions.Pack.Commands;
using Dassie.Resources;
using System.Reflection;

namespace Dassie.Extensions.Pack;

public class PackExtension : Extension
{
    public override PackageMetadata Metadata { get; } = new()
    {
        Name = "Dassie.Pack",
        Description = StringHelper.PackExtension_Description,
        Author = "Losch",
        Version = VersionCommand.GetFriendlyVersion(Assembly.GetExecutingAssembly().GetName().Version ?? new(1, 0))
    };

    private IEnvironmentInfo _env;
    private ICompilerCommand _packCommand;

    public override int InitializeGlobal(IEnvironmentInfo environment)
    {
        _env = environment;
        _packCommand = new PackCommand(_env);
        return 0;
    }

    public override ICompilerCommand[] Commands() => [_packCommand];
}
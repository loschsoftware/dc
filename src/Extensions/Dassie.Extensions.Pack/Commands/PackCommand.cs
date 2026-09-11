using Dassie.Messages;
using Dassie.Resources;

namespace Dassie.Extensions.Pack.Commands;

internal class PackCommand(IEnvironmentInfo env) : CompilerCommand
{
    public override string Command => "pack";
    public override string Description => StringHelper.PackCommand_Description;

    public override int Invoke(string[] args)
    {
        MessageWriter.WriteLine(env.UICulture.DisplayName);
        return 0;
    }
}
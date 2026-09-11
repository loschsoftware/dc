using Dassie.Messages;
using Dassie.Resources;

namespace Dassie.Extensions.Pack.Commands;

internal class PackCommand(IEnvironmentInfo env, LocalizationProvider lp) : CompilerCommand
{
    public override string Command => "pack";
    public override string Description => lp.GetString("PackCommand_Description");

    public override int Invoke(string[] args)
    {
        MessageWriter.WriteLine(env.UICulture.DisplayName);
        return 0;
    }
}
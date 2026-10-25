using Dassie.Resources;

namespace Dassie.Extensions.Pack.Commands;

internal class PackCommand(LocalizationProvider lp) : CompilerCommand
{
    public override string Command => "pack";
    public override string Description => lp.GetString("PackCommand_Description");

    public override CommandHelpDetails HelpDetails => GetHelpDetails();
    private CommandHelpDetails GetHelpDetails()
    {
        return new()
        {
            Description = Description,
            Usage = ["dc pack"]
        };
    }

    public override int Invoke(string[] args)
    {
        return 0;
    }
}
using Dassie.Extensions;
using System.Text;

#pragma warning disable IDE0079
#pragma warning disable IL3000

namespace Dassie.Core.Commands;

internal class PackageSourceCommand : CompilerCommand
{
    private static PackageSourceCommand _instance;
    public static PackageSourceCommand Instance => _instance ??= new();

    public override string Command => "source";

    public override string Description => StringHelper.PackageSourceCommand_Description;

    public override CommandHelpDetails HelpDetails => GetHelpDetails();
    private static CommandHelpDetails GetHelpDetails()
    {
        StringBuilder commandsSb = new();
        commandsSb.Append($"{"    list",-35}{HelpCommand.FormatLines(StringHelper.PackageSourceCommand_ListDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    add <Name> <Path>",-35}{HelpCommand.FormatLines(StringHelper.PackageSourceCommand_AddDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    remove <Name>",-35}{HelpCommand.FormatLines(StringHelper.PackageSourceCommand_RemoveDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    enable <Name>",-35}{HelpCommand.FormatLines(StringHelper.PackageSourceCommand_EnableDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    disable <Name>",-35}{HelpCommand.FormatLines(StringHelper.PackageSourceCommand_DisableDescription, indentWidth: 35)}");
        commandsSb.Append($"{"    set-primary <Name>",-35}{HelpCommand.FormatLines(StringHelper.PackageSourceCommand_SetPrimaryDescription, indentWidth: 35)}");

        return new()
        {
            Description = StringHelper.PackageSourceCommand_Description,
            Usage = ["dc package source [Command] [Options]"],
            Options =
            [
                ("Command", StringHelper.PackageCommand_CommandOption),
                ("Options", StringHelper.PackageCommand_OptionsOption)
            ],
            CustomSections =
            [
                (StringHelper.PackageCommand_AvailableCommands, commandsSb.ToString())
            ],
            Examples =
            [
                ("dc package source list", StringHelper.PackageSourceCommand_Example1),
                ("dc package source add default https://losch.at/dc/packages", StringHelper.PackageSourceCommand_Example2),
                ("dc package source disable source1", StringHelper.PackageSourceCommand_Example3),
            ]
        };
    }

    public override int Invoke(string[] args)
    {
        args ??= [];

        if (args.Length == 0)
            args = ["help"];

        string command = args[0];

        if (command == "list")
            return ListExtensionSources();

        if (command == "add" && args.Length > 1)
            return AddExtensionSource(args[1..]);

        if (command == "remove" && args.Length > 1)
            return RemoveExtensionSource(args[1..]);

        if (command == "enable" && args.Length > 1)
            return EnableExtensionSource(args[1..]);

        if (command == "disable" && args.Length > 1)
            return DisableExtensionSource(args[1..]);

        if (command == "set-primary" && args.Length > 1)
            return SetPrimarySource(args[1..]);

        return ShowUsage();
    }

    private static int ListExtensionSources()
    {
        return 0;
    }

    private static int AddExtensionSource(string[] args)
    {
        return 0;
    }

    private static int RemoveExtensionSource(string[] args)
    {
        return 0;
    }

    private static int EnableExtensionSource(string[] args)
    {
        return 0;
    }

    private static int DisableExtensionSource(string[] args)
    {
        return 0;
    }

    private static int SetPrimarySource(string[] args)
    {
        // Sets the source that is used if two or more
        // sources offer a package of the same name
        return 0;
    }

    private static int ShowUsage()
    {
        return HelpCommand.DisplayHelpForCommand(Instance);
    }
}
using Dassie.Cli.Commands;
using System;
using System.Text;

namespace Dassie.Extensions;

static class ExtensionSourceManager
{
    public static int HandleArgs(string[] args)
    {
        args ??= [];

        if (args.Length == 1)
            args = [];

        if (args.Length == 0)
            args = ["help"];

        string command = args[0];

        return ShowUsage();
    }

    private static int ShowUsage()
    {
        StringBuilder sb = new();

        sb.AppendLine();
        sb.AppendLine("Usage: dc package source [Command] [Options]");

        sb.AppendLine();
        sb.AppendLine("Available commands:");
        sb.Append($"{"    list",-35}{HelpCommand.FormatLines("Lists all enabled extension sources.", indentWidth: 35)}");
        sb.Append($"{"    add <Url> [Name]",-35}{HelpCommand.FormatLines("Adds a new extension source with an optional name.", indentWidth: 35)}");
        sb.Append($"{"    remove <Url|Name>",-35}{HelpCommand.FormatLines("Removes the specified extension source.", indentWidth: 35)}");
        sb.Append($"{"    help",-35}{HelpCommand.FormatLines("Shows this list.", indentWidth: 35)}");

        HelpCommand.DisplayLogo();
        Console.Write(sb.ToString());
        return 0;
    }
}
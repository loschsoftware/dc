using Dassie.Configuration;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Dassie.Cli.Commands;

internal class HelpCommand : ICompilerCommand
{
    private static HelpCommand _instance;
    public static HelpCommand Instance => _instance ??= new();

    public string Command => "help";

    public List<string> Aliases() => ["?", "-h", "-help", "--help", "-?", "/?", "/help"];

    public string Description => "Lists all available commands.";

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = "Shows a list of available subcommands.",
        Usage = ["dc help [(-o|--options) | (-s|--simple) | --no-external]"],
        Options =
        [
            ("-o|--options", "Shows a list of all available project file properties."),
            ("-s|--simple", "Shows a simplified selection of commands suitable for minimalist developers."),
            ("--no-external", "Does not display commands defined by external packages.")
        ]
    };

    public int Invoke(string[] args)
    {
        if (args.Any(a => !a.StartsWith('-')))
        {
            if (CommandHandler.TryInvoke(args.First(a => !a.StartsWith('-')), ["--help"], out int exit))
                return exit;
        }

        return DisplayHelpMessage(args);
    }

    public static void DisplayLogo()
    {
        StringBuilder logoBuilder = new();

        Version v = Assembly.GetExecutingAssembly().GetName().Version;

        // 8517 -> Days between 01/01/2000 and 27/04/2023, on which development on dc was started
        Version version = new(v.Major, v.Minor, v.Build - 8517);
        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(v.Build);

        logoBuilder.AppendLine();
        logoBuilder.AppendLine($"{ProductName} for .NET {typeof(object).Assembly.GetName().Version.ToString(2)}");
        logoBuilder.AppendLine($"Version {version.ToString(2)}, Build {version.Build} ({buildDate.ToShortDateString()})");

        ConsoleColor def = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOut.Write(logoBuilder.ToString());
        Console.ForegroundColor = def;
    }

    public static string FormatLines(string text, bool initialPadLeft = false, int indentWidth = 50)
    {
        int maxWidth = Console.BufferWidth - 50 - 5;

        if (maxWidth < 30)
            return $"{text}{Environment.NewLine}";

        StringBuilder sb = new();
        string[] words = text.Split(' ');

        while (words.Length > 0)
        {
            StringBuilder lineBuilder = new();

            while (words.Length > 0 && lineBuilder.Length + words[0].Length < maxWidth)
            {
                lineBuilder.Append(words[0] + " ");
                words = words.Skip(1).ToArray();
            }

            string line = lineBuilder.ToString();

            if (initialPadLeft || sb.Length > 0)
                line = line.PadLeft(indentWidth + line.Length);

            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    private static string GetPropertyTypeName(Type t)
    {
        if (t == typeof(bool))
            return "Bool";

        if (t == typeof(string))
            return "String";

        if (t == typeof(int))
            return "Int";

        if (t.IsArray)
            return $"List<{GetPropertyTypeName(t.GetElementType())}>";

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
            return $"List<{GetPropertyTypeName(t.GetGenericArguments().First())}>";

        if (t.IsEnum)
            return "Enum";

        return "Object";
    }

    private static int DisplayHelpMessage(string[] args)
    {
        // "Tsoding mode", to appeal to "minimalist developers"
        if (args.Any(a => a == "-s" || a == "--simple"))
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Version version = new(v.Major, v.Minor, v.Build - 8517);
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(v.Build);

            StringBuilder sb = new();
            sb.AppendLine();
            sb.AppendLine($"Dassie Compiler - Build {version.Build} ({buildDate.ToShortDateString()})");
            sb.AppendLine("Simplified Mode");
            sb.AppendLine();
            sb.AppendLine("Usage: dc <Files>");
            sb.AppendLine("       Compiles the specified source files.");
            LogOut.Write(sb.ToString());

            return 0;
        }

        DisplayLogo();

        if (args.Any(a => a == "-o" || a == "--options"))
        {
            StringBuilder outputBuilder = new();

            string header = $"{"A "}{"Name",-30}{"Type",-20}{"Default",-10}{"Description"}";
            outputBuilder.AppendLine();
            outputBuilder.AppendLine(header);
            outputBuilder.AppendLine(new string('-', Console.WindowWidth));

            int descriptionWidth = Console.WindowWidth - 62 - 5;

            PropertyInfo[] properties = typeof(DassieConfig).GetProperties();
            List<(string PropertyName, string Text)> propertyLines = [];

            foreach (PropertyInfo property in properties)
            {
                string name = property.Name;

                XmlElementAttribute element = property.GetCustomAttribute<XmlElementAttribute>();
                XmlAttributeAttribute attrib = property.GetCustomAttribute<XmlAttributeAttribute>();

                if (element != null && !string.IsNullOrEmpty(element.ElementName))
                    name = element.ElementName;

                if (attrib != null && !string.IsNullOrEmpty(attrib.AttributeName))
                    name = attrib.AttributeName;

                string defaultVal = "";

                DefaultValueAttribute defaultValAttrib = property.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultValAttrib != null)
                    defaultVal = (defaultValAttrib.Value ?? "").ToString();

                string alias = "";
                if (CommandLineOptionParser.Aliases.ContainsValue(name))
                    alias = CommandLineOptionParser.Aliases.First(a => a.Value == name).Key;

                string descriptionText = CommandLineOptionParser.GetDescription(property.Name);
                string descriptionFormatted = FormatLines(descriptionText, false, 62);

                propertyLines.Add((name, $"{alias,-1} {property.Name,-30}{GetPropertyTypeName(property.PropertyType),-20}{defaultVal,-10}{descriptionFormatted}"));
            }

            foreach (string prop in propertyLines.OrderBy(p => p.PropertyName).Select(p => p.Text))
                outputBuilder.Append(prop);

            LogOut.WriteLine(outputBuilder.ToString());
            return 0;
        }

        return DisplayHelpMessage();
    }

    private static int DisplayHelpMessage()
    {
        if (LogOut == Console.Out && Console.BufferWidth - 50 - 5 < 30)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Increase the console width for a better viewing experience.");
            Console.ForegroundColor = prev;
        }

        StringBuilder sb = new();

        sb.AppendLine();
        sb.AppendLine("Usage:");

        sb.Append("    dc <FileNames> [Options]".PadRight(35));
        sb.Append(FormatLines("Compiles the specified source files. Use 'dc help compile' for more information."));

        sb.Append("    dc [Command] [Options]".PadRight(35));
        sb.Append(FormatLines("Executes a command from the list below."));

        sb.Append("    dc help <Command>".PadRight(35));
        sb.Append(FormatLines("Displays more information about a specific command."));

        sb.AppendLine();
        sb.AppendLine("Commands:");

        IEnumerable<ICompilerCommand> internalCommands = ExtensionLoader.Commands.OrderBy(c => c.Command).Where(c => !c.Hidden()
            && c.GetType().Assembly == Assembly.GetExecutingAssembly());

        IEnumerable<ICompilerCommand> externalCommands = ExtensionLoader.Commands.Where(c => !c.Hidden()).Except(internalCommands);

        foreach (ICompilerCommand command in internalCommands)
        {
            sb.Append($"    {command.Command}".PadRight(35));
            sb.Append(FormatLines(command.Description));
        }

        if (externalCommands.Any() && !Environment.GetCommandLineArgs().Any(c => c == "--no-external"))
        {
            sb.AppendLine();
            sb.AppendLine("External commands:");

            foreach (ICompilerCommand cmd in externalCommands)
                sb.Append($"{$"    {cmd.Command}",-35}{FormatLines(cmd.Description)}");
        }

        LogOut.Write(sb.ToString());
        return 0;
    }

    public static int DisplayHelpForCommand(ICompilerCommand command)
    {
        if (command.HelpDetails() == null)
        {
            DisplayLogo();
            LogOut.WriteLine();
            LogOut.WriteLine($"The command '{command.Command}' does not define help details.");
            return 0;
        }

        CommandHelpDetails hd = command.HelpDetails();
        StringBuilder sb = new();

        sb.AppendLine();
        sb.AppendLine($"dc {command.Command}: {(string.IsNullOrEmpty(hd.Description) ? command.Description : hd.Description)}");
        sb.AppendLine();

        sb.Append("Usage:");
        if (hd.Usage == null || hd.Usage.Count == 0)
            sb.AppendLine($" {command.Command}");
        else if (hd.Usage.Count == 1)
            sb.AppendLine($" {hd.Usage[0]}");
        else
        {
            sb.AppendLine();
            foreach (string usage in hd.Usage)
                sb.Append(FormatLines(usage, true, 4));
        }

        sb.AppendLine();

        if (hd.Options != null && hd.Options.Count > 0)
        {
            sb.AppendLine("Options:");
            foreach ((string opt, string desc) in hd.Options)
                sb.Append($"{$"    {opt}",-35}{FormatLines(desc, indentWidth: 35)}");
            sb.AppendLine();
        }

        if (hd.CustomSections != null && hd.CustomSections.Count > 0)
        {
            foreach ((string header, string content) in hd.CustomSections)
            {
                sb.AppendLine($"{header}:");
                sb.AppendLine(content);
            }
        }

        if (!string.IsNullOrEmpty(hd.Remarks))
        {
            sb.AppendLine("Remarks:");
            sb.AppendLine(FormatLines(hd.Remarks, true, 4));
        }

        DisplayLogo();
        LogOut.Write($"{sb}\b");

        return 0;
    }
}
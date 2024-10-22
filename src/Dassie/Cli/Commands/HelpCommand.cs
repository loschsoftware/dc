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
    private readonly Dictionary<string, string> _installedCommands;

    public HelpCommand(Dictionary<string, string> commands) => _installedCommands = commands;

    public string Command => "help";

    public List<string> Aliases() => ["?"];

    public string UsageString => "help, ? [-o, --options]";

    public string Description => "Shows this page. Use the -o flag to display all available options.";

    public string Help => @"
help command";

    public int Invoke(string[] args) => DisplayHelpMessage(args, _installedCommands);

    public static void DisplayLogo()
    {
        Version v = Assembly.GetExecutingAssembly().GetName().Version;

        // 8517 -> Days between 01/01/2000 and 27/04/2023, on which development on dc was started
        Version version = new(v.Major, v.Minor, v.Build - 8517);
        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(v.Build);

        ConsoleColor def = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Yellow;

        LogOut.WriteLine();
        LogOut.WriteLine("Dassie Command Line Compiler for .NET");
        LogOut.WriteLine($"Version {version.ToString(2)}, Build {version.Build} ({buildDate.ToShortDateString()})");

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

    private static int DisplayHelpMessage(string[] args, Dictionary<string, string> installedCommands)
    {
        DisplayLogo();

        if (args.Any(a => a == "-o" || a == "--options"))
        {
            StringBuilder outputBuilder = new();

            string header = $"{"A "}{"Name",-40}{"Type",-20}{"Default"}";
            outputBuilder.AppendLine();
            outputBuilder.AppendLine(header);
            outputBuilder.AppendLine(new string('-', header.Length));

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
                    alias = CommandLineOptionParser.Aliases.First(a => a.Value == name).Key.ToLowerInvariant();

                propertyLines.Add((name, $"{alias,-1} {property.Name,-40}{GetPropertyTypeName(property.PropertyType),-20}{defaultVal}"));
            }

            foreach (string prop in propertyLines.OrderBy(p => p.PropertyName).Select(p => p.Text))
                outputBuilder.AppendLine(prop);

            Console.WriteLine(outputBuilder.ToString());
            return 0;
        }

        return DisplayHelpMessage(installedCommands);
    }

    private static int DisplayHelpMessage(Dictionary<string, string> installedCommands)
    {
        if (Console.BufferWidth - 50 - 5 < 30)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Increase the console width for a better viewing experience.");
            Console.ForegroundColor = prev;
        }

        StringBuilder sb = new();

        sb.AppendLine();
        sb.AppendLine("Usage:");

        sb.Append("    dc [Command] [Options]".PadRight(50));
        sb.Append(FormatLines("Executes a command from the list below."));

        sb.Append("    dc <FileNames> [Options]".PadRight(50));
        sb.AppendLine("Compiles the specified source files.");

        sb.AppendLine();
        sb.AppendLine("Commands:");

        sb.Append("    new <Type> <Name>".PadRight(50));

        sb.AppendLine("Creates the file structure of a Dassie project.");

        sb.Append("        console".PadRight(25).PadRight(50));
        sb.AppendLine("Specifies a command-line application.");
        sb.Append("        library".PadRight(25).PadRight(50));
        sb.AppendLine("Specifies a dynamic link library.");
        sb.AppendLine();

        sb.Append("    build [BuildProfile]".PadRight(50));
        sb.Append(FormatLines("Executes the specified build profile, or compiles all .ds source files in the current directory if none is specified."));

        sb.Append("    run [Arguments]".PadRight(50));
        sb.Append(FormatLines("Automatically compiles using the default profile and then runs the output executable with the specified arguments."));

        sb.Append("    watch, auto".PadRight(50));
        sb.Append(FormatLines("Watches all .ds files in the current folder structure and automatically recompiles when files are changed."));

        sb.Append("    quit".PadRight(50));
        sb.Append(FormatLines("Stops all file watchers."));

        sb.Append("    scratchpad [Command] [Options]".PadRight(50));
        sb.Append(FormatLines("Allows compiling and running Dassie source code from the console. Use 'dc scratchpad help' to display available commands."));

        sb.Append("    config".PadRight(50));
        sb.Append(FormatLines("Creates a new dsconfig.xml file with default values."));

        sb.Append("    package [Command] [Options]".PadRight(50));
        sb.Append(FormatLines("Used to install and manage compiler extensions. Use 'dc package help' to display available commands."));

        sb.Append("    help, ? [-o, --options]".PadRight(50));
        sb.Append(FormatLines("Shows this page. Use the -o flag to display all available options."));

        if (installedCommands.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("External commands:");

            foreach (KeyValuePair<string, string> cmd in installedCommands)
                sb.Append($"{$"    {cmd.Key}",-50}{FormatLines(cmd.Value.Replace(Environment.NewLine, " "))}");
        }

        sb.AppendLine();
        sb.AppendLine("Options:");
        sb.AppendLine(FormatLines("Options from project files (dsconfig.xml) can be included in the following way:", true, 4));

        sb.Append("    --<PropertyName>=<Value>".PadRight(50));
        sb.Append(FormatLines("For simple properties of type 'string', 'bool' or 'enum'. The property name is case-insensitive. For boolean properties, 0 and 1 are supported aliases for false and true. Example: --MeasureElapsedTime=true"));

        sb.Append("    --<ArrayPropertyName>+<Value>".PadRight(50));
        sb.Append(FormatLines("To add elements to an array property. Property names are recognized by the first characters, where 'References' takes precedence over 'Resources'. Example: --R+\"assembly.dll\""));

        sb.Append("    --<PropertyName>::<ChildProperty>=<Value>".PadRight(50));
        sb.Append(FormatLines("For setting child properties of more complex objects. Object names are recognized by first characters. Example: --VersionInfo::Description=\"Application\""));

        LogOut.Write(sb.ToString());
        return 0;
    }
}
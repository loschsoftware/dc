﻿using Dassie.Configuration;
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

    public string UsageString => "help, ? [(-o|--options)]";

    public string Description => "Shows this page. Use the -o flag to display all available options.";

    public int Invoke(string[] args)
    {
        if (args.Any(a => !a.StartsWith('-')))
            return DisplayHelpForCommand(args.First(a => !a.StartsWith('-')));

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

            Console.WriteLine(outputBuilder.ToString());
            return 0;
        }

        return DisplayHelpMessage();
    }

    private static int DisplayHelpMessage()
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

        IEnumerable<ICompilerCommand> internalCommands = CommandRegistry.Commands.OrderBy(c => c.Command).Where(c => !c.Hidden()
            && c.GetType().Assembly == Assembly.GetExecutingAssembly());

        IEnumerable<ICompilerCommand> externalCommands = CommandRegistry.Commands.Where(c => !c.Hidden()).Except(internalCommands);

        foreach (ICompilerCommand command in internalCommands)
        {
            sb.Append($"    {command.UsageString}".PadRight(50));
            sb.Append(FormatLines(command.Description));
        }

        if (externalCommands.Any())
        {
            sb.AppendLine();
            sb.AppendLine("External commands:");

            foreach (ICompilerCommand cmd in externalCommands)
                sb.Append($"{$"    {cmd.UsageString}",-50}{FormatLines(cmd.Description)}");
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

    private static int DisplayHelpForCommand(string name)
    {
        LogOut.WriteLine("Command-specific help is not yet available. Refer to the online documentation for help.");
        return 0;
    }
}
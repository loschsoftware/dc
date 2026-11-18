using Dassie.Core;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dassie.Cli.Help;

#pragma warning disable CS9113

/// <summary>
/// Provides helpers for generating compiler documentation files in Markdown format.
/// </summary>
internal static class DocumentationGenerator
{
    private const char Arrow = '↳';

    [InterpolatedStringHandler]
    private readonly struct MarkdownInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        private readonly StringBuilder _sb = new(literalLength);

        public readonly void AppendLiteral(string s)
        {
            _sb.Append(s);
        }

        public readonly void AppendFormatted<T>(T t)
        {
            _sb.Append(t?.ToString()
                .Replace("\\", "\\\\")
                .Replace("|", "\\|"));
        }

        public static implicit operator MarkdownInterpolatedStringHandler(string value)
        {
            MarkdownInterpolatedStringHandler handler = new(value.Length, 0);
            handler.AppendLiteral(value);
            return handler;
        }

        public override string ToString() => _sb.ToString();
    }

    private class MarkdownWriter
    {
        private readonly StringBuilder _sb = new();

        public void Heading1(MarkdownInterpolatedStringHandler text)
            => AppendLine($"# {text}{Environment.NewLine}");

        public void Heading2(MarkdownInterpolatedStringHandler text)
            => AppendLine($"## {text}{Environment.NewLine}");

        public void Paragraph(MarkdownInterpolatedStringHandler text)
            => AppendLine($"{text}{Environment.NewLine}");

        public void AppendLine()
            => AppendLine("");

        public void AppendLine(MarkdownInterpolatedStringHandler text)
            => _sb.AppendLine(text.ToString());

        public override string ToString()
        {
            return _sb.ToString();
        }
    }

    private static string Link(string text, string target) =>
        $"[{text}]({target.Replace(Path.DirectorySeparatorChar, '/')})";

    /// <summary>
    /// Generates Markdown documentation for all installed commands, including
    /// individual files for each command as well as an index file.
    /// <para/>
    /// Files are generated in the current directory.
    /// </summary>
    /// <param name="includeExternal">Whether or not to also generate documentation for commands defined by external extensions.</param>
    /// <returns>A status code; 0 indicates success.</returns>
    public static int GenerateDocumentation(bool includeExternal = true)
    {
        GenerateGlobalIndex(includeExternal);
        return 0;
    }

    private static void GenerateGlobalIndex(bool includeExternal)
    {
        MarkdownWriter writer = new();
        writer.Heading1(StringHelper.DocumentationGenerator_DassieCompilerDocumentation);
        writer.Heading2(StringHelper.DocumentationGenerator_Extensions);

        foreach (IExtension extension in includeExternal ? ExtensionLoader.InstalledExtensions : [CorePackage.Instance])
        {
            GenerateForPackage(extension);
            writer.AppendLine($"- {Link(extension.Metadata.Name, Path.Combine(".", extension.Metadata.Name, "index.md"))}");
        }

        File.WriteAllText("index.md", writer.ToString());
    }

    private static void GenerateForPackage(IExtension extension)
    {
        Directory.CreateDirectory(extension.Metadata.Name);

        MarkdownWriter writer = new();
        writer.Heading1(StringHelper.Format(nameof(StringHelper.DocumentationGenerator_Package), extension.Metadata.Name));
        writer.Heading2(StringHelper.DocumentationGenerator_PackageDetails);
        writer.AppendLine($"|{StringHelper.DocumentationGenerator_Field}|{StringHelper.DocumentationGenerator_Value}|");
        writer.AppendLine("|--|--|");
        writer.Paragraph($"""
            |{StringHelper.PackageCommand_Name}|{extension.Metadata.Name}|
            |{StringHelper.PackageCommand_AuthorNoColon}|{extension.Metadata.Author}|
            |{StringHelper.PackageCommand_Version}|{extension.Metadata.Version}|
            """);

        writer.Paragraph($"**{StringHelper.DocumentationGenerator_Hierarchy}**");

        List<IExtension> parentHierarchy = [extension];
        IExtension current = extension;

        while (current.ParentExtension is Type t)
        {
            IExtension parent = ExtensionLoader.InstalledExtensions.FirstOrDefault(e => e.GetType() == t);

            if (parent == null)
                break;

            current = parent;
            parentHierarchy.Add(current);
        }

        IExtension baseExt = parentHierarchy[^1];
        writer.AppendLine($"{Link(baseExt.Metadata.Name, Path.Combine("..", baseExt.Metadata.Name, "index.md"))}  ");

        int indentDepth = 0;

        for (int i = parentHierarchy.Count - 2; i >= 0; i--, indentDepth++)
        {
            IExtension ext = parentHierarchy[i];
            string name = ext.Metadata.Name;

            writer.AppendLine($"{new string(' ', 2 * indentDepth)}{Arrow} {Link(name, Path.Combine("..", name, "index.md"))}  ");
        }

        writer.AppendLine();

        if (extension.Commands()?.Length > 0)
        {
            writer.Heading2(StringHelper.DocumentationGenerator_Commands);
            writer.AppendLine($"|{StringHelper.DocumentationGenerator_Command}|{StringHelper.DocumentationGenerator_Description}|");
            writer.AppendLine("|--|--|");

            foreach (ICompilerCommand command in extension.Commands().Where(c => !c.Options.HasFlag(CommandOptions.Hidden)))
            {
                GenerateForCommand(Path.Combine(".", extension.Metadata.Name), command);
                writer.AppendLine($"|{Link($"`{command.Command}`", Path.Combine(".", $"{command.Command}.md"))}|{command.Description}");
            }
        }
        else
            writer.Paragraph(StringHelper.DocumentationGenerator_ExtensionNoCommands);

        File.WriteAllText(Path.Combine(extension.Metadata.Name, "index.md"), writer.ToString());
    }

    private static void GenerateForCommand(string baseDir, ICompilerCommand command)
    {
        MarkdownWriter writer = new();

        StringBuilder parentCommandSb = new(" ");
        StringBuilder commandIdSb = new();
        ICompilerCommand currentParent = command.Parent;
        while (currentParent != null)
        {
            parentCommandSb.Insert(1, $"{currentParent.Command} ");
            commandIdSb.Insert(0, $"{currentParent.Command}.");
            currentParent = currentParent.Parent;
        }

        commandIdSb.Append(command.Command);
        string commandId = commandIdSb.ToString();

        writer.Heading1($"dc{parentCommandSb}{command.Command} – {command.Description}{Environment.NewLine}");

        if (command.Aliases?.Count > 0)
        {
            writer.AppendLine($"**{(command.Aliases.Count == 1 ? StringHelper.HelpCommand_AliasSingular : StringHelper.HelpCommand_AliasPlural)}**");
            foreach (string alias in command.Aliases)
                writer.AppendLine($"- `{alias}`");
            writer.AppendLine();
        }

        if (command.Subcommands?.Count > 0)
        {
            writer.Heading2(StringHelper.DocumentationGenerator_Subcommands);
            writer.AppendLine($"|{StringHelper.DocumentationGenerator_Command}|{StringHelper.DocumentationGenerator_Description}|");
            writer.AppendLine("|--|--|");

            foreach (ICompilerCommand subcommand in command.Subcommands)
            {
                GenerateForCommand(baseDir, subcommand);
                writer.AppendLine($"|{Link(subcommand.Command, Path.Combine(".", $"{commandId}.{subcommand.Command}.md"))}|{subcommand.Description}|");
            }
        }

        if (command.HelpDetails is CommandHelpDetails hd)
        {
            if (hd.Usage?.Count > 0)
            {
                writer.Heading2(StringHelper.DocumentationGenerator_Usage);
                foreach (string usage in hd.Usage)
                    writer.AppendLine($"- {usage}");
                writer.AppendLine();
            }

            if (hd.Options?.Count > 0)
            {
                writer.Heading2(StringHelper.DocumentationGenerator_Options);
                writer.AppendLine($"|{StringHelper.DocumentationGenerator_Option}|{StringHelper.DocumentationGenerator_Description}|");
                writer.AppendLine("|--|--|");
                foreach ((string option, string description) in hd.Options)
                    writer.AppendLine($"|{option}|{description}|");
                writer.AppendLine();
            }

            if (hd.CustomSections?.Count > 0)
            {
                foreach ((string heading, string text) in hd.CustomSections)
                {
                    writer.Heading2(heading);
                    writer.Paragraph(text);
                }
            }

            if (!string.IsNullOrEmpty(hd.Remarks))
            {
                writer.Heading2(StringHelper.DocumentationGenerator_Remarks);
                writer.Paragraph(hd.Remarks);
            }

            if (hd.Examples?.Count > 0)
            {
                writer.Heading2(StringHelper.DocumentationGenerator_Examples);
                writer.AppendLine($"|{StringHelper.DocumentationGenerator_Example}|{StringHelper.DocumentationGenerator_Description}|");
                writer.AppendLine("|--|--|");
                foreach ((string example, string description) in hd.Examples)
                    writer.AppendLine($"|{example}|{description}|");
                writer.AppendLine();
            }
        }
        else
            writer.Paragraph(StringHelper.DocumentationGenerator_NoHelpDetails);

        string path = Path.Combine(baseDir, $"{commandId}.md");
        File.WriteAllText(path, writer.ToString());
    }
}
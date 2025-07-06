using Dassie.Cli;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace Dassie.Errors.Devices;

/// <summary>
/// Provides a singleton build log device used for logging to one or more <see cref="TextWriter"/> objects.
/// </summary>
internal class TextWriterBuildLogDevice : IBuildLogDevice
{
    private TextWriterBuildLogDevice() { }
    private static TextWriterBuildLogDevice _instance = null;
    public static TextWriterBuildLogDevice Instance => _instance ??= new();

    /// <summary>
    /// The output text writer used for error messages.
    /// </summary>
    public static ErrorTextWriter ErrorOut { get; set; } = new([Console.Error]);
    /// <summary>
    /// The output text writer used for warning messages.
    /// </summary>
    public static ErrorTextWriter WarnOut { get; set; } = new([Console.Out]);
    /// <summary>
    /// The output text writer used for information messages.
    /// </summary>
    public static ErrorTextWriter InfoOut { get; set; } = new([Console.Out]);

    public string Name => "Default";
    public BuildLogSeverity SeverityLevel => BuildLogSeverity.All;

    public void Initialize(List<XmlAttribute> attributes, List<XmlNode> elements) { }

    public void WriteString(string input)
    {
        InfoOut.Write(input);
    }

    public void Log(ErrorInfo error)
    {
        Log(error, false, true);
    }

    internal static void Log(ErrorInfo error, TextWriter output, bool treatAsError = false, bool addToErrorList = true, bool applyFormatting = false)
    {
        try
        {
            ConsoleColor defaultColor = Console.ForegroundColor;

            StringBuilder outBuilder = new();

            void SetColorRgb(byte r, byte g, byte b)
            {
                if (applyFormatting || (ConsoleHelper.AnsiEscapeSequenceSupported && (output == Console.Out || output == Console.Error)))
                    outBuilder.Append($"\x1b[38;2;{r};{g};{b}m");
            }

            void SetColor()
            {
                Context.Configuration.ErrorColor ??= "#FE4A49";
                Context.Configuration.WarningColor ??= "#FFA500";
                Context.Configuration.MessageColor ??= "#1E90FF";

                Color color = ColorTranslator.FromHtml(error.Severity switch
                {
                    Severity.Error => Context.Configuration.ErrorColor,
                    Severity.Warning => Context.Configuration.WarningColor,
                    Severity.Information => Context.Configuration.MessageColor,
                    _ => "#CCCCCC"
                });

                SetColorRgb(color.R, color.G, color.B);
            }

            void ResetColor()
            {
                if (applyFormatting || (ConsoleHelper.AnsiEscapeSequenceSupported && (output == Console.Out || output == Console.Error)))
                    outBuilder.Append($"\x1b[0m");
            }

            Console.CursorLeft = 0;
            Console.ForegroundColor = ConsoleColor.DarkGray;

            string prefix = "\r\n";

            if (!string.IsNullOrEmpty(MessagePrefix) && error.Severity != Severity.BuildLogMessage)
            {
                outBuilder.Append($"\r\n[{MessagePrefix}] ");
                prefix = "";
            }

            outBuilder.Append(prefix);

            string severityPrefix = "";
            if (Context.Configuration.EnableSeverityIndicators)
            {
                severityPrefix = error.Severity switch
                {
                    Severity.Information => "ℹ️ ",
                    Severity.Warning => "⚠️ ",
                    Severity.Error => "❌ ",
                    _ => ""
                };
            }

            if (Context.Configuration.EnableMessageTimestamps)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                outBuilder.Append($"[{DateTime.Now}] ");
            }

            string errCode = error.ErrorCode == ErrorKind.CustomError ? error.CustomErrorCode : error.ErrorCode.ToString().Split('_')[0];
            string codePos = "\b";

            // Legacy colors
            if (!ConsoleHelper.AnsiEscapeSequenceSupported && (output == Console.Out || output == Console.Error))
            {
                Console.ForegroundColor = error.Severity switch
                {
                    Severity.Error => ConsoleColor.Red,
                    Severity.Warning => ConsoleColor.Yellow,
                    Severity.Information => ConsoleColor.Cyan,
                    _ => ConsoleColor.Gray
                };
            }

            if (!error.HideCodePosition)
                codePos = $"({error.CodePosition.Item1},{error.CodePosition.Item2})";

            if (error.Severity == Severity.BuildLogMessage)
                outBuilder.AppendLine($"{error.ErrorMessage}");
            else
            {
                SetColorRgb(120, 120, 120);
                outBuilder.Append(Path.GetFileName(error.File));
                outBuilder.Append(' ');
                outBuilder.Append(codePos);
                outBuilder.Append(": ");
                ResetColor();

                if (error.Source != ErrorSource.Compiler)
                {
                    SetColorRgb(34, 139, 34);
                    outBuilder.Append($"[{error.Source.ToString()}] ");
                    ResetColor();
                }

                SetColor();
                outBuilder.Append($"{severityPrefix}{error.Severity switch
                {
                    Severity.Error => "error",
                    Severity.Warning => "warning",
                    _ => "message"
                }}");
                outBuilder.Append(' ');
                outBuilder.Append(errCode);
                outBuilder.Append(": ");
                ResetColor();

                SetColorRgb(255, 255, 255);
                outBuilder.AppendLine(error.ErrorMessage);
                ResetColor();
            }

            output.Write(outBuilder.ToString());
            output.Flush();
            outBuilder.Clear();

            if (!string.IsNullOrEmpty(error.Tip) && Context.Configuration.EnableTips)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                InfoOut.WriteLine(error.Tip);
            }

            Console.ForegroundColor = defaultColor;

            if (Context.Configuration.AdvancedErrorMessages && error.Severity != Severity.BuildLogMessage)
            {
                try
                {
                    outBuilder.AppendLine();

                    using StreamReader sr = new(CurrentFile.Path);
                    string line = "";

                    for (int i = 0; i < error.CodePosition.Item1; i++, line = sr.ReadLine()) ;

                    outBuilder.AppendLine(line);
                    outBuilder.Append(new string(' ', error.CodePosition.Item2));

                    ResetColor();
                    output.Write(outBuilder.ToString());
                    outBuilder.Clear();

                    Console.ForegroundColor = error.Severity switch
                    {
                        Severity.Error => ConsoleColor.DarkRed,
                        Severity.Warning => ConsoleColor.DarkYellow,
                        _ => ConsoleColor.DarkCyan
                    };

                    outBuilder.Append('^');
                    outBuilder.AppendLine(new string('~', Math.Max(error.Length, 0)));

                    ResetColor();
                    output.Write(outBuilder.ToString());
                    outBuilder.Clear();

                    Console.ForegroundColor = defaultColor;
                }
                catch (Exception)
                {
                    ResetColor();
                    Console.ForegroundColor = defaultColor;

                    if (addToErrorList)
                        CurrentFile.Errors.Add(error);

                    if (treatAsError || error.Severity == Severity.Error)
                        CurrentFile.CompilationFailed = true;
                }
            }

            if (treatAsError || error.Severity == Severity.Error)
                CurrentFile.CompilationFailed = true;

            if (addToErrorList)
                CurrentFile.Errors.Add(error);

            ResetColor();
            output.Write(outBuilder.ToString());
        }
        catch (IOException)
        {
            CurrentFile.Errors.Add(error);
            return;
        }
    }

    internal static void Log(ErrorInfo error, bool treatAsError = false, bool addToErrorList = true, bool applyFormatting = true)
    {
        var outStream = error.Severity switch
        {
            Severity.Error => ErrorOut,
            Severity.Warning => WarnOut,
            _ => InfoOut
        };

        foreach (TextWriter writer in outStream.Writers)
            Log(error, writer);
    }
}
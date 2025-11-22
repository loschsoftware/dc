using Dassie.Cli;
using Dassie.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

#pragma warning disable CS0420

namespace Dassie.Messages.Devices;

/// <summary>
/// Provides a singleton build log device used for logging to one or more <see cref="TextWriter"/> objects.
/// </summary>
internal class TextWriterBuildLogDevice : IBuildLogDevice
{
    private static readonly Lock _lockObject = new();
    private static volatile TextWriterBuildLogDevice _instance = null;

    private TextWriterBuildLogDevice() { }

    public static TextWriterBuildLogDevice Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    _instance ??= new();
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// The output text writer used for error messages.
    /// </summary>
    public static MessageTextWriter ErrorOut { get; set; } = new([Console.Error]);
    /// <summary>
    /// The output text writer used for warning messages.
    /// </summary>
    public static MessageTextWriter WarnOut { get; set; } = new([Console.Out]);
    /// <summary>
    /// The output text writer used for information messages.
    /// </summary>
    public static MessageTextWriter InfoOut { get; set; } = new([Console.Out]);

    public string Name => "Default";
    public BuildLogSeverity SeverityLevel => BuildLogSeverity.All;

    private volatile bool _disabled;

    public void Initialize(List<XmlAttribute> attributes, List<XmlNode> elements)
    {
        if (attributes != null && attributes.Any(a => a.Name == "Disabled"))
            _ = bool.TryParse(attributes.First(a => a.Name == "Disabled").InnerText, out _disabled);
    }

    public void WriteString(string input)
    {
        if (_disabled)
            return;

        InfoOut.Write(input);
    }

    public void Log(MessageInfo error)
    {
        if (_disabled)
            return;

        Log(error, false, true);
    }

    internal static void Log(MessageInfo error, TextWriter output, bool treatAsError = false, bool addToErrorList = true, bool applyFormatting = false)
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

            lock (_lockObject)
            {
                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            string prefix = "\r\n";

            if (error.Severity == Severity.BuildLogMessage)
                prefix = "";

            string messagePrefix = MessagePrefix; // Thread-safe read
            if (!string.IsNullOrEmpty(messagePrefix) && error.Severity != Severity.BuildLogMessage)
            {
                outBuilder.Append($"\r\n[{messagePrefix}] ");
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

            if (Context.Configuration.EnableMessageTimestamps || error.Severity == Severity.BuildLogMessage)
            {
                SetColorRgb(120, 120, 120);
                outBuilder.Append($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] ");
                SetColor();
            }

            string errCode = error.Code == Custom ? error.CustomCode : error.Code.ToString().Split('_')[0];
            string codePos = "\b";

            // Legacy colors
            if (!ConsoleHelper.AnsiEscapeSequenceSupported && (output == Console.Out || output == Console.Error))
            {
                lock (_lockObject)
                {
                    Console.ForegroundColor = error.Severity switch
                    {
                        Severity.Error => ConsoleColor.Red,
                        Severity.Warning => ConsoleColor.Yellow,
                        Severity.Information => ConsoleColor.Cyan,
                        _ => ConsoleColor.Gray
                    };
                }
            }

            if (!error.HideCodePosition)
                codePos = $"({error.Location.Line},{error.Location.Column})";

            string fileError = Path.GetFileName(error.File);

            if (string.IsNullOrEmpty(fileError))
                fileError = error.File;

            if (error.Severity == Severity.BuildLogMessage)
                outBuilder.AppendLine($"{error.Text}");
            else
            {
                if (!error.HideCodePosition || !string.IsNullOrEmpty(fileError))
                {
                    SetColorRgb(120, 120, 120);

                    if (!string.IsNullOrEmpty(fileError))
                    {
                        outBuilder.Append(fileError);
                        outBuilder.Append(' ');
                    }

                    outBuilder.Append(codePos);
                    outBuilder.Append(": ");
                    ResetColor();
                }

                if (error.Source != MessageSource.Compiler)
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

                if (!string.IsNullOrEmpty(errCode))
                {
                    outBuilder.Append(' ');
                    outBuilder.Append(errCode);
                }

                outBuilder.Append(": ");
                ResetColor();

                SetColorRgb(255, 255, 255);
                outBuilder.AppendLine(error.Text);
                ResetColor();
            }

            lock (output)
            {
                output.Write(outBuilder.ToString());
                output.Flush();
            }
            outBuilder.Clear();

            if (!string.IsNullOrEmpty(error.Tip) && Context.Configuration.EnableTips)
            {
                lock (_lockObject)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                InfoOut.WriteLine(error.Tip);
            }

            lock (_lockObject)
            {
                Console.ForegroundColor = defaultColor;
            }

            if (Context.Configuration.AdvancedErrorMessages && error.Severity != Severity.BuildLogMessage)
            {
                try
                {
                    outBuilder.AppendLine();

                    using StreamReader sr = new(CurrentFile.Path);
                    string line = "";

                    for (int i = 0; i < error.Location.Line; i++, line = sr.ReadLine()) ;

                    outBuilder.AppendLine(line);
                    outBuilder.Append(new string(' ', error.Location.Column));

                    ResetColor();
                    lock (output)
                    {
                        output.Write(outBuilder.ToString());
                    }
                    outBuilder.Clear();

                    lock (_lockObject)
                    {
                        Console.ForegroundColor = error.Severity switch
                        {
                            Severity.Error => ConsoleColor.DarkRed,
                            Severity.Warning => ConsoleColor.DarkYellow,
                            _ => ConsoleColor.DarkCyan
                        };
                    }

                    outBuilder.Append('^');
                    outBuilder.AppendLine(new string('~', Math.Max(error.Length, 0)));

                    ResetColor();
                    lock (output)
                    {
                        output.Write(outBuilder.ToString());
                    }
                    outBuilder.Clear();

                    lock (_lockObject)
                    {
                        Console.ForegroundColor = defaultColor;
                    }
                }
                catch (Exception)
                {
                    ResetColor();
                    lock (_lockObject)
                    {
                        Console.ForegroundColor = defaultColor;
                    }

                    if (addToErrorList)
                    {
                        lock (CurrentFile.Errors)
                        {
                            CurrentFile.Errors.Add(error);
                        }
                    }

                    if (treatAsError || error.Severity == Severity.Error)
                        CurrentFile.CompilationFailed = true;
                }
            }

            if (treatAsError || error.Severity == Severity.Error)
                CurrentFile.CompilationFailed = true;

            if (addToErrorList)
            {
                lock (CurrentFile.Errors)
                {
                    CurrentFile.Errors.Add(error);
                }
            }

            ResetColor();
            lock (output)
            {
                output.Write(outBuilder.ToString());
            }
        }
        catch (IOException)
        {
            lock (CurrentFile.Errors)
            {
                CurrentFile.Errors.Add(error);
            }
            return;
        }
    }

    internal static void Log(MessageInfo error, bool treatAsError = false, bool addToErrorList = true, bool applyFormatting = true)
    {
        var outStream = error.Severity switch
        {
            Severity.Error => ErrorOut,
            Severity.Warning => WarnOut,
            _ => InfoOut
        };

        TextWriter[] writers;
        lock (outStream)
        {
            writers = outStream.Writers.ToArray();
        }

        foreach (TextWriter writer in writers)
            Log(error, writer, treatAsError, addToErrorList, applyFormatting);
    }
}
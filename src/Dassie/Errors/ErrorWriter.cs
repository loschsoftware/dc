using Dassie.Cli;
using Dassie.Configuration;
using Dassie.Configuration.Analysis;
using Dassie.Errors.Devices;
using Dassie.Extensions;
using Dassie.Meta;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Dassie.Errors;

/// <summary>
/// Provides methods for emitting Dassie error messages.
/// </summary>
public static class ErrorWriter
{
    static ErrorWriter()
    {
        CurrentFile ??= new("");
        Context ??= new();
        Context.Configuration ??= new();
    }

    internal static readonly List<ErrorInfo> messages = new();

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

    /// <summary>
    /// A list of additional build log devices to use.
    /// </summary>
    public static List<BuildLogDeviceContext> BuildLogDevices { get; } = [];

    /// <summary>
    /// A prefix of all error messages, indicating which project the error is from.
    /// </summary>
    public static string MessagePrefix { get; set; } = "";

    /// <summary>
    /// Contains configuration for the error writer.
    /// </summary>
    public static DassieConfig Config { get; set; } = new();

    /// <summary>
    /// Used to completely disable the error writer.
    /// </summary>
    public static bool Disabled { get; set; } = false;

    /// <summary>
    /// A value added to the line number of every error message.
    /// </summary>
    public static int LineNumberOffset { get; set; } = 0;

    internal static void EmitGeneric(ErrorInfo error, bool treatAsError = false, bool addToErrorList = true)
    {
        if (Disabled)
            return;

        error.CodePosition = (error.CodePosition.Item1 + LineNumberOffset, error.CodePosition.Item2);

        Context ??= new();
        Context.Configuration ??= ProjectFileDeserializer.DassieConfig;
        Context.Configuration ??= new();
        Context.ConfigurationPath ??= ProjectConfigurationFileName;
        Context.Configuration.IgnoredMessages ??= Array.Empty<Ignore>();

        if (Context.Configuration.IgnoreMessages && (error.Severity == Severity.Information || error.Severity == Severity.BuildLogMessage))
            return;

        if (Context.Configuration.IgnoreWarnings && error.Severity == Severity.Warning)
            return;

        if (Context.Configuration.TreatWarningsAsErrors && error.Severity == Severity.Warning)
            error.Severity = Severity.Error;

        if (Context.Configuration.IgnoredMessages.Any(i => i.Code == error.ErrorCode.ToString().Split('_')[0]))
        {
            if (error.Severity == Severity.Error)
            {
                XmlLocationService.Location loc = XmlLocationService.GetElementLocation(Context.ConfigurationPath, "IgnoredMessages");
                if (loc == XmlLocationService.Location.Invalid)
                    loc = new(0, 0, 0);

                EmitWarningMessage(
                    loc.Row,
                    loc.Column,
                    loc.Length,
                    DS0071_IllegalIgnoredMessage,
                    $"The error code {error.ErrorCode.ToString().Split('_')[0]} cannot be ignored.",
                    ProjectConfigurationFileName);
            }

            else
                return;
        }

        if (Context.CompilerSuppressedMessages.Any(e => e == error.ErrorCode))
            return;

        try
        {
            ConsoleColor defaultColor = Console.ForegroundColor;

            // Filter out duplicate messages
            if (messages.Where(e => e.ErrorMessage == error.ErrorMessage && e.CodePosition == error.CodePosition).Any())
                return;

            var outStream = error.Severity switch
            {
                Severity.Error => ErrorOut,
                Severity.Warning => WarnOut,
                _ => InfoOut
            };

            StringBuilder outBuilder = new();

            void SetColorRgb(byte r, byte g, byte b)
            {
                if (ConsoleHelper.AnsiEscapeSequenceSupported && (outStream.Writers.Contains(Console.Out) || outStream.Writers.Contains(Console.Error)))
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
                if (ConsoleHelper.AnsiEscapeSequenceSupported && (outStream.Writers.Contains(Console.Out) || outStream.Writers.Contains(Console.Error)))
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

            if (Context.Configuration.EnableMessageTimestamps)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                outBuilder.Append($"[{DateTime.Now}] ");
            }

            string errCode = error.ErrorCode == ErrorKind.CustomError ? error.CustomErrorCode : error.ErrorCode.ToString().Split('_')[0];
            string codePos = "\b";

            // Legacy colors
            if (!ConsoleHelper.AnsiEscapeSequenceSupported && (outStream.Writers.Contains(Console.Out) || outStream.Writers.Contains(Console.Error)))
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
                outBuilder.Append(error.Severity switch
                {
                    Severity.Error => "error",
                    Severity.Warning => "warning",
                    _ => "message"
                });
                outBuilder.Append(' ');
                outBuilder.Append(errCode);
                outBuilder.Append(": ");
                ResetColor();

                SetColorRgb(255, 255, 255);
                outBuilder.AppendLine(error.ErrorMessage);
                ResetColor();
            }

            outStream.Write(outBuilder.ToString());
            outStream.Flush();
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
                    outStream.Write(outBuilder.ToString());
                    outBuilder.Clear();

                    Console.ForegroundColor = error.Severity switch
                    {
                        Severity.Error => ConsoleColor.DarkRed,
                        Severity.Warning => ConsoleColor.DarkYellow,
                        _ => ConsoleColor.DarkCyan
                    };

                    outBuilder.Append("^");
                    outBuilder.AppendLine(new string('~', Math.Max(error.Length, 0)));

                    ResetColor();
                    outStream.Write(outBuilder.ToString());
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

            messages.Add(error);

            ResetColor();
            outStream.Write(outBuilder.ToString());
        }
        catch (IOException)
        {
            CurrentFile.Errors.Add(error);
            return;
        }

        foreach ((IBuildLogDevice device, var attribs, var elems) in BuildLogDevices)
            device.Log(error, attribs, elems);
    }

    /// <summary>
    /// Writes an error message using <see cref="ErrorOut"/>.
    /// </summary>
    public static void EmitErrorMessage(ErrorInfo error, bool addToErrorList = true)
    {
        EmitGeneric(error, true, addToErrorList);
    }

    /// <summary>
    /// Writes a warning message using <see cref="WarnOut"/>.
    /// </summary>
    public static void EmitWarningMessage(ErrorInfo error, bool treatAsError = false)
    {
        EmitGeneric(error, treatAsError, false);
    }

    /// <summary>
    /// Writes a message using <see cref="InfoOut"/>.
    /// </summary>
    public static void EmitMessage(ErrorInfo error)
    {
        EmitGeneric(error, false, false);
    }

    /// <summary>
    /// Emits a build log message that is not caused by an error in the code.
    /// </summary>
    /// <param name="message">The message to emit.</param>
    public static void EmitBuildLogMessage(string message)
    {
        EmitGeneric(new ErrorInfo()
        {
            CodePosition = (0, 0),
            Length = 0,
            ErrorCode = DS0101_DiagnosticInfo,
            ErrorMessage = message,
            File = "",
            HideCodePosition = true,
            Severity = Severity.BuildLogMessage,
            Tip = "",
            ToolTip = null
        }, false);
    }

    /// <summary>
    /// Writes an error message using <see cref="ErrorOut"/>.
    /// </summary>
    /// <remarks>If <paramref name="file"/> is null, will assume <see cref="FileContext.Path"/>.</remarks>
    public static void EmitErrorMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.DS0001_SyntaxError, string msg = "Syntax error.", string file = null, bool addToErrorList = true, string tip = "", bool hideCodePosition = false)
    {
        hideCodePosition = ln == 0 && col == 0 && length == 0;

        ObservableCollection<Word> words = new()
        {
            new()
            {
                Text = $"{errorType.ToString().Split('_')[0]}: {msg}"
            }
        };

        EmitErrorMessage(new ErrorInfo()
        {
            CodePosition = (ln, col),
            Length = length,
            ErrorCode = errorType,
            ErrorMessage = msg,
            File = file ?? Path.GetFileName(CurrentFile.Path),
            Severity = Severity.Error,
            Tip = tip,
            HideCodePosition = hideCodePosition,
            ToolTip = new()
            {
                IconResourceName = "CodeErrorRule",
                Words = words
            }
        }, addToErrorList);
    }

    /// <summary>
    /// Writes a warning message using <see cref="WarnOut"/>.
    /// </summary>
    public static void EmitWarningMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.DS0001_SyntaxError, string msg = "Syntax error.", string file = null, bool treatAsError = false, string tip = "", bool hideCodePosition = false)
    {
        hideCodePosition = ln == 0 && col == 0 && length == 0;

        ObservableCollection<Word> words = new()
        {
            new()
            {
                Text = $"{errorType.ToString().Split('_')[0]}: {msg}"
            }
        };

        ErrorInfo err = new()
        {
            CodePosition = (ln, col),
            Length = length,
            ErrorCode = errorType,
            ErrorMessage = msg,
            File = file ?? CurrentFile.Path,
            Severity = Severity.Warning,
            Tip = tip,
            HideCodePosition = hideCodePosition,
            ToolTip = new()
            {
                Words = words,
                IconResourceName = Context.Configuration.TreatWarningsAsErrors ? "CodeErrorRule" : "CodeWarningRule"
            }
        };

        EmitWarningMessage(err, treatAsError);
    }

    /// <summary>
    /// Writes a message using <see cref="InfoOut"/>.
    /// </summary>
    public static void EmitMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.DS0001_SyntaxError, string msg = "Syntax error.", string file = null, string tip = "", bool hideCodePosition = false)
    {
        hideCodePosition = ln == 0 && col == 0 && length == 0;

        ObservableCollection<Word> words = new()
        {
            new()
            {
                Text = $"{errorType.ToString().Split('_')[0]}: {msg}"
            }
        };

        EmitMessage(new ErrorInfo()
        {
            CodePosition = (ln, col),
            Length = length,
            ErrorCode = errorType,
            ErrorMessage = msg,
            File = file ?? CurrentFile.Path,
            Severity = Severity.Information,
            Tip = tip,
            HideCodePosition = hideCodePosition,
            ToolTip = new()
            {
                Words = words,
                IconResourceName = "CodeInformation"
            }
        });
    }
}
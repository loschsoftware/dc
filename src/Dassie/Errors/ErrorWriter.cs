using Dassie.Configuration;
using Dassie.Configuration.Analysis;
using Dassie.Meta;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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
    public static TextWriter ErrorOut { get; set; } = Console.Error;
    /// <summary>
    /// The output text writer used for warning messages.
    /// </summary>
    public static TextWriter WarnOut { get; set; } = Console.Out;
    /// <summary>
    /// The output text writer used for information messages.
    /// </summary>
    public static TextWriter InfoOut { get; set; } = Console.Out;

    /// <summary>
    /// A prefix of all error messages, indicating which project the error is from.
    /// </summary>
    public static string MessagePrefix { get; set; } = "";

    /// <summary>
    /// Contains configuration for the error writer.
    /// </summary>
    public static DassieConfig Config { get; set; } = new();

    internal static void EmitGeneric(ErrorInfo error, bool treatAsError = false, bool addToErrorList = true)
    {
        Context ??= new();
        Context.Configuration ??= new();
        Context.ConfigurationPath ??= "dsconfig.xml";
        Context.Configuration.IgnoredMessages ??= Array.Empty<Ignore>();

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
                    "dsconfig.xml");
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

            Console.CursorLeft = 0;

            Console.ForegroundColor = ConsoleColor.DarkGray;

            string prefix = "\r\n";

            if (!string.IsNullOrEmpty(MessagePrefix))
            {
                outStream.Write($"\r\n[{MessagePrefix}] ");
                prefix = "";
            }

            Console.ForegroundColor = error.Severity switch
            {
                Severity.Error => ConsoleColor.Red,
                Severity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.Cyan
            };

            string errCode = error.ErrorCode == ErrorKind.CustomError ? error.CustomErrorCode : error.ErrorCode.ToString().Split('_')[0];
            string codePos = "(~)";

            if (!error.HideCodePosition)
                codePos = $"({error.CodePosition.Item1},{error.CodePosition.Item2})";

            outStream.WriteLine($"{prefix}{Path.GetFileName(error.File)} {codePos}: {error.Severity switch
            {
                Severity.Error => "error",
                Severity.Warning => "warning",
                _ => "message"
            }} {errCode}: {error.ErrorMessage}");

            if (!string.IsNullOrEmpty(error.Tip) && Context.Configuration.EnableTips)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                InfoOut.WriteLine(error.Tip);
            }

            Console.ForegroundColor = defaultColor;

            if (Context.Configuration.AdvancedErrorMessages)
            {
                try
                {
                    Console.WriteLine();

                    using StreamReader sr = new(CurrentFile.Path);
                    string line = "";

                    for (int i = 0; i < error.CodePosition.Item1; i++, line = sr.ReadLine()) ;

                    Console.WriteLine(line);

                    Console.ForegroundColor = error.Severity switch
                    {
                        Severity.Error => ConsoleColor.DarkRed,
                        Severity.Warning => ConsoleColor.DarkYellow,
                        _ => ConsoleColor.DarkCyan
                    };

                    Console.Write(new string(' ', error.CodePosition.Item2));
                    Console.Write("^");
                    Console.WriteLine(new string('~', Math.Max(error.Length, 0)));

                    Console.ForegroundColor = defaultColor;
                }
                catch (Exception)
                {
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
        }
        catch (IOException)
        {
            CurrentFile.Errors.Add(error);
            return;
        }
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

        if (Context.Configuration.IgnoreWarnings)
            return;

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

        if (Context.Configuration.TreatWarningsAsErrors)
        {
            EmitErrorMessage(err);
            return;
        }

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

        if (Context.Configuration.IgnoreMessages)
            return;

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
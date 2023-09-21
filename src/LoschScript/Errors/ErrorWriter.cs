using Losch.LoschScript.Configuration;
using LoschScript.Meta;
using LoschScript.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LoschScript.Errors;

/// <summary>
/// Provides methods for emitting LoschScript error messages.
/// </summary>
public static class ErrorWriter
{
    static ErrorWriter()
    {
        CurrentFile ??= new("");
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
    /// Contains configuration for the error writer.
    /// </summary>
    public static LSConfig Config { get; set; } = new();

    private static void EmitGeneric(ErrorInfo error, bool treatAsError = false, bool addToErrorList = true)
    {
        try
        {
            ConsoleColor defaultColor = Console.ForegroundColor;

            // Filter out duplicate messages
            if (messages.Where(e => e.ErrorMessage == error.ErrorMessage && e.CodePosition == error.CodePosition).Any())
                return;

            Console.ForegroundColor = error.Severity switch
            {
                Severity.Error => ConsoleColor.Red,
                Severity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.Cyan
            };

            Console.CursorLeft = 0;

            (error.Severity switch
            {
                Severity.Error => ErrorOut,
                Severity.Warning => WarnOut,
                _ => InfoOut

            }).WriteLine($"\r\n{error.File} ({error.CodePosition.Item1},{error.CodePosition.Item2}): {error.Severity switch
            {
                Severity.Error => "error",
                Severity.Warning => "warning",
                _ => "information"
            }} {error.ErrorCode.ToString().Split('_')[0]}: {error.ErrorMessage}");

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
    public static void EmitErrorMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.LS0001_SyntaxError, string msg = "Syntax error.", string file = null, bool addToErrorList = true, string tip = "")
    {
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
    public static void EmitWarningMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.LS0001_SyntaxError, string msg = "Syntax error.", string file = null, bool treatAsError = false, string tip = "")
    {
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
    public static void EmitMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.LS0001_SyntaxError, string msg = "Syntax error.", string file = null, string tip = "")
    {
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
            ToolTip = new()
            {
                Words = words,
                IconResourceName = "CodeInformation"
            }
        });
    }
}
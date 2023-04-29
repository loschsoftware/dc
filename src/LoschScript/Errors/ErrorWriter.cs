using Losch.LoschScript.Configuration;
using LoschScript.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoschScript.Errors;

/// <summary>
/// Provides methods for emitting LoschScript error messages.
/// </summary>
public static class ErrorWriter
{
    private static readonly List<ErrorInfo> messages = new();

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

    /// <summary>
    /// Writes an error message using <see cref="ErrorOut"/>.
    /// </summary>
    public static void EmitErrorMessage(ErrorInfo error, bool addToErrorList = true)
    {
        // Filter out duplicate messages
        if (messages.Where(e => e.ErrorMessage == error.ErrorMessage && e.CodePosition == error.CodePosition).Any())
            return;

        Console.ForegroundColor = ConsoleColor.Red;

        Console.CursorLeft = 0;
        ErrorOut.WriteLine($"{error.File} ({error.CodePosition.Item1},{error.CodePosition.Item2}): error {error.ErrorCode.ToString().Split('_')[0]}: {error.ErrorMessage}\r\n");
        CurrentFile.CompilationFailed = true;

        if (addToErrorList)
            CurrentFile.Errors.Add(error);

        Console.ForegroundColor = ConsoleColor.Gray;

        messages.Add(error);
    }

    /// <summary>
    /// Writes a warning message using <see cref="WarnOut"/>.
    /// </summary>
    public static void EmitWarningMessage(ErrorInfo error, bool treatAsError = false)
    {
        // Filter out duplicate messages
        if (messages.Where(e => e.ErrorMessage == error.ErrorMessage && e.CodePosition == error.CodePosition).Any())
            return;

        if (Config.IgnoreWarnings)
            return;

        if (Config.TreatWarningsAsErrors)
            treatAsError = true;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.CursorLeft = 0;
        WarnOut.WriteLine($"{error.File} ({error.CodePosition.Item1},{error.CodePosition.Item2}): warning {error.ErrorCode.ToString().Split('_')[0]}: {error.ErrorMessage}\r\n");
        Console.ForegroundColor = ConsoleColor.Gray;

        if (treatAsError)
        {
            CurrentFile.Errors.Add(error);
            CurrentFile.CompilationFailed = true;
        }

        messages.Add(error);
    }

    /// <summary>
    /// Writes a message using <see cref="InfoOut"/>.
    /// </summary>
    public static void EmitMessage(ErrorInfo error)
    {
        // Filter out duplicate messages
        if (messages.Where(e => e.ErrorMessage == error.ErrorMessage && e.CodePosition == error.CodePosition).Any())
            return;

        if (!Config.IgnoreMessages)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.CursorLeft = 0;
            InfoOut.WriteLine($"{error.File} ({error.CodePosition.Item1},{error.CodePosition.Item2}): information {error.ErrorCode.ToString().Split('_')[0]}: {error.ErrorMessage}\r\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        messages.Add(error);
    }

    /// <summary>
    /// Writes an error message using <see cref="ErrorOut"/>.
    /// </summary>
    /// <remarks>If <paramref name="file"/> is null, will assume <see cref="FileContext.Path"/>.</remarks>
    public static void EmitErrorMessage(int ln = 0, int col = 0, ErrorKind errorType = ErrorKind.LS0001_SyntaxError, string msg = "Syntax error.", string file = null, bool addToErrorList = true)
    {
        EmitErrorMessage(new ErrorInfo()
        {
            CodePosition = (ln, col),
            //CodeEndPosition = (line.lnEnd, column.colEnd),
            ErrorCode = errorType,
            ErrorMessage = msg,
            File = file ?? CurrentFile.Path
        }, addToErrorList);
    }

    /// <summary>
    /// Writes a warning message using <see cref="WarnOut"/>.
    /// </summary>
    public static void EmitWarningMessage(int ln = 0, int col = 0, ErrorKind errorType = ErrorKind.LS0001_SyntaxError, string msg = "Syntax error.", string file = null, bool treatAsError = false)
    {
        EmitWarningMessage(new ErrorInfo()
        {
            CodePosition = (ln, col),
            //CodeEndPosition = (line.lnEnd, column.colEnd),
            ErrorCode = errorType,
            ErrorMessage = msg,
            File = file ?? CurrentFile.Path
        }, treatAsError);
    }

    /// <summary>
    /// Writes a message using <see cref="InfoOut"/>.
    /// </summary>
    public static void EmitMessage(int ln = 0, int col = 0, ErrorKind errorType = ErrorKind.LS0001_SyntaxError, string msg = "Syntax error.", string file = null)
    {
        EmitMessage(new ErrorInfo()
        {
            CodePosition = (ln, col),
            //CodeEndPosition = (line.lnEnd, column.colEnd),
            ErrorCode = errorType,
            ErrorMessage = msg,
            File = file ?? CurrentFile.Path
        });
    }
}
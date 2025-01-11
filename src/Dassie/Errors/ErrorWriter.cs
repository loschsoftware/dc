using Dassie.Configuration;
using Dassie.Configuration.Analysis;
using Dassie.Errors.Devices;
using Dassie.Extensions;
using Dassie.Meta;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    internal static readonly List<ErrorInfo> messages = [];

    /// <summary>
    /// A list of build devices to use.
    /// </summary>
    public static List<IBuildLogDevice> BuildLogDevices { get; set; } = [TextWriterBuildLogDevice.Instance];

    /// <summary>
    /// A prefix of all error messages, indicating which project the error is from.
    /// </summary>
    public static string MessagePrefix { get; set; } = "";

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

        // Filter out duplicate messages
        if (messages.Where(e => e.ErrorMessage == error.ErrorMessage && e.CodePosition == error.CodePosition).Any())
            return;

        foreach (IBuildLogDevice device in BuildLogDevices)
            device.Log(error);

        messages.Add(error);
    }

    /// <summary>
    /// Writes an error message to the designated error outputs.
    /// </summary>
    public static void EmitErrorMessage(ErrorInfo error, bool addToErrorList = true)
    {
        EmitGeneric(error, true, addToErrorList);
    }

    /// <summary>
    /// Writes a warning message to the designated warning outputs.
    /// </summary>
    public static void EmitWarningMessage(ErrorInfo error, bool treatAsError = false)
    {
        EmitGeneric(error, treatAsError, false);
    }

    /// <summary>
    /// Writes a message to the designated information outputs.
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
    /// Writes an error message to the designated error outputs.
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
    /// Writes a warning message to the designated warning outputs.
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
    /// Writes a message to the designated information outputs.
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

    /// <summary>
    /// Writes a string to the designated information outputs.
    /// </summary>
    /// <param name="str">The string to write.</param>
    public static void WriteOutString(string str)
    {
        foreach (IBuildLogDevice device in BuildLogDevices)
            device.WriteString(str);
    }

    /// <summary>
    /// Writes a string followed by a newline sequence to the designated information outputs.
    /// </summary>
    /// <param name="str">The string to write.</param>
    public static void WriteLine(string str)
    {
        WriteOutString($"{str}{Environment.NewLine}");
    }
}
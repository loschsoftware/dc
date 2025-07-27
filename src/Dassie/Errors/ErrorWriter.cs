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

    /// <summary>
    /// A list of all build messages emitted so far.
    /// </summary>
    public static readonly List<ErrorInfo> Messages = [];
    
    private static readonly List<(ErrorInfo Error, int MinVerbosity)> _deferredMessages = [];

    /// <summary>
    /// A list of build devices to use.
    /// </summary>
    public static List<IBuildLogDevice> BuildLogDevices { get; set; } = [TextWriterBuildLogDevice.Instance];

    private static readonly List<IBuildLogDevice> _disabledDevices = [];

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

    private static void BuildLogDeviceSafeCall(IBuildLogDevice device, Action<IBuildLogDevice> func)
    {
        try
        {
            func(device);
        }
        catch (Exception ex)
        {
            _disabledDevices.Add(device);

            EmitWarningMessage(
                0, 0, 0,
                DS0215_ExtensionThrewException,
                $"An unhandled exception of type '{ex.GetType()}' was caused by the build log device '{device.Name}'. This build log device will be disabled for the rest of the compilation.",
                "dc");
            
            try
            {
                if (ProjectFileDeserializer.DassieConfig.PrintExceptionInfo)
                    TextWriterBuildLogDevice.ErrorOut.WriteLine(ex);
            }
            catch { }
        }
    }

    /// <summary>
    /// Emits a generic build log message.
    /// </summary>
    /// <param name="error">The message to emit.</param>
    public static void EmitGeneric(ErrorInfo error)
    {
        if (Disabled)
            return;

        Context ??= new();
        Context.Configuration ??= ProjectFileDeserializer.DassieConfig;
        Context.Configuration ??= new();
        Context.ConfigurationPath ??= ProjectConfigurationFileName;
        Context.Configuration.IgnoredMessages ??= [];

        if (Context.Configuration.Verbosity < 1)
            return;

        error.CodePosition = (error.CodePosition.Line + LineNumberOffset, error.CodePosition.Column);

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
        if (Messages.Any(e => e.ErrorMessage == error.ErrorMessage && e.CodePosition == error.CodePosition))
            return;

        BuildLogSeverity severity = error.Severity switch
        {
            Severity.Warning => BuildLogSeverity.Warning,
            Severity.Error => BuildLogSeverity.Error,
            _ => BuildLogSeverity.Message
        };

        foreach (IBuildLogDevice device in BuildLogDevices)
        {
            if (_disabledDevices.Contains(device))
                continue;

            if (!device.SeverityLevel.HasFlag(severity))
                continue;

            BuildLogDeviceSafeCall(device, d => d.Log(error));
        }

        Messages.Add(error);
    }

    /// <summary>
    /// Emits a build log message that is not caused by an error in the code.
    /// </summary>
    /// <param name="message">The message to emit.</param>
    /// <param name="minimumVerbosity">The minimum verbosity at which the message is printed.</param>
    /// <param name="defer">If <see langword="true"/>, defers the emission of the message until the next call to <see cref="EmitDeferredBuildLogMessages"/>.</param>
    /// <returns><see langword="true"/> if the message was emitted or deferred. <see langword="false"/> if the message was not emitted due to an insufficient verbosity configuration.</returns>
    public static bool EmitBuildLogMessage(string message, int minimumVerbosity = 1, bool defer = false)
    {
        ErrorInfo msg = new()
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
        };

        if (defer)
        {
            _deferredMessages.Add((msg, minimumVerbosity));
            return true;
        }

        if (Context.Configuration.Verbosity < minimumVerbosity)
            return false;

        EmitGeneric(msg);
        return true;
    }

    /// <summary>
    /// Emits all build log messages marked as "deferred" by calls to <see cref="EmitBuildLogMessage(string, int, bool)"/>.
    /// </summary>
    public static void EmitDeferredBuildLogMessages()
    {
        foreach ((ErrorInfo error, int v) in _deferredMessages)
        {
            if (Context.Configuration.Verbosity >= v)
                EmitGeneric(error);
        }

        _deferredMessages.Clear();
    }

    /// <summary>
    /// Writes an error message to the designated error outputs.
    /// </summary>
    /// <remarks>If <paramref name="file"/> is null, will assume <see cref="FileContext.Path"/>.</remarks>
    public static void EmitErrorMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.DS0000_UnknownError, string msg = "Unknown error.", string file = null, string tip = "", string customErrorCode = null)
    {
        bool hideCodePosition = ln == 0 && col == 0 && length == 0;

        ObservableCollection<Word> words =
        [
            new()
            {
                Text = $"{errorType.ToString().Split('_')[0]}: {msg}"
            }
        ];

        EmitGeneric(new ErrorInfo()
        {
            CodePosition = (ln, col),
            Length = length,
            ErrorCode = errorType,
            CustomErrorCode = customErrorCode,
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
        });
    }

    /// <summary>
    /// Writes a warning message to the designated warning outputs.
    /// </summary>
    public static void EmitWarningMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.DS0000_UnknownError, string msg = "Unknown error.", string file = null, string tip = "", string customErrorCode = null)
    {
        bool hideCodePosition = ln == 0 && col == 0 && length == 0;

        ObservableCollection<Word> words =
        [
            new()
            {
                Text = $"{errorType.ToString().Split('_')[0]}: {msg}"
            }
        ];

        ErrorInfo err = new()
        {
            CodePosition = (ln, col),
            Length = length,
            ErrorCode = errorType,
            CustomErrorCode = customErrorCode,
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

        EmitGeneric(err);
    }

    /// <summary>
    /// Writes a message to the designated information outputs.
    /// </summary>
    public static void EmitMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.DS0000_UnknownError, string msg = "Unknown error.", string file = null, string tip = "", string customErrorCode = null)
    {
        bool hideCodePosition = ln == 0 && col == 0 && length == 0;

        ObservableCollection<Word> words =
        [
            new()
            {
                Text = $"{errorType.ToString().Split('_')[0]}: {msg}"
            }
        ];

        EmitGeneric(new ErrorInfo()
        {
            CodePosition = (ln, col),
            Length = length,
            ErrorCode = errorType,
            CustomErrorCode = customErrorCode,
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
        if (Disabled)
            return;

        if (Context.Configuration.Verbosity < 1)
            return;

        foreach (IBuildLogDevice device in BuildLogDevices)
        {
            if (_disabledDevices.Contains(device))
                continue;

            BuildLogDeviceSafeCall(device, d => d.WriteString(str));
        }
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
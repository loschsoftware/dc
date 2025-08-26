using Dassie.Cli.Commands;
using Dassie.Configuration;
using Dassie.Configuration.Analysis;
using Dassie.Errors.Devices;
using Dassie.Extensions;
using Dassie.Text.Tooltips;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Dassie.Errors;

/// <summary>
/// Provides methods for emitting diagnostic messages.
/// </summary>
public static class ErrorWriter
{
    private static readonly Lock _lockObject = new();
    private static readonly ReaderWriterLockSlim _buildLogDevicesLock = new();
    private static readonly Lock _deferredMessagesLock = new();

    static ErrorWriter()
    {
        CurrentFile ??= new("");
        Context ??= new();
        Context.Configuration ??= new();
    }

    /// <summary>
    /// A list of all build messages emitted so far.
    /// </summary>
    public static readonly ConcurrentBag<ErrorInfo> Messages = [];
    
    private static readonly List<(ErrorInfo Error, int MinVerbosity)> _deferredMessages = [];

    internal static bool BuildFailed
    {
        get
        {
            lock (_lockObject)
            {
                return Messages.Any(m => m.Severity == Severity.Error);
            }
        }
    }

    private static List<IBuildLogDevice> _buildLogDevices = [TextWriterBuildLogDevice.Instance];

    /// <summary>
    /// A list of build devices to use.
    /// </summary>
    public static List<IBuildLogDevice> BuildLogDevices 
    { 
        get
        {
            _buildLogDevicesLock.EnterReadLock();
            try
            {
                return [.. _buildLogDevices];
            }
            finally
            {
                _buildLogDevicesLock.ExitReadLock();
            }
        }
        set
        {
            _buildLogDevicesLock.EnterWriteLock();
            try
            {
                _buildLogDevices = value ?? [TextWriterBuildLogDevice.Instance];
            }
            finally
            {
                _buildLogDevicesLock.ExitWriteLock();
            }
        }
    }

    private static readonly ConcurrentBag<IBuildLogDevice> _disabledDevices = [];

    private static string _messagePrefix = "";
    /// <summary>
    /// A prefix of all error messages, indicating which project the error is from.
    /// </summary>
    public static string MessagePrefix 
    { 
        get
        {
            lock (_lockObject)
            {
                return _messagePrefix;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _messagePrefix = value ?? "";
            }
        }
    }

    private static bool _disabled = false;
    /// <summary>
    /// Used to completely disable the error writer.
    /// </summary>
    public static bool Disabled 
    { 
        get
        {
            lock (_lockObject)
            {
                return _disabled;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _disabled = value;
            }
        }
    }

    private static int _lineNumberOffset = 0;
    /// <summary>
    /// A value added to the line number of every error message.
    /// </summary>
    public static int LineNumberOffset 
    { 
        get
        {
            lock (_lockObject)
            {
                return _lineNumberOffset;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _lineNumberOffset = value;
            }
        }
    }

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
                DS0216_ExtensionThrewException,
                $"An unhandled exception of type '{ex.GetType()}' was caused by the build log device '{device.Name}'. This build log device will be disabled for the rest of the compilation.",
                CompilerExecutableName);
            
            try
            {
                if (ProjectFileDeserializer.DassieConfig.PrintExceptionInfo)
                    TextWriterBuildLogDevice.ErrorOut.WriteLine(ex);
            }
            catch { }
        }
    }

    /// <summary>
    /// Emits a build message.
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
                    DS0072_IllegalIgnoredMessage,
                    $"The error code {error.ErrorCode.ToString().Split('_')[0]} cannot be ignored.",
                    ProjectConfigurationFileName);
            }

            else
                return;
        }

        if (Context.CompilerSuppressedMessages.Any(e => e == error.ErrorCode))
            return;

        // Filter out duplicate messages
        lock (_lockObject)
        {
            if (Messages.Any(e => e.ErrorMessage == error.ErrorMessage && e.CodePosition == error.CodePosition))
                return;
        }

        BuildLogSeverity severity = error.Severity switch
        {
            Severity.Warning => BuildLogSeverity.Warning,
            Severity.Error => BuildLogSeverity.Error,
            _ => BuildLogSeverity.Message
        };

        List<IBuildLogDevice> devices = BuildLogDevices; // Get a thread-safe copy
        foreach (IBuildLogDevice device in devices)
        {
            if (_disabledDevices.Contains(device))
                continue;

            if (!device.SeverityLevel.HasFlag(severity))
                continue;

            BuildLogDeviceSafeCall(device, d => d.Log(error));
        }

        Messages.Add(error);

        if (Context.Configuration.MaxErrors > 0 && Messages.Count(m => m.Severity == Severity.Error) >= Context.Configuration.MaxErrors)
            CompileCommand.Abort();
    }

    /// <summary>
    /// Emits a build log message that is not caused by an error in the code.
    /// </summary>
    /// <param name="message">The message to emit.</param>
    /// <param name="minimumVerbosity">The minimum verbosity at which the message is printed.</param>
    /// <param name="defer">If <see langword="true"/>, defers the emission of the message until the next call to <see cref="EmitDeferredBuildLogMessages"/>.</param>
    /// <returns><see langword="true"/> if the message was emitted or deferred. <see langword="false"/> if the message was not emitted due to an insufficient verbosity configuration.</returns>
    public static bool EmitBuildLogMessage(string message, int minimumVerbosity = 2, bool defer = false)
    {
        if (!defer && Context.Configuration.Verbosity < minimumVerbosity)
            return false;

        ErrorInfo msg = new()
        {
            CodePosition = (0, 0),
            Length = 0,
            ErrorCode = DS0102_DiagnosticInfo,
            ErrorMessage = message,
            File = "",
            HideCodePosition = true,
            Severity = Severity.BuildLogMessage,
            Tip = "",
            ToolTip = null
        };

        if (defer)
        {
            lock (_deferredMessagesLock)
            {
                _deferredMessages.Add((msg, minimumVerbosity));
            }
            return true;
        }

        EmitGeneric(msg);
        return true;
    }

    /// <summary>
    /// Emits all build log messages marked as "deferred" by calls to <see cref="EmitBuildLogMessage(string, int, bool)"/>.
    /// </summary>
    public static void EmitDeferredBuildLogMessages()
    {
        lock (_deferredMessagesLock)
        {
            foreach ((ErrorInfo error, int v) in _deferredMessages)
            {
                if (Context.Configuration.Verbosity >= v)
                    EmitGeneric(error);
            }

            _deferredMessages.Clear();
        }
    }

    /// <summary>
    /// Writes an error message to the designated error outputs.
    /// </summary>
    /// <param name="ln">The line of the error.</param>
    /// <param name="col">The column of the error.</param>
    /// <param name="length">The length the error.</param>
    /// <param name="errorType">The error code of the message.</param>
    /// <param name="msg">The error message.</param>
    /// <param name="file">The file name to use in the error message.</param>
    /// <param name="tip">An optional tip displayed in the message.</param>
    /// <param name="customErrorCode">A custom error code. Can only be used if <paramref name="errorType"/> is set to <see cref="CustomError"/>.</param>
    public static void EmitErrorMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = DS0001_UnknownError, string msg = "Unknown error.", string file = null, string tip = "", string customErrorCode = null)
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
    /// <param name="ln">The line of the warning.</param>
    /// <param name="col">The column of the warning.</param>
    /// <param name="length">The length the warning.</param>
    /// <param name="errorType">The error code of the message.</param>
    /// <param name="msg">The warning message.</param>
    /// <param name="file">The file name to use in the warning message.</param>
    /// <param name="tip">An optional tip displayed in the message.</param>
    /// <param name="customErrorCode">A custom error code. Can only be used if <paramref name="errorType"/> is set to <see cref="CustomError"/>.</param>
    public static void EmitWarningMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = ErrorKind.DS0001_UnknownError, string msg = "Unknown error.", string file = null, string tip = "", string customErrorCode = null)
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
    /// <param name="ln">The line of the message.</param>
    /// <param name="col">The column of the message.</param>
    /// <param name="length">The length the message.</param>
    /// <param name="errorType">The error code of the message.</param>
    /// <param name="msg">The message.</param>
    /// <param name="file">The file name to use in the message.</param>
    /// <param name="tip">An optional tip displayed in the message.</param>
    /// <param name="customErrorCode">A custom error code. Can only be used if <paramref name="errorType"/> is set to <see cref="CustomError"/>.</param>
    public static void EmitMessage(int ln = 0, int col = 0, int length = 0, ErrorKind errorType = DS0001_UnknownError, string msg = "Unknown error.", string file = null, string tip = "", string customErrorCode = null)
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

        List<IBuildLogDevice> devices = BuildLogDevices; // Get a thread-safe copy
        foreach (IBuildLogDevice device in devices)
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
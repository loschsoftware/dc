using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Dassie.Messages;

internal sealed class DiagnosticManager
{
    public DiagnosticManager(TextWriter logOut)
    {
        LogOut = logOut ?? TextWriter.Null;
    }

    public TextWriter LogOut { get; }
    public ConcurrentQueue<MessageInfo> Messages { get; } = [];
    public bool HasErrors => Messages.Any(message => message.Severity == Severity.Error);

    public void Write(string text) => LogOut.Write(text);
    public void WriteLine(string text = "") => LogOut.WriteLine(text);

    public void Emit(MessageInfo message)
    {
        if (message is null)
            return;

        Messages.Enqueue(message);

        if (message.Severity == Severity.BuildLogMessage)
            WriteLine(message.Text);
        else
            Write(message.ToString());
    }

    public void EmitBuildLogMessage(string message, int minimumVerbosity = 2)
    {
        Emit(new MessageInfo
        {
            Location = (0, 0),
            Code = DS0102_DiagnosticInfo,
            Text = message,
            File = "",
            Severity = Severity.BuildLogMessage,
            // TODO: Store minimum verbosity
            //MinimumVerbosity = minimumVerbosity
        });
    }

    public void EmitErrorMessage(int line, int column, int length, MessageCode errorType, string message, string file = null, string tip = null, string customErrorCode = null) =>
        Emit(CreateMessage(line, column, length, errorType, message, file, tip, customErrorCode, Severity.Error));

    public void EmitWarningMessage(int line, int column, int length, MessageCode errorType, string message, string file = null, string tip = null, string customErrorCode = null) =>
        Emit(CreateMessage(line, column, length, errorType, message, file, tip, customErrorCode, Severity.Warning));

    public void EmitMessage(int line, int column, int length, MessageCode errorType, string message, string file = null, string tip = null, string customErrorCode = null) =>
        Emit(CreateMessage(line, column, length, errorType, message, file, tip, customErrorCode, Severity.Information));

    public void EmitErrorMessageFormatted(int line, int column, int length, MessageCode errorType, string resourceId, object[] args, string file = null, string tipResourceId = null, object[] tipArgs = null, string customErrorCode = null) =>
        EmitFormatted(line, column, length, errorType, resourceId, args, file, tipResourceId, tipArgs, customErrorCode, Severity.Error);

    public void EmitWarningMessageFormatted(int line, int column, int length, MessageCode errorType, string resourceId, object[] args, string file = null, string tipResourceId = null, object[] tipArgs = null, string customErrorCode = null) =>
        EmitFormatted(line, column, length, errorType, resourceId, args, file, tipResourceId, tipArgs, customErrorCode, Severity.Warning);

    public void EmitMessageFormatted(int line, int column, int length, MessageCode errorType, string resourceId, object[] args, string file = null, string tipResourceId = null, object[] tipArgs = null, string customErrorCode = null) =>
        EmitFormatted(line, column, length, errorType, resourceId, args, file, tipResourceId, tipArgs, customErrorCode, Severity.Information);

    public void EmitBuildLogMessageFormatted(string resourceId, object[] args, int minimumVerbosity = 2)
    {
        Emit(new MessageInfo
        {
            Location = (0, 0),
            Code = DS0102_DiagnosticInfo,
            TextTemplate = (resourceId, args),
            File = "",
            Severity = Severity.BuildLogMessage,
            // TODO: Store minimum verbosity
            //MinimumVerbosity = minimumVerbosity
        });
    }

    private static MessageInfo CreateMessage(int line, int column, int length, MessageCode errorType, string message, string file, string tip, string customErrorCode, Severity severity)
    {
        return new()
        {
            Location = (line, column),
            Length = length,
            Code = errorType,
            CustomCode = customErrorCode,
            Text = message,
            File = file ?? "",
            Severity = severity,
            Tip = tip
        };
    }

    private void EmitFormatted(int line, int column, int length, MessageCode errorType, string resourceId, object[] args, string file, string tipResourceId, object[] tipArgs, string customErrorCode, Severity severity)
    {
        Emit(new MessageInfo
        {
            Location = (line, column),
            Length = length,
            Code = errorType,
            CustomCode = customErrorCode,
            TextTemplate = (resourceId, args),
            File = file ?? "",
            Severity = severity,
            TipTemplate = (tipResourceId, tipArgs)
        });
    }
}
namespace Dassie.Extensions;

/// <summary>
/// The error severities at which a <see cref="IBuildLogWriter"/> or <see cref="IBuildLogDevice"/> is active.
/// </summary>
public enum BuildLogSeverity
{
    /// <summary>
    /// Active on information messages.
    /// </summary>
    Message = 2,
    /// <summary>
    /// Active on warnings.
    /// </summary>
    Warning = 4,
    /// <summary>
    /// Active on errors.
    /// </summary>
    Error = 8,
    /// <summary>
    /// Active on all kinds of compiler messages.
    /// </summary>
    All = Message | Warning | Error,
    /// <summary>
    /// Active on warnings and errors.
    /// </summary>
    Important = Warning | Error
}
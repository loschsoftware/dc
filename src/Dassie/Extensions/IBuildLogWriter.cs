using System.IO;

namespace Dassie.Extensions;

/// <summary>
/// Defines a mechanism to write build logs using <see cref="System.IO.TextWriter"/>.
/// </summary>
public interface IBuildLogWriter
{
    /// <summary>
    /// The error severities at which the build log writer is active.
    /// </summary>
    public enum Severity
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

    /// <summary>
    /// The name of the log writer.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The error severities at which the log writer is active.
    /// </summary>
    public Severity Severities => IBuildLogWriter.Severity.All;

    /// <summary>
    /// All text writers making up the build log writer.
    /// </summary>
    public TextWriter[] Writers { get; }
}
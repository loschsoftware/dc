using System.IO;

namespace Dassie.Extensions;

/// <summary>
/// Defines a mechanism to write build logs using <see cref="System.IO.TextWriter"/>.
/// </summary>
public interface IBuildLogWriter
{
    /// <summary>
    /// The name of the log writer.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The error severities at which the log writer is active.
    /// </summary>
    public BuildLogSeverity Severity { get; }

    /// <summary>
    /// All text writers making up the build log writer.
    /// </summary>
    public TextWriter[] Writers { get; }
}
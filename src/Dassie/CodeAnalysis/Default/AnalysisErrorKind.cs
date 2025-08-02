namespace Dassie.CodeAnalysis.Default;

/// <summary>
/// Specifies error codes emitted by default Dassie code analyzers.
/// </summary>
public enum AnalysisErrorKind
{
    /// <summary>
    /// A default error that is not further specified.
    /// </summary>
    DS2000_AnalysisDefaultError,
    /// <summary>
    /// Emitted when a naming convention is violated.
    /// </summary>
    DS2001_NamingConvention,
    /// <summary>
    /// Emitted when the application entry point is not called 'Main'.
    /// </summary>
    DS2002_EntryPointWrongName,
    /// <summary>
    /// Emitted when a globally accessible type is not contained within a namespace.
    /// </summary>
    DS2003_TypeOutsideNamespace
}
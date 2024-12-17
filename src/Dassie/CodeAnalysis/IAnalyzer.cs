namespace Dassie.CodeAnalysis;

/// <summary>
/// The base interface for all Dassie code analyzers.
/// </summary>
public interface IAnalyzer<out TKind>
{
    /// <summary>
    /// Defines the name of the analyzer.
    /// </summary>
    public string Name { get; }
}
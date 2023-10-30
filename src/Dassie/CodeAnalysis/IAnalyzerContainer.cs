using System.Collections.Generic;

namespace Dassie.CodeAnalysis;

/// <summary>
/// Represents a container of multiple code analyzers.
/// </summary>
public interface IAnalyzerContainer
{
    /// <summary>
    /// The analyzers of the container.
    /// </summary>
    public List<IAnalyzer<object>> Analyzers { get; }
}
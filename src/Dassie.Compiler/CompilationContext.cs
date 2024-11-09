using Dassie.Configuration;
using System.Collections.Generic;

namespace Dassie.Compiler;

/// <summary>
/// Represents the context of a Dassie compilation, including source files to compile and configuration.
/// </summary>
public class CompilationContext
{
    /// <summary>
    /// The source documents to compile.
    /// </summary>
    public IEnumerable<SourceDocument> Documents { get; }

    /// <summary>
    /// Configuration for the current compilation.
    /// </summary>
    public DassieConfig Configuration { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="CompilationContext"/> type.
    /// </summary>
    /// <param name="documents">The source documents to include in the compilation.</param>
    /// <param name="config">Configuration for the current compilation.</param>
    public CompilationContext(IEnumerable<SourceDocument> documents, DassieConfig config)
    {
        Documents = documents;
        Configuration = config;
    }
}
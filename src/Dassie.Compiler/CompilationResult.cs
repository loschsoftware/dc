using Dassie.Errors;
using System.Collections.Generic;

namespace Dassie.Compiler;

/// <summary>
/// Represents the result of a Dassie compilation.
/// </summary>
/// <param name="Success">Wheter or not the compilation was successful.</param>
/// <param name="Errors">A list of compiler errors and messages emitted during the compilation.</param>
public record CompilationResult(bool Success, List<ErrorInfo> Errors);
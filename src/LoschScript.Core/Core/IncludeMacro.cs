using System;
using System.IO;

namespace LoschScript.Core;

/// <summary>
/// Provides a macro to include the contents of a code file inside another.
/// </summary>
public class IncludeMacro : IMacro
{
    /// <summary>
    /// The name used to call the macro. Always returns <c>include</c>.
    /// </summary>
    public string MacroName => "include";
    
    /// <summary>
    /// Executes the macro with the specified argument.
    /// </summary>
    /// <param name="input">The absolute or relative path to the code file to include.</param>
    /// <returns>Returns the contents of the specified file.</returns>
    /// <exception cref="FileNotFoundException">Exception thrown when the argument is not an existing file.</exception>
    public string Process(string input)
    {
        if (!File.Exists(input))
            throw new FileNotFoundException("The specified source file does not exist.", input);

        return File.ReadAllText(input);
    }

    /// <summary>
    /// Throws an exception, because no code file is specified.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    public string Process()
    {
        throw new InvalidOperationException("No file to be included specified.");
    }
}
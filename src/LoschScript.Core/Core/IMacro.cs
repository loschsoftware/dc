namespace LoschScript.Core;

/// <summary>
/// Macros are evaluated during the preprocessing stage and enable code injection or conditional compilation.
/// </summary>
public interface IMacro
{
    /// <summary>
    /// Evaluates a macro call with one operand.
    /// </summary>
    /// <param name="input">The operand of the macro call.</param>
    /// <returns>Returns the code being injected into the file that uses the macro.</returns>
    public string Process(string input);

    /// <summary>
    /// Evaluates a macro call with no operands.
    /// </summary>
    /// <returns>Returns the code being injected into the file that uses the macro.</returns>
    public string Process();

    /// <summary>
    /// Specifies the name of the macro that is used to call it.
    /// </summary>
    public string MacroName { get; }
}
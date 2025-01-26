namespace Dassie.Core;

/// <summary>
/// Provides a mechanism for processing string literals, for example to allow them to have different encodings.
/// </summary>
public interface IStringProcessor<out TReturn>
{
    /// <summary>
    /// Processes the specified input string.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>The processed object.</returns>
    public static abstract TReturn Process(string input);
}
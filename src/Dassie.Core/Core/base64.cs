using System;
using System.Text;

#pragma warning disable IDE1006

namespace Dassie.Core;

/// <summary>
/// Provides a string processor for encoding string literals as Base64.
/// </summary>
public class base64 : IStringProcessor<string>
{
    /// <inheritdoc/>
    [Pure]
    public static string Process(string input)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
}

/// <summary>
/// Provides a string processor for decoding Base64 string literals.
/// </summary>
public class from_base64 : IStringProcessor<string>
{
    /// <inheritdoc/>
    [Pure]
    public static string Process(string input)
        => Encoding.UTF8.GetString(Convert.FromBase64String(input));
}
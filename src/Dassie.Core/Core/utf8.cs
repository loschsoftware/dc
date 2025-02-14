using System;
using System.Text;

#pragma warning disable IDE1006

namespace Dassie.Core;

/// <summary>
/// Provides a string processor to treat string literals as UTF-8 character arrays.
/// </summary>
public class utf8 : IStringProcessor<byte[]>
{
    /// <inheritdoc/>
    [Pure]
    public static byte[] Process(string input)
        => Encoding.UTF8.GetBytes(input);
}
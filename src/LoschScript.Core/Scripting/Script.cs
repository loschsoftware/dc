using System;
using System.IO;

namespace LoschScript.Core.Scripting;

/// <summary>
/// Represents a script, which is a LoschScript program embedded inside a host application.
/// </summary>
public class Script
{
    private Script() { }

    internal string _text;

    /// <summary>
    /// Creates a new instance of the <see cref="Script"/> type based on a script file.
    /// </summary>
    /// <param name="path">The path to the script file.</param>
    /// <returns>Returns a script object representing the specified file.</returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static Script FromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("The specified script file does not exist.", path);

        return new()
        {
            _text = File.ReadAllText(path)
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Script"/> type based on the source code of a script.
    /// </summary>
    /// <param name="source">The source code of the script.</param>
    /// <returns>Returns a script object representing the specified source code.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the specified source code is an empty value.</exception>
    public static Script FromSource(string source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        return new()
        {
            _text = source
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Script"/> type based on a stream.
    /// </summary>
    /// <param name="stream">The stream representing the script.</param>
    /// <returns>Returns a script object based on the specified stream.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the specified stream is an empty value.</exception>
    public static Script FromStream(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using StreamReader sr = new(stream);

        return new()
        {
            _text = sr.ReadToEnd()
        };
    }
}
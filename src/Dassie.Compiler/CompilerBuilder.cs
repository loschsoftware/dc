using Dassie.Configuration;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Dassie.Compiler;

/// <summary>
/// Provides a fluent API for building <see cref="DassieCompiler"/> objects.
/// </summary>
public class CompilerBuilder
{
    private CompilerBuilder()
    {
        _guid = Guid.NewGuid();
        _tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", _guid.ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    private readonly Guid _guid;
    private readonly string _tempDir;

    private int _fileCount = 0;
    private bool _isConfigAdded = false;

    /// <summary>
    /// Initializes a new <see cref="CompilerBuilder"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="CompilerBuilder"/>.</returns>
    public static CompilerBuilder CreateBuilder() => new();

    /// <summary>
    /// Adds a string of Dassie code to the compilation context.
    /// </summary>
    /// <param name="src">The source code to add.</param>
    /// <returns>The current instance of <see cref="CompilerBuilder"/>.</returns>
    public CompilerBuilder AddSourceFromString(string src)
    {
        using StreamWriter sw = new(Path.Combine(_tempDir, $"{_fileCount++}.ds"));
        sw.Write(src);

        return this;
    }

    /// <summary>
    /// Adds an existing Dassie source file to the compilation context.
    /// </summary>
    /// <param name="file">The path to the file to add.</param>
    /// <returns>The current instance of <see cref="CompilerBuilder"/>.</returns>
    public CompilerBuilder AddSourceFromFile(string file)
    {
        File.Copy(file, Path.Combine(_tempDir, $"{_fileCount++}_{Path.GetFileName(file)}"));
        return this;
    }

    /// <summary>
    /// Specifies the Dassie compiler options to use.
    /// </summary>
    /// <param name="config">The configuration to use.</param>
    /// <returns>The current instance of <see cref="CompilerBuilder"/>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public CompilerBuilder WithConfiguration(DassieConfig config)
    {
        if (_isConfigAdded)
            throw new InvalidOperationException("Configuration was already added.");

        _isConfigAdded = true;

        using StreamWriter sw = new(Path.Combine(_tempDir, "dsconfig.xml"));
        XmlSerializer xmls = new(typeof(DassieConfig));
        xmls.Serialize(sw, config);

        return this;
    }

    /// <summary>
    /// Creates an instance of <see cref="DassieCompiler"/> based on the specified files and settings.
    /// </summary>
    /// <returns>A new instance of <see cref="DassieCompiler"/>.</returns>
    public DassieCompiler CreateCompiler() => new(_tempDir);
}
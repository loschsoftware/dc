using Dassie.Configuration;
using Dassie.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Xml.Serialization;

namespace Dassie.Compiler;

/// <summary>
/// Provides a fluent API for creating objects of type <see cref="CompilationContext"/>.
/// </summary>
public class CompilationContextBuilder
{
    private CompilationContextBuilder() { }

    /// <summary>
    /// Initializes a new compilation context builder.
    /// </summary>
    /// <returns>A new instance of <see cref="CompilationContextBuilder"/>.</returns>
    public static CompilationContextBuilder CreateBuilder() => new();

    private readonly List<SourceDocument> _documents = [];
    private DassieConfig _config = new();

    /// <summary>
    /// Sets the compiler configuration for the compilation context.
    /// </summary>
    /// <param name="config">The configuration to use.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"/>
    public CompilationContextBuilder WithConfiguration(DassieConfig config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        _config = config;
        return this;
    }

    /// <summary>
    /// Adds the compiler configuration for the compilation context from an XML configuration file.
    /// </summary>
    /// <param name="configFile">The path to the configuration file.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="IOException"></exception>
    public CompilationContextBuilder WithConfiguration(string configFile)
    {
        if (!File.Exists(configFile))
            throw new FileNotFoundException();

        using StreamReader sr = new(configFile);
        XmlSerializer xmls = new(typeof(DassieConfig));
        _config = (DassieConfig)xmls.Deserialize(sr);
        return this;
    }

    /// <summary>
    /// Adds Dassie source code from a file to the compilation context.
    /// </summary>
    /// <param name="path">The source file to add.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="SecurityException"></exception>
    public CompilationContextBuilder AddSourceFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException();

        _documents.Add(new(File.ReadAllText(path), path));
        return this;
    }

    /// <summary>
    /// Adds multiple Dassie source files to the compilation context.
    /// </summary>
    /// <param name="files">The source files to add.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="SecurityException"></exception>
    public CompilationContextBuilder AddSourceFiles(IEnumerable<string> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        foreach (string path in files)
            AddSourceFile(path);

        return this;
    }

    /// <summary>
    /// Adds a directory containing Dassie source files to the compilation context.
    /// </summary>
    /// <param name="path">The path to the directory to add.</param>
    /// <param name="recursive">Wheter to include source files from subdirectories.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public CompilationContextBuilder AddSourceDirectory(string path, bool recursive)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException();

        foreach (string file in Directory.GetFiles(path, "*.ds", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            AddSourceFile(file);

        return this;
    }

    /// <summary>
    /// Adds a <see cref="SourceDocument"/> to the compilation context.
    /// </summary>
    /// <param name="document">The document to add.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"/>.
    public CompilationContextBuilder AddSourceDocument(SourceDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _documents.Add(document);
        return this;
    }

    /// <summary>
    /// Adds multiple <see cref="SourceDocument"/>s to the compilation context.
    /// </summary>
    /// <param name="documents">The documents to add.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"/>.
    public CompilationContextBuilder AddSourceDocuments(IEnumerable<SourceDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);
        foreach (SourceDocument document in documents)
            AddSourceDocument(document);

        return this;
    }

    /// <summary>
    /// Adds a string of Dassie source code to the compilation context.
    /// </summary>
    /// <param name="text">The source code to add.</param>
    /// <param name="symbolicName">A symbolic name for the document.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"/>.
    public CompilationContextBuilder AddSourceFromText(string text, string symbolicName)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(symbolicName);
        AddSourceDocument(new(text, symbolicName));
        return this;
    }

    /// <summary>
    /// Adds a string of Dassie source code to the compilation context.
    /// </summary>
    /// <param name="text">The source code to add.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"/>.
    public CompilationContextBuilder AddSourceFromText(string text) => AddSourceFromText(text, "");

    /// <summary>
    /// Allows redirecting compiler messages to any <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="messageResolver">A resolver function that returns the <see cref="TextWriter"/> to write compiler messages to based on the severity of an error.</param>
    /// <returns>The current instance of <see cref="CompilationContextBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"/>.
    public CompilationContextBuilder RedirectCompilerMessages(Func<Severity, TextWriter> messageResolver)
    {
        ArgumentNullException.ThrowIfNull(messageResolver);
        ErrorWriter.InfoOut = new([messageResolver(Severity.Information)]);
        ErrorWriter.WarnOut = new([messageResolver(Severity.Warning)]);
        ErrorWriter.ErrorOut = new([messageResolver(Severity.Error)]);
        return this;
    }

    /// <summary>
    /// Builds a <see cref="CompilationContext"/> object with the specified data.
    /// </summary>
    /// <returns>A new instance of <see cref="CompilationContext"/>.</returns>
    public CompilationContext Build() => new(_documents, _config);
}
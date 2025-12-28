using Antlr4.Runtime.Tree;
using Dassie.CodeAnalysis;
using System.Collections.Generic;
using System.Xml;

namespace Dassie.Extensions;

/// <summary>
/// Provides an abstract implementation of the <see cref="IPackage"/> interface.
/// </summary>
public abstract class Extension : IPackage
{
    /// <inheritdoc/>
    public virtual PackageMetadata Metadata => new()
    {
        Name = GetType().Name,
        Version = new(1, 0, 0, 0)
    };

    /// <inheritdoc/>
    public virtual bool Hidden() => false;

    /// <inheritdoc/>
    public virtual ExtensionModes Modes() => ExtensionModes.Global | ExtensionModes.Transient;

    /// <inheritdoc/>
    public virtual int InitializeGlobal(IEnvironmentInfo environment) => 0;

    /// <inheritdoc/>
    public virtual int InitializeTransient(IEnvironmentInfo environment, List<XmlAttribute> attributes, List<XmlElement> elements) => 0;

    /// <inheritdoc/>
    public virtual void Unload() { }

    /// <inheritdoc/>
    public virtual GlobalConfigProperty[] GlobalProperties() => [];

    /// <inheritdoc/>
    public virtual ICompilerCommand[] Commands() => [];

    /// <inheritdoc/>
    public virtual IProjectTemplate[] ProjectTemplates() => [];

    /// <inheritdoc/>
    public virtual IConfigurationProvider[] ConfigurationProviders() => [];

    /// <inheritdoc/>
    public virtual IAnalyzer<IParseTree>[] CodeAnalyzers() => [];

    /// <inheritdoc/>
    public virtual IBuildLogWriter[] BuildLogWriters() => [];

    /// <inheritdoc/>
    public virtual IBuildLogDevice[] BuildLogDevices() => [];

    /// <inheritdoc/>
    public virtual ICompilerDirective[] CompilerDirectives() => [];

    /// <inheritdoc/>
    public virtual IDocumentSource[] DocumentSources() => [];

    /// <inheritdoc/>
    public virtual IDeploymentTarget[] DeploymentTargets() => [];

    /// <inheritdoc/>
    public virtual ISubsystem[] Subsystems() => [];

    /// <inheritdoc/>
    public virtual IBuildAction[] BuildActions() => [];

    /// <inheritdoc/>
    public virtual IMacro[] Macros() => [];
}
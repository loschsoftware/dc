using Antlr4.Runtime.Tree;
using Dassie.CodeAnalysis;
using System.Collections.Generic;
using System.Xml;

namespace Dassie.Extensions;

/// <summary>
/// Defines a Dassie compiler extension.
/// </summary>
public interface IPackage
{
    /// <summary>
    /// The metadata of the extension.
    /// </summary>
    public PackageMetadata Metadata { get; }

    /// <summary>
    /// If <see langword="true"/>, the extension is hidden and does not appear in extension lists.
    /// </summary>
    /// <returns>Wheter or not the package is hidden.</returns>
    public virtual bool Hidden() => false;

    /// <summary>
    /// Specifies the modes the extension can be loaded in.
    /// </summary>
    public virtual ExtensionModes Modes() => ExtensionModes.Global | ExtensionModes.Transient;

    /// <summary>
    /// The method called when the extension is initialized in global mode. This method is only called if the extension is configured to allow initialization in global mode.
    /// </summary>
    /// <param name="environment">The environment the extension is loaded in.</param>
    /// <returns>A status code representing the result of the initialization. If nonzero, the compiler will emit an error.</returns>
    public virtual int InitializeGlobal(IEnvironmentInfo environment) => 0;

    /// <summary>
    /// The method called when the extension is initialized in transient mode. This method is only called if the extension is configured to allow initialization in transient mode.
    /// </summary>
    /// <param name="environment">The environment the extension is loaded in.</param>
    /// <param name="attributes">The XML attributes the extension is initialized with.</param>
    /// <param name="elements">The XML elements the extension is initialized with.</param>
    /// <returns>A status code representing the result of the initialization. If nonzero, the compiler will emit an error.</returns>
    public virtual int InitializeTransient(IEnvironmentInfo environment, List<XmlAttribute> attributes, List<XmlElement> elements) => 0;

    /// <summary>
    /// The method that is called when the extension is unloaded.
    /// </summary>
    public virtual void Unload() { }

    /// <summary>
    /// An array of global configuration properties.
    /// </summary>
    public virtual GlobalConfigProperty[] GlobalProperties() => [];

    /// <summary>
    /// An array of compiler commands added by this extension.
    /// </summary>
    public virtual ICompilerCommand[] Commands() => [];

    /// <summary>
    /// An array of project templates added by this extension.
    /// </summary>
    public virtual IProjectTemplate[] ProjectTemplates() => [];

    /// <summary>
    /// An array of configuration providers added by this extension.
    /// </summary>
    public virtual IConfigurationProvider[] ConfigurationProviders() => [];

    /// <summary>
    /// An array of code analyzers added by this extensions.
    /// </summary>
    public virtual IAnalyzer<IParseTree>[] CodeAnalyzers() => [];

    /// <summary>
    /// An array of build log writers.
    /// </summary>
    /// <returns></returns>
    public virtual IBuildLogWriter[] BuildLogWriters() => [];

    /// <summary>
    /// An array of build log devices.
    /// </summary>
    public virtual IBuildLogDevice[] BuildLogDevices() => [];

    /// <summary>
    /// An array of compiler directives.
    /// </summary>
    public virtual ICompilerDirective[] CompilerDirectives() => [];

    /// <summary>
    /// An array of document sources.
    /// </summary>
    public virtual IDocumentSource[] DocumentSources() => [];

    /// <summary>
    /// An array of deployment targets.
    /// </summary>
    public virtual IDeploymentTarget[] DeploymentTargets() => [];

    /// <summary>
    /// An array of subsystems.
    /// </summary>
    public virtual ISubsystem[] Subsystems() => [];
}

using Dassie.Configuration;
using System.Collections.Generic;
using System.Xml;

namespace Dassie.Deployment;

/// <summary>
/// Represents the context passed to deployment targets.
/// </summary>
/// <param name="SourceDirectory">The source directory of the deployment.</param>
/// <param name="ProjectGroup">The configuration of the project group.</param>
/// <param name="ExecutableProject">The configuration of the executable project.</param>
/// <param name="Attributes">The XML attributes passed to the deployment target.</param>
/// <param name="Elements">The XML elements passed to the deployment target.</param>
public record DeploymentContext(
    string SourceDirectory,
    DassieConfig ProjectGroup,
    DassieConfig ExecutableProject,
    List<XmlAttribute> Attributes,
    List<XmlNode> Elements
    );
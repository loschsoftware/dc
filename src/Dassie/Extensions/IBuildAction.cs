using System;
using System.Collections.Generic;
using System.Xml;

namespace Dassie.Extensions;

/// <summary>
/// The context in which an action is executed.
/// </summary>
/// <param name="XmlElements">The XML elements passed to the action.</param>
/// <param name="XmlAttributes">The XML attributes passed to the action.</param>
/// <param name="Text">The string argument passed to the action.</param>
/// <param name="Mode">The mode the action is being invoked in.</param>
/// <param name="BuildEventName">The name of the build event executing the action, if the action is executed as part of a build event.</param>
public record ActionContext(List<XmlNode> XmlElements, List<XmlAttribute> XmlAttributes, string Text, ActionModes Mode, string BuildEventName);

/// <summary>
/// Specifies the modes in which an action can be executed.
/// </summary>
[Flags]
public enum ActionModes
{
    /// <summary>
    /// Indicates that no options are set or that the default value should be used.
    /// </summary>
    None = 0,
    /// <summary>
    /// Indicates that the action can be executed as part of a pre-build event.
    /// </summary>
    PreBuildEvent = 1,
    /// <summary>
    /// Indicates that the action can be executed as part of a post-build event.
    /// </summary>
    PostBuildEvent = 2,
    /// <summary>
    /// Indicates that the action can be executed outside of build events.
    /// </summary>
    Custom = 4,
    /// <summary>
    /// Indicates that the action can be executed as part of any build event.
    /// </summary>
    BuildEvent = PreBuildEvent | PostBuildEvent,
    /// <summary>
    /// Indicates that the action can be executed in any mode.
    /// </summary>
    All = PreBuildEvent | PostBuildEvent | Custom
}

/// <summary>
/// Represents an action that can be performed as part of a build event.
/// </summary>
public interface IBuildAction
{
    /// <summary>
    /// The name of the action.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The modes the action can be invoked in.
    /// </summary>
    public ActionModes SupportedModes { get; }

    /// <summary>
    /// The method called when action is executed.
    /// </summary>
    /// <param name="context">The context of the invocation.</param>
    /// <returns>The status code returned by the action. A return value of 0 indicates success.</returns>
    public int Execute(ActionContext context);
}
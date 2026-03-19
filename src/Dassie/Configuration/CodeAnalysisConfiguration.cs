using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Used to configure the behavior of a code analyzer.
/// </summary>
[XmlRoot]
[Serializable]
public class CodeAnalysisConfiguration
{
    /// <summary>
    /// Configures the severity of an analysis message.
    /// </summary>
    [XmlRoot("Configure")]
    [Serializable]
    public class Configure
    {
        /// <summary>
        /// Represents the severity of a message.
        /// </summary>
        public enum MessageSeverity
        {
            /// <summary>
            /// An information message.
            /// </summary>
            Information,
            /// <summary>
            /// A warning message.
            /// </summary>
            Warning,
            /// <summary>
            /// An error message.
            /// </summary>
            Error
        }

        /// <summary>
        /// Specifies the code of the message to configure.
        /// </summary>
        [XmlAttribute]
        public string Code { get; set; }

        /// <summary>
        /// Sets the severity of all messages with a code matching <see cref="Code"/>.
        /// </summary>
        [XmlAttribute]
        public MessageSeverity Severity { get; set; }
    }

    /// <summary>
    /// An array of message configurations.
    /// </summary>
    [XmlArray("Messages")]
    public Configure[] MessageConfigurations { get; set; } = [];
}
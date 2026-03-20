using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Used to configure the behavior of a code analyzer.
/// </summary>
[XmlRoot]
[Serializable]
public class CodeAnalysisConfiguration : ConfigObject
{
    /// <inheritdoc/>
    public CodeAnalysisConfiguration(PropertyStore store) : base(store) { }

    /// <summary>
    /// Configures the severity of an analysis message.
    /// </summary>
    [XmlRoot("Configure")]
    [Serializable]
    public class Configure : ConfigObject
    {
        /// <inheritdoc/>
        public Configure(PropertyStore store) : base(store) { }

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
        public string Code
        {
            get => Get<string>(nameof(Code));
            set => Set(nameof(Code), value);
        }

        /// <summary>
        /// Sets the severity of all messages with a code matching <see cref="Code"/>.
        /// </summary>
        [XmlAttribute]
        public MessageSeverity Severity
        {
            get => Get<MessageSeverity>(nameof(Severity));
            set => Set(nameof(Severity), value);
        }
    }

    /// <summary>
    /// An array of message configurations.
    /// </summary>
    [XmlArray("Messages")]
    public Configure[] MessageConfigurations
    {
        get => Get<Configure[]>(nameof(MessageConfigurations));
        set => Set(nameof(MessageConfigurations), value);
    }
}
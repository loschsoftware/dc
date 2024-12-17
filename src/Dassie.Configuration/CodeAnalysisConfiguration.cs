namespace Dassie.Configuration;

[XmlRoot]
[Serializable]
public class CodeAnalysisConfiguration
{
    [XmlRoot("Configure")]
    [Serializable]
    public class Configure
    {
        public enum MessageSeverity
        {
            Information,
            Warning,
            Error
        }

        [XmlAttribute]
        public string Code { get; set; }

        [XmlAttribute]
        public MessageSeverity Severity { get; set; }
    }

    [XmlArray("Messages")]
    public Configure[] MessageConfigurations { get; set; } = [];
}
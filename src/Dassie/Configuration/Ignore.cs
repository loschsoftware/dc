using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents an ignored message.
/// </summary>
[XmlRoot]
[Serializable]
public class Ignore
{
    /// <summary>
    /// The message code to ignore.
    /// </summary>
    [XmlText]
    public string Code { get; set; }
}

/// <summary>
/// Represents an ignored message.
/// </summary>
[XmlRoot]
[Serializable]
public class Message : Ignore { }

/// <summary>
/// Represents an ignored warning.
/// </summary>
[XmlRoot]
[Serializable]
public class Warning : Ignore { }
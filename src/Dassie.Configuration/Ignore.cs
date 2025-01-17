﻿namespace Dassie.Configuration;

[XmlRoot]
[Serializable]
public class Ignore
{
    [XmlText]
    public string Code { get; set; }
}

[XmlRoot]
[Serializable]
public class Message : Ignore { }

[XmlRoot]
[Serializable]
public class Warning : Ignore { }
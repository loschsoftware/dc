﻿using System;
using System.Xml.Serialization;

namespace Dassie.Configuration;

[Serializable]
[XmlRoot("Analyzer")]
public class CodeAnalyzer
{
    [XmlText]
    public string FullName { get; set; }
}
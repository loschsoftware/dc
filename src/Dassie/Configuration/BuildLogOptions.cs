using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Represents a list of build log devices.
/// </summary>
[Serializable]
[XmlRoot("Devices")]
public partial class BuildLogOptions : ConfigObject
{
    /// <inheritdoc/>
    public BuildLogOptions(PropertyStore store) : base(store) { }

    /// <summary>
    /// A list of XML elements representing the build log devices to enable.
    /// </summary>
    [XmlAnyElement]
    [ConfigProperty]
    public partial List<XmlElement> Elements { get; set; }
}
using System;
using System.Xml.Serialization;
namespace Dassie.Configuration;

[Serializable]
[XmlRoot("VersionInformation")]
public sealed class VersionInformation
{
    public VersionInformation(string product, string productVersion, string company, string copyright, string trademark)
    {
        Product = product;
        ProductVersion = productVersion;
        Company = company;
        Copyright = copyright;
        Trademark = trademark;
    }

    public VersionInformation() { }

    [XmlElement("Product")]
    public string Product { get; set; }

    [XmlElement("ProductVersion")]
    public string ProductVersion { get; set; }

    [XmlElement("Company")]
    public string Company { get; set; }

    [XmlElement("Copyright")]
    public string Copyright { get; set; }

    [XmlElement("Trademark")]
    public string Trademark { get; set; }
}
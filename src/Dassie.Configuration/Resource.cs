namespace Dassie.Configuration;

[XmlRoot]
[Serializable]
public abstract class Resource
{
    [XmlText]
    public string Path { get; set; }
}

[XmlRoot]
[Serializable]
public class UnmanagedResource : Resource { }

public enum ResourceType
{
    Unmanaged,
    Managed
}

[XmlRoot]
[Serializable]
public class ManagedResource : Resource
{
    [XmlAttribute]
    public string Name { get; set; }
}
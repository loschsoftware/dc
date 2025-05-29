namespace Dassie.CodeGeneration.Auxiliary;

internal class ResourceEntry
{
    public uint Language { get; set; }
    public byte[] Data { get; set; }
}

internal class ResourceLanguageGroup
{
    public ResourceEntry[] LanguageVariants { get; set; }
}

internal class ResourceTypeGroup
{
    public uint Id;
    public ResourceLanguageGroup[] Resources { get; set; }
}

internal class ResourceList
{
    public ResourceTypeGroup[] Types { get; set; }
}
namespace Dassie.CodeGeneration.Auxiliary;

internal enum ResourceKind : uint
{
    Version = 0x10
}

internal class NativeResource
{
    public byte[] Data { get; set; }

    public ResourceKind Kind { get; set; }
}
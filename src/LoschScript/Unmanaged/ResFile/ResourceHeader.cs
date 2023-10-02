using System.Runtime.InteropServices;

namespace LoschScript.Unmanaged.ResFile;

[StructLayout(LayoutKind.Sequential)]
internal struct ResourceHeader
{
    public uint DataSize;
    public uint HeaderSize;
    public uint Type;
    public uint Name;
    public uint DataVersion;
    public ushort MemoryFlags;
    public ushort LanguageId;
    public uint Version;
    public uint Characteristics;
}
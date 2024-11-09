using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Dassie.CodeGeneration.Auxiliary;

// TODO: Clean up this mess
internal class ResourceBuilder : ResourceSectionBuilder
{
    public ResourceBuilder(NativeResource[] resources)
    {
        _resources = resources;
    }

    private readonly NativeResource[] _resources;

    protected override void Serialize(BlobBuilder builder, SectionLocation location)
    {
        using MemoryStream ms = new();
        BinaryWriter bw = new(ms);

        // TODO: Make this whole thing work with more than just version infos
        _resources[0].Data = ResourceExtractor.ExtractVersionInfoResource(_resources[0].Data);

        bw.Write((uint)0);                      // Characteristics -> irrelevant
        bw.Write((uint)0);                      // Time Stamp -> irrelevant
        bw.Write((ushort)0);                    // Major version -> irrelevant
        bw.Write((ushort)0);                    // Minor version -> irrelevant
        bw.Write((ushort)0);                    // Number of named entries -> always 0, Dassie compiler only uses ID entries
        bw.Write((ushort)_resources.Length);    // Number of ID entries

        uint realOffset = 16;

        for (int i = 0; i < _resources.Length; i++)
        {
            //uint offset = (uint)location.PointerToRawData;

            //if (i > 0)
            //    offset += (uint)_resources[0..(i - 1)].Select(r => r.Length).Sum();

            realOffset += 8;
            bw.Write((uint)_resources[i].Kind);
            bw.Write(realOffset + 0x80000000);
        }

        //for (int i = 0; i < _resources.Length; i++)
        //{
        //    bw.Write(offsetToData);                     // Offset to data
        //    bw.Write((uint)_resources[i].Data.Length);  // Size
        //    bw.Write((uint)0);                          // Code page, irrelevant
        //    bw.Write((uint)0);                          // Reserved, irrelevant

        //    offsetToData += (uint)_resources[i].Data.Length;
        //}

        for (int i = 0; i < _resources.Length; i++)
        {
            // IMAGE_RESOURCE_DIRECTORY
            bw.Write((uint)0);     // Characteristics -> irrelevant
            bw.Write((uint)0);     // Time Stamp -> irrelevant
            bw.Write((ushort)0);   // Major version -> irrelevant
            bw.Write((ushort)0);   // Minor version -> irrelevant
            bw.Write((ushort)0);   // Number of named entries -> always 0, Dassie compiler only uses ID entries
            bw.Write((ushort)1);   // Number of ID entries
            realOffset += 16;

            // IMAGE_RESOURCE_DIRECTORY_ENTRY
            bw.Write((uint)1);
            bw.Write(realOffset + 0x80000000 + 8);
            realOffset += 8;

            bw.Write((uint)0);     // Characteristics -> irrelevant
            bw.Write((uint)0);     // Time Stamp -> irrelevant
            bw.Write((ushort)0);   // Major version -> irrelevant
            bw.Write((ushort)0);   // Minor version -> irrelevant
            bw.Write((ushort)0);   // Number of named entries -> always 0, Dassie compiler only uses ID entries
            bw.Write((ushort)1);   // Number of ID entries

            // IMAGE_RESOURCE_DIRECTORY_ENTRY
            bw.Write((uint)0);
            bw.Write(realOffset + 24);
            realOffset += 8;

            realOffset += 24;
        }

        for (int i = 0; i < _resources.Length; i++)
        {
            bw.Write((uint)location.RelativeVirtualAddress + realOffset + 8);  // Offset to data
            bw.Write((uint)_resources[i].Data.Length);                         // Data size
            bw.Write((uint)0);                                                 // Code page
            bw.Write((uint)0);                                                 // Reserved
            realOffset += 16;
        }

        foreach (byte[] res in _resources.Select(r => r.Data))
            bw.Write(res);

        bw.Flush();
        bw.Close();

        builder.WriteBytes(ms.ToArray());
    }
}
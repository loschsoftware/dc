using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Dassie.CodeGeneration.Auxiliary;

/// <summary>
/// Serializes the .rsrc section of a .NET assembly.
/// </summary>
internal class ResourceBuilder : ResourceSectionBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceBuilder"/> class with the specified resources.
    /// </summary>
    /// <param name="resources">A <see cref="ResourceList"/> containing the resources to include.</param>
    public ResourceBuilder(ResourceList resources)
    {
        _resources = resources;
    }

    private readonly ResourceList _resources;

    protected override void Serialize(BlobBuilder builder, SectionLocation location)
    {
        using MemoryStream ms = new();
        BinaryWriter bw = new(ms);

        bw.Write((uint)0);                            // Characteristics -> irrelevant
        bw.Write((uint)0);                            // Time Stamp -> irrelevant
        bw.Write((ushort)0);                          // Major version -> irrelevant
        bw.Write((ushort)0);                          // Minor version -> irrelevant
        bw.Write((ushort)0);                          // Number of named entries -> always 0, Dassie compiler only uses ID entries
        bw.Write((ushort)_resources.Types.Length);    // Number of ID entries

        uint realOffset = 16;

        for (int i = 0; i < _resources.Types.Length; i++)
        {
            bw.Write(_resources.Types[i].Id);
            bw.Write((uint)(0x80000000 + realOffset + _resources.Types.Length * 8 + i * 16));
        }

        realOffset += (uint)_resources.Types.Length * 8;

        foreach (ResourceTypeGroup rtg in _resources.Types)
        {
            foreach (ResourceLanguageGroup rlg in rtg.Resources)
            {
                bw.Write((uint)0);                               // Characteristics -> irrelevant
                bw.Write((uint)0);                               // Time Stamp -> irrelevant
                bw.Write((ushort)0);                             // Major version -> irrelevant
                bw.Write((ushort)0);                             // Minor version -> irrelevant
                bw.Write((ushort)0);                             // Number of named entries -> always 0, Dassie compiler only uses ID entries
                bw.Write((ushort)rlg.LanguageVariants.Length);   // Number of ID entries
                realOffset += 16;
            }

            foreach (ResourceLanguageGroup rlg in rtg.Resources)
            {
                foreach ((int i, ResourceEntry res) in rlg.LanguageVariants.Index())
                {
                    bw.Write(res.Language);
                    bw.Write((uint)(0x80000000 + realOffset + rtg.Resources.Length * 8 + i * 16));
                    realOffset += 8;
                }

                foreach (ResourceEntry res in rlg.LanguageVariants)
                {
                    bw.Write((uint)0);     // Characteristics -> irrelevant
                    bw.Write((uint)0);     // Time Stamp -> irrelevant
                    bw.Write((ushort)0);   // Major version -> irrelevant
                    bw.Write((ushort)0);   // Minor version -> irrelevant
                    bw.Write((ushort)0);   // Number of named entries -> always 0, Dassie compiler only uses ID entries
                    bw.Write((ushort)1);   // Number of ID entries

                    bw.Write((uint)0);
                    bw.Write(realOffset + 24);

                    realOffset += 24;
                }

                foreach (ResourceEntry res in rlg.LanguageVariants)
                {
                    bw.Write((uint)location.RelativeVirtualAddress + realOffset + 16);  // Offset to data
                    bw.Write((uint)res.Data.Length);                                   // Data size
                    bw.Write((uint)0);                                                 // Code page
                    bw.Write((uint)0);                                                 // Reserved
                    realOffset += 16;
                }

                foreach (ResourceEntry res in rlg.LanguageVariants)
                    bw.Write(res.Data);
            }
        }

        bw.Flush();
        bw.Close();

        builder.WriteBytes(ms.ToArray());
    }
}
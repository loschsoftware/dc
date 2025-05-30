using Microsoft.Cci;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Dassie.CodeGeneration.Auxiliary;

/// <summary>
/// Serializes the .rsrc section of a .NET assembly.
/// </summary>
internal class ResourceSerializer : ResourceSectionBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceSerializer"/> class with the specified resources.
    /// </summary>
    /// <param name="resources">A <see cref="ResourceList"/> containing the resources to include.</param>
    public ResourceSerializer(ResourceExtractor.Resource[] resources)
    {
        _resources = resources;
    }

    private readonly ResourceExtractor.Resource[] _resources;

    protected override void Serialize(BlobBuilder builder, SectionLocation location)
    {
        NativeResourceWriter.SerializeWin32Resources(builder, _resources.Select(r => new Win32Resource()
        {
            Data = r.Data,
            Id = r.Id == null ? -1 : (int)r.Id.Value,
            Name = r.Name,
            TypeId = r.ResourceType == null ? -1 : (int)r.ResourceType,
            TypeName = r.CustomType,
            LanguageId = r.LanguageId,
            CodePage = 0
        }), location.RelativeVirtualAddress);
    }

    //protected override void Serialize(BlobBuilder builder, SectionLocation location)
    //{
    //    using MemoryStream ms = new();
    //    BinaryWriter bw = new(ms);

    //    // Precompute sizes
    //    uint root_size = 16 + (uint)(8 * _resources.Types.Length);
    //    uint type_dirs_size = 0;
    //    uint resource_id_dirs_size = 0;
    //    int total_variants = 0;

    //    foreach (ResourceTypeGroup rtg in _resources.Types)
    //    {
    //        uint typeDirSize = 16 + (uint)(8 * rtg.Resources.Length);
    //        type_dirs_size += typeDirSize;

    //        foreach (ResourceId rid in rtg.Resources)
    //        {
    //            uint resDirSize = 16 + (uint)(8 * rid.LanguageVariants.Length);
    //            resource_id_dirs_size += resDirSize;
    //            total_variants += rid.LanguageVariants.Length;
    //        }
    //    }

    //    uint data_entries_start = root_size + type_dirs_size + resource_id_dirs_size;

    //    // Write root directory
    //    bw.Write(0u);                         // Characteristics
    //    bw.Write(0u);                         // Time Stamp
    //    bw.Write((ushort)0);                  // Major version
    //    bw.Write((ushort)0);                  // Minor version
    //    bw.Write((ushort)0);                  // Number of named entries
    //    bw.Write((ushort)_resources.Types.Length); // Number of ID entries

    //    // Calculate root directory entries with correct offsets
    //    uint currentOffset = root_size;
    //    for (int i = 0; i < _resources.Types.Length; i++)
    //    {
    //        bw.Write(_resources.Types[i].Id); // Resource type ID
    //        bw.Write(0x80000000 | currentOffset); // Offset to type directory
    //        currentOffset += 16 + (uint)(8 * _resources.Types[i].Resources.Length);
    //    }

    //    int data_entry_index = 0;

    //    // Write type directories and resource ID directories
    //    currentOffset = root_size;
    //    for (int i = 0; i < _resources.Types.Length; i++)
    //    {
    //        ResourceTypeGroup rtg = _resources.Types[i];

    //        // Write type directory header
    //        bw.Write(0u); // Characteristics
    //        bw.Write(0u); // Time Stamp
    //        bw.Write((ushort)0); // Major version
    //        bw.Write((ushort)0); // Minor version
    //        bw.Write((ushort)0); // Number of named entries
    //        bw.Write((ushort)rtg.Resources.Length); // Number of ID entries

    //        // Write type directory entries
    //        uint resDirOffset = currentOffset + 16 + (uint)(8 * rtg.Resources.Length);
    //        for (int j = 0; j < rtg.Resources.Length; j++)
    //        {
    //            ResourceId rid = rtg.Resources[j];
    //            bw.Write(rid.Id); // Resource ID
    //            bw.Write(0x80000000 | resDirOffset); // Offset to resource ID directory
    //            resDirOffset += 16 + (uint)(8 * rid.LanguageVariants.Length);
    //        }

    //        // Update current offset for next type group
    //        currentOffset += 16 + (uint)(8 * rtg.Resources.Length);

    //        // Write resource ID directories for this type
    //        for (int j = 0; j < rtg.Resources.Length; j++)
    //        {
    //            ResourceId rid = rtg.Resources[j];

    //            // Write resource ID directory header
    //            bw.Write(0u); // Characteristics
    //            bw.Write(0u); // Time Stamp
    //            bw.Write((ushort)0); // Major version
    //            bw.Write((ushort)0); // Minor version
    //            bw.Write((ushort)0); // Number of named entries
    //            bw.Write((ushort)rid.LanguageVariants.Length); // Number of ID entries

    //            // Write language entries
    //            for (int k = 0; k < rid.LanguageVariants.Length; k++)
    //            {
    //                bw.Write(rid.LanguageVariants[k].Language); // Language ID
    //                uint data_entry_offset = data_entries_start + (uint)(data_entry_index * 16);
    //                bw.Write(data_entry_offset); // Offset to data entry
    //                data_entry_index++;
    //            }
    //        }
    //    }

    //    // Write data entries (with dummy data offsets for now)
    //    List<long> dataEntryPositions = new List<long>();
    //    List<byte[]> resourceData = new List<byte[]>();

    //    foreach (ResourceTypeGroup rtg in _resources.Types)
    //    {
    //        foreach (ResourceId rid in rtg.Resources)
    //        {
    //            foreach (ResourceEntry rent in rid.LanguageVariants)
    //            {
    //                long pos = ms.Position;
    //                bw.Write(0u); // OffsetToData (dummy)
    //                bw.Write(rent.Data.Length);
    //                bw.Write(0u); // Code page
    //                bw.Write(0u); // Reserved
    //                dataEntryPositions.Add(pos);
    //                resourceData.Add(rent.Data);
    //            }
    //        }
    //    }

    //    // Align to 8 bytes for raw data
    //    long currentPos = ms.Position;
    //    long alignedPos = (currentPos + 7) & ~7;
    //    while (ms.Position < alignedPos)
    //        bw.Write((byte)0);

    //    // Write raw data and fix data entries
    //    for (int i = 0; i < resourceData.Count; i++)
    //    {
    //        long rawDataPos = ms.Position;
    //        bw.Write(resourceData[i]);

    //        long savedPos = ms.Position;
    //        ms.Seek(dataEntryPositions[i], SeekOrigin.Begin);
    //        bw.Write(location.RelativeVirtualAddress + (uint)rawDataPos);
    //        ms.Seek(savedPos, SeekOrigin.Begin);

    //        // Align next resource to 8 bytes
    //        if (i < resourceData.Count - 1)
    //        {
    //            currentPos = ms.Position;
    //            alignedPos = (currentPos + 7) & ~7;
    //            while (ms.Position < alignedPos)
    //                bw.Write((byte)0);
    //        }
    //    }

    //    bw.Flush();
    //    builder.WriteBytes(ms.ToArray());
    //}
}
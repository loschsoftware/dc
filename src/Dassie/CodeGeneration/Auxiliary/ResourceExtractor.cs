using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dassie.CodeGeneration.Auxiliary;

/// <summary>
/// Provides helper functions for extracting resource entries out of Windows resource files.
/// </summary>
internal static class ResourceExtractor
{
    /// <summary>
    /// Represents a predefined, numeric resource type.
    /// </summary>
    public enum ResourceType : uint
    {
        Accelerator = 9,
        AniCursor = 21,
        AniIcon = 22,
        Bitmap = 2,
        Cursor = 1,
        Dialog = 5,
        DlgInclude = 17,
        Font = 8,
        FontDir = 7,
        GroupCursor = Cursor + 11,
        GroupIcon = Icon + 11,
        Html = 23,
        Icon = 3,
        Manifest = 24,
        Menu = 4,
        MessageTable = 11,
        PlugPlay = 19,
        RcData = 10,
        String = 6,
        Version = 16,
        Vxd = 20
    }

    /// <summary>
    /// Represents an entry in a resource file.
    /// </summary>
    public record struct Resource
    {
        public uint DataSize;
        public uint HeaderSize;
        public ResourceType? ResourceType;
        public string CustomType;
        public uint? Id;
        public string Name;
        public uint DataVersion;
        public ushort MemoryFlags;
        public ushort LanguageId;
        public uint Version;
        public uint Characteristics;
        public byte[] Data;
    }

    /// <summary>
    /// Reads a null-terminated Unicode string from the input.
    /// </summary>
    /// <param name="br">The <see cref="BinaryReader"/> to read from.</param>
    /// <param name="firstChar">The first character of the string.</param>
    /// <returns>A string read from the input.</returns>
    private static string ReadString(BinaryReader br, ushort firstChar)
    {
        List<byte> bytes = [];
        bytes.AddRange(BitConverter.GetBytes(firstChar));

        while (true)
        {
            ushort codePoint = br.ReadUInt16();

            if (codePoint == 0)
                break;

            bytes.AddRange(BitConverter.GetBytes(codePoint));
        }

        return Encoding.Unicode.GetString(bytes.ToArray());
    }

    /// <summary>
    /// Reads all resource entries from the specified buffer.
    /// </summary>
    /// <param name="buffer">An array of bytes representing the bytes of a Windows resource file.</param>
    /// <param name="fileName">The file name to be used in error messages.</param>
    /// <returns>An array of <see cref="Resource"/> objects representing the resource entries of the file.</returns>
    public static Resource[] GetResources(byte[] buffer, string fileName)
    {
        List<Resource> resources = [];

        using MemoryStream ms = new(buffer);
        using BinaryReader br = new(ms);

        try
        {
            // Skip the empty first entry
            int offset = 0x20;
            br.ReadBytes(0x20);

            while (br.PeekChar() != -1)
            {
                uint dataSize = br.ReadUInt32();
                uint headerSize = br.ReadUInt32();

                ushort typeFirstChar = 0;
                uint? type = null;
                string typeName = null;

                if ((typeFirstChar = br.ReadUInt16()) == 0xFFFF)
                    type = br.ReadUInt16();
                else
                    typeName = ReadString(br, typeFirstChar);

                ushort nameFirstChar = 0;
                uint? id = null;
                string name = null;

                if ((nameFirstChar = br.ReadUInt16()) == 0xFFFF)
                    id = br.ReadUInt16();
                else
                    name = ReadString(br, nameFirstChar);

                if (br.BaseStream.Position % 4 != 0)
                    br.ReadUInt16();

                uint dataVersion = br.ReadUInt32();
                ushort memoryFlags = br.ReadUInt16();
                ushort languageId = br.ReadUInt16();
                uint version = br.ReadUInt32();
                uint characteristics = br.ReadUInt32();

                byte[] data = br.ReadBytes((int)dataSize);

                resources.Add(new()
                {
                    DataSize = dataSize,
                    HeaderSize = headerSize,
                    ResourceType = (ResourceType?)type,
                    CustomType = typeName,
                    Id = id,
                    Name = name,
                    DataVersion = dataVersion,
                    MemoryFlags = memoryFlags,
                    LanguageId = languageId,
                    Version = version,
                    Characteristics = characteristics,
                    Data = data
                });

                offset += (int)(dataSize + headerSize);

                if (offset % 8 != 0)
                {
                    int alignment = 8 - (offset % 8);
                    br.ReadBytes(alignment);
                    offset += alignment;
                }
            }
        }
        catch (Exception)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0207_InvalidResourceFile,
                $"The resource file '{fileName}' is malformed or contains unsupported constructs.",
                fileName);
        }

        return resources.ToArray();
    }

    /// <summary>
    /// Constructs a <see cref="ResourceList"/> from the specified resources.
    /// </summary>
    /// <param name="resources">The resources to include in the resource list.</param>
    /// <returns>An object of type <see cref="ResourceList"/> representing the specified resource entries.</returns>
    public static ResourceList GetResourceList(Resource[] resources)
    {
        List<ResourceTypeGroup> typeGroups = [];

        foreach (var typeGroup in resources.GroupBy(r => r.ResourceType))
        {
            List<ResourceLanguageGroup> langGroups = [];

            foreach (var langGroup in typeGroup.GroupBy(t => t.LanguageId))
            {
                List<ResourceEntry> entries = [];

                foreach (Resource res in langGroup)
                {
                    entries.Add(new()
                    {
                        Language = langGroup.Key,
                        Data = res.Data
                    });
                }

                langGroups.Add(new()
                {
                    LanguageVariants = entries.ToArray()
                });
            }

            ResourceTypeGroup rtg = new()
            {
                Id = (uint)typeGroup.Key.Value,
                Resources = langGroups.ToArray()
            };

            typeGroups.Add(rtg);
        }

        ResourceList rl = new()
        {
            Types = typeGroups.ToArray()
        };

        return rl;
    }
}
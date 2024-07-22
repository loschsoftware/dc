using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Dassie.CodeGeneration.Auxiliary;

internal class VersionInfoBuilder : ResourceSectionBuilder
{
    public VersionInfoBuilder(string resFilePath)
    {
        if (resFilePath == null)
            return;

        rawBytes = File.ReadAllBytes(resFilePath);
    }

    private readonly byte[] rawBytes = null;

    protected override void Serialize(BlobBuilder builder, SectionLocation location)
    {
        if (rawBytes == null)
            return;

        builder.WriteBytes(rawBytes);
    }
}
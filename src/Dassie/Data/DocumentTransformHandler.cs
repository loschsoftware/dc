using Dassie.Configuration;
using Dassie.Extensions;
using Ganss.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Dassie.Data;

internal static class DocumentTransformHandler
{
    public static List<Document> Transform(List<Document> input, DocumentTransformerList transformerList)
    {
        List<IDocumentTransformer> transformers = [];
        HashSet<string> added = [];

        foreach (XmlElement elem in transformerList?.Transformers ?? [])
        {
            if (ExtensionLoader.DocumentTransformers.FirstOrDefault(t => t.Name == elem.LocalName) is IDocumentTransformer transformer
                && added.Add(transformer.Name))
            {
                transformers.Add(transformer);
            }
        }

        return Transform(input, transformers);
    }

    public static List<Document> Transform(List<Document> input, IEnumerable<IDocumentTransformer> transformers)
    {
        List<IDocumentTransformer> transformerList = [.. transformers];
        Dictionary<IDocumentTransformer, List<Document>> buckets = [];
        List<Document> result = [.. input];

        foreach (Document doc in input)
        {
            if (transformerList.FirstOrDefault(t => Glob.IsMatch(t.Pattern, doc.FilePath)) is not IDocumentTransformer transformer)
                continue;

            if (!buckets.TryGetValue(transformer, out List<Document> docs))
            {
                docs = [];
                buckets[transformer] = docs;
            }

            docs.Add(doc);
        }

        foreach ((IDocumentTransformer transformer, List<Document> docs) in buckets)
        {
            List<Document> transformed = [.. (transformer.Transform(docs) ?? [])];

            if (transformer.TransformMode == DocumentTransformMode.Overwrite)
            {
                HashSet<Document> originals = [.. docs];
                result.RemoveAll(originals.Contains);
            }

            result.AddRange(transformed);

            EmitBuildLogMessageFormatted(
                nameof(StringHelper.DocumentTransformHandler_BuildLogMessage),
                [DocListToString(docs), DocListToString(transformed), transformer.Name]);
        }

        return result;
    }

    private static string DocListToString(List<Document> docs)
    {
        StringBuilder sb = new();
        sb.Append('[');

        for (int i = 0; i < docs.Count; i++)
        {
            sb.Append(docs[i].Name);

            if (i < docs.Count - 1)
                sb.Append(", ");
        }

        sb.Append(']');
        return sb.ToString();
    }
}
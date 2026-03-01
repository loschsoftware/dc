using Dassie.Configuration;
using Dassie.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Dassie.Data;

internal static class DocumentSourceManager
{
    public static List<InputDocument> GetDocuments(DassieConfig config)
    {
        if (config.DocumentSources == null || config.DocumentSources.Sources.Count == 0)
            return [];

        List<InputDocument> docs = [];

        foreach (XmlElement sourceElement in config.DocumentSources.Sources)
        {
            string sourceName = sourceElement.Name;

            if (!ExtensionLoader.DocumentSources.Any(d => d.Name == sourceName))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0232_DocumentSourceNotFound,
                    nameof(StringHelper.DocumentSourceManager_SourceNotFound), [sourceName],
                    ProjectConfigurationFileName);

                continue;
            }

            IEnumerable<XmlAttribute> attribs = sourceElement.Attributes.Cast<XmlAttribute>();
            IEnumerable<XmlNode> nodes = sourceElement.Attributes.Cast<XmlNode>();

            IDocumentSource source = ExtensionLoader.DocumentSources.First(d => d.Name == sourceName);
            string text = source.GetText(attribs.ToList(), nodes.ToList());

            if (docs.Any(d => d.Name == source.DocumentName))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0233_DocumentSourcesDuplicateDocumentName,
                    nameof(StringHelper.DocumentSourceManager_DuplicateDocument), [source.DocumentName],
                    ProjectConfigurationFileName);

                continue;
            }

            docs.Add(new(text, source.DocumentName));
        }

        return docs;
    }
}
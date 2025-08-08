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
                EmitErrorMessage(
                    0, 0, 0,
                    DS0231_DocumentSourceNotFound,
                    $"The document source '{sourceName}' could not be found.",
                    ProjectConfigurationFileName);

                continue;
            }

            IEnumerable<XmlAttribute> attribs = sourceElement.Attributes.Cast<XmlAttribute>();
            IEnumerable<XmlNode> nodes = sourceElement.Attributes.Cast<XmlNode>();

            IDocumentSource source = ExtensionLoader.DocumentSources.First(d => d.Name == sourceName);
            string text = source.GetText(attribs.ToList(), nodes.ToList());

            if (docs.Any(d => d.Name == source.DocumentName))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0232_DocumentSourcesDuplicateDocumentName,
                    $"Multiple document sources attempted to write to document '{source.DocumentName}'.",
                    ProjectConfigurationFileName);

                continue;
            }

            docs.Add(new(text, source.DocumentName));
        }

        return docs;
    }
}
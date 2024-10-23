using System.Collections.Generic;
using System.Linq;

namespace Dassie.Data;

internal static class DocumentCommandLineManager
{
    public static IEnumerable<InputDocument> ExtractDocuments(string[] args)
    {
        List<InputDocument> docs = [];

        foreach (string documentArg in args.Where(arg => arg.StartsWith("--Document:")))
        {
            string name = documentArg.Split(':')[1];
            string value = string.Join(":", documentArg.Split(':')[2..]);
            docs.Add(new(value, name));
        }

        return docs;
    }
}
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Data;

/// <summary>
/// Extracts input documents out of command-line arguments passed to the compiler.
/// </summary>
internal static class DocumentCommandLineManager
{
    /// <summary>
    /// Extracts input documents from command-line arguments starting with '--Document'.
    /// </summary>
    /// <param name="args">An array of command-line arguments.</param>
    /// <returns>An enumerable of <see cref="InputDocument"/> instances representing the documents extracted from the command-line arguments that were passed.</returns>
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
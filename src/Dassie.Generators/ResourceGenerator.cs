using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Dassie.Generators;

[Generator]
public sealed class ResourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AdditionalText> jsonFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith("strings.en-US.json"));

        IncrementalValuesProvider<string> content = jsonFiles
            .Select((file, cancellationToken) =>
                file.GetText(cancellationToken)?.ToString());

        context.RegisterSourceOutput(content, (spc, json) =>
        {
            if (string.IsNullOrWhiteSpace(json))
                return;

            using JsonDocument doc = JsonDocument.Parse(json);

            List<(string, string)> resourceEntries = doc.RootElement
                .EnumerateObject()
                .Select(p => (p.Name, p.Value.ToString()))
                .ToList();

            StringBuilder sb = new();
            sb.AppendLine("namespace Dassie.Resources;");
            sb.AppendLine("internal static partial class StringHelper");
            sb.AppendLine("{");

            for (int i = 0; i < resourceEntries.Count; i++)
            {
                (string key, string value) = resourceEntries[i];

                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// {WebUtility.HtmlEncode(value)}");
                sb.AppendLine($"    /// </summary>");
                sb.AppendLine($"    public static string {key} => GetString(nameof({key}));");
                
                if (i < resourceEntries.Count - 1)
                    sb.AppendLine();
            }

            sb.AppendLine("}");

            spc.AddSource("StringHelper.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }
}
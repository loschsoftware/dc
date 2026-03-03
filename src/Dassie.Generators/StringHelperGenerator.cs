using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Dassie.Generators;

[Generator]
public sealed class StringHelperGenerator : IIncrementalGenerator
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

            StringBuilder stringHelperBuilder = new();
            stringHelperBuilder.AppendLine("namespace Dassie.Resources;");
            stringHelperBuilder.AppendLine("internal static partial class StringHelper");
            stringHelperBuilder.AppendLine("{");

            for (int i = 0; i < resourceEntries.Count; i++)
            {
                (string key, string value) = resourceEntries[i];

                string escapedValue = value.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n");

                stringHelperBuilder.AppendLine($"    /// <summary>");
                stringHelperBuilder.AppendLine($"    /// {escapedValue}");
                stringHelperBuilder.AppendLine($"    /// </summary>");
                stringHelperBuilder.AppendLine($"    public static string {key} => GetString(nameof({key}));");
                
                if (i < resourceEntries.Count - 1)
                    stringHelperBuilder.AppendLine();
            }

            stringHelperBuilder.AppendLine("}");
            spc.AddSource("StringHelper.g.cs", SourceText.From(stringHelperBuilder.ToString(), Encoding.UTF8));
        });
    }
}
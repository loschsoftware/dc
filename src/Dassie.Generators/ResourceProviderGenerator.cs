using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Dassie.Generators;

[Generator]
public sealed class ResourceProviderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var optionsProvider = context.AnalyzerConfigOptionsProvider.Select((p, _) =>
        {
            p.GlobalOptions.TryGetValue("build_property.ResourceProviderGenerator_UseExplicitKeys", out string useExplicitKeysStr);
            p.GlobalOptions.TryGetValue("build_property.ResourceProviderGenerator_InputFileName", out string inputFileName);
            p.GlobalOptions.TryGetValue("build_property.ResourceProviderGenerator_OutputNamespace", out string outputNamespace);
            p.GlobalOptions.TryGetValue("build_property.ResourceProviderGenerator_OutputTypeName", out string outputTypeName);

            return (
                UseExplicitKeys: bool.TryParse(useExplicitKeysStr, out bool b) && b,
                InputFileName: inputFileName ?? "strings.en-US.json",
                OutputNamespace: outputNamespace ?? "Dassie.Resources",
                OutputTypeName: outputTypeName ?? "DefaultStrings"
            );
        });

        var jsonFilesWithOptions = context.AdditionalTextsProvider
            .Combine(optionsProvider)
            .Where(pair => pair.Left.Path.EndsWith(pair.Right.InputFileName))
            .Select((pair, ct) => (
                Content: pair.Left.GetText(ct)?.ToString(),
                Options: pair.Right
            ));

        IncrementalValuesProvider<string> content = jsonFilesWithOptions
            .Select((j, _) =>
                j.Content);

        context.RegisterSourceOutput(jsonFilesWithOptions, (spc, data) =>
        {
            if (string.IsNullOrWhiteSpace(data.Content))
                return;

            (bool useExplicitKeys, _, string outputNamespace, string outputTypeName) = data.Options;

            using JsonDocument doc = JsonDocument.Parse(data.Content);

            List<(string, string)> resourceEntries = doc.RootElement
                .EnumerateObject()
                .Select(p => (p.Name, p.Value.ToString()))
                .ToList();

            StringBuilder sb = new();
            sb.AppendLine("using Dassie.Extensions;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine($"namespace {outputNamespace};");
            sb.AppendLine($"internal partial class {outputTypeName} : IResourceProvider<string>");
            sb.AppendLine("{");

            sb.AppendLine("    public Dictionary<string, string> Resources { get; } = new()");
            sb.AppendLine("    {");

            for (int i = 0; i < resourceEntries.Count; i++)
            {
                (string key, string value) = resourceEntries[i];

                string keyName = useExplicitKeys ? $"\"{key}\"" : $"nameof(StringHelper.{key})";
                string escapedValue = value.Replace("\"", "\"\"");

                sb.AppendLine($"        [{keyName}] = @\"{escapedValue}\",");

                if (i < resourceEntries.Count - 1)
                    sb.AppendLine();
            }

            sb.AppendLine("    };");
            sb.AppendLine("}");
            spc.AddSource($"{outputTypeName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }
}
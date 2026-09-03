using Dassie.Syntax.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Dassie.Helpers;

/// <summary>
/// Provides functionality for exporting trees as Mermaid flowcharts.
/// </summary>
internal static class TreeExportHelper
{
    /// <summary>
    /// Specifies the role of a node, which corresponds to a distinct visual style.
    /// </summary>
    public enum NodeStyle
    {
        /// <summary>
        /// Represents a regular node with children.
        /// </summary>
        Regular,
        /// <summary>
        /// Represents a visual grouping node not corresponding to a syntax node.
        /// </summary>
        Synthetic,
        /// <summary>
        /// Represents a terminal node.
        /// </summary>
        Terminal,
        /// <summary>
        /// Represents a special terminal inserted by the generator, with no corresponding syntax node.
        /// </summary>
        Special
    }

    /// <summary>
    /// Represents a tree node to export.
    /// </summary>
    /// <param name="Value">The value of the node.</param>
    /// <param name="Data">Additional data attached to the node.</param>
    /// <param name="Children">A collection of labeled edges to zero or more children.</param>
    /// <param name="Style">The style of the node.</param>
    public record Node(
        string Value,
        IReadOnlyList<string> Data,
        IEnumerable<(string Label, IReadOnlyList<Node> Nodes)> Children,
        NodeStyle Style = NodeStyle.Regular);

    private static readonly string Indent = "    ";
    private static Node EmptyNode => new("<empty>", [], [], NodeStyle.Special);

    /// <summary>
    /// Exports a tree in a textual format.
    /// </summary>
    /// <param name="node">The node to export.</param>
    /// <returns>A <see cref="string"/> representing the tree in textual format.</returns>
    public static string ExportTextual(Node node)
    {
        return null;
    }

    /// <summary>
    /// Exports a tree as a Mermaid flowchart.
    /// </summary>
    /// <param name="node">The node to export.</param>
    /// <returns>A <see cref="string"/> of Mermaid source code representing the tree.</returns>
    public static string ExportMermaid(Node node)
    {
        static string Escape(string value)
        {
            string escapedString = StringHelpers.EscapeString(value);
            if (value.StartsWith('"') && value.EndsWith('"'))
                escapedString = $"\"{escapedString[2..^2]}\"";

            return HttpUtility.HtmlEncode(escapedString);
        }

        StringBuilder sb = new();
        sb.AppendLine("flowchart TD");
        sb.AppendLine($"{Indent}classDef synthetic stroke-dasharray: 5 5");
        sb.AppendLine($"{Indent}classDef special stroke-dasharray: 2 2,font-style: italic\r\n");

        int index = 0;
        Dictionary<Node, string> nodes = new(ReferenceEqualityComparer.Instance);

        string GetId(Node node)
        {
            if (!nodes.TryGetValue(node, out string nodeId))
            {
                string start = "[";
                string end = "]";

                if (node.Style is NodeStyle.Terminal or NodeStyle.Special)
                {
                    start = "([";
                    end = "])";
                }

                nodeId = $"node{++index}";
                sb.AppendLine($"{Indent}{nodeId}{start}\"{Escape(node.Value)}\"{end}");
                nodes.Add(node, nodeId);

                if (node.Style == NodeStyle.Synthetic)
                    sb.AppendLine($"{Indent}class {nodeId} synthetic");

                if (node.Style == NodeStyle.Special)
                    sb.AppendLine($"{Indent}class {nodeId} special");
            }

            return nodeId;
        }

        void AddEdge(string from, string to, string label)
        {
            string labelStr = string.IsNullOrEmpty(label) ? "" : $"|\"{Escape(label)}\"|";
            sb.AppendLine($"{Indent}{from} -->{labelStr} {to}");
        }

        void AddNode(Node node)
        {
            string id = GetId(node);

            foreach ((string label, IReadOnlyList<Node> children) in node.Children ?? [])
            {
                if (children == null || children.Count == 0)
                {
                    AddEdge(id, GetId(EmptyNode), label);
                }
                else if (children.Count == 1)
                {
                    Node child = children[0];
                    AddNode(child);
                    AddEdge(id, GetId(child), label);
                }
                else
                {
                    Node group = new(label, [], [], NodeStyle.Synthetic);

                    string groupId = GetId(group);
                    AddEdge(id, groupId, label);

                    foreach ((int i, Node child) in children.Index())
                    {
                        AddNode(child);
                        AddEdge(groupId, GetId(child), $"[{i + 1}]");
                    }
                }
            }
        }

        AddNode(node);
        return sb.ToString();
    }
}
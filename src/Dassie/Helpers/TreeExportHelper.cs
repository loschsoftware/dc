using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Dassie.Helpers;

internal static class TreeExportHelper
{
    public record Node(
        string Value,
        List<string> Data,
        IEnumerable<(string Label, IReadOnlyList<Node> Nodes)> Children);

    private static readonly string Indent = "    ";
    private static readonly Node EmptyNode = new("<empty>", [], []);

    public static string ExportTextual(Node node)
    {
        return null;
    }

    public static string ExportMermaid(Node node)
    {
        static string Escape(string value) => HttpUtility.HtmlEncode(value);

        StringBuilder sb = new();
        sb.AppendLine("flowchart TD");

        int index = 0;
        Dictionary<Node, string> nodes = new(ReferenceEqualityComparer.Instance);

        string GetId(Node node)
        {
            if (!nodes.TryGetValue(node, out string nodeId))
            {
                nodeId = $"node{++index}";
                sb.AppendLine($"{Indent}{nodeId}[\"{Escape(node.Value)}\"]");
                nodes.Add(node, nodeId);
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
                    Node group = new(label, [], []);

                    string groupId = GetId(group);
                    AddEdge(id, groupId, label);

                    foreach (Node child in children)
                    {
                        AddNode(child);
                        AddEdge(groupId, GetId(child), "");
                    }
                }
            }
        }

        AddNode(node);
        return sb.ToString();
    }
}
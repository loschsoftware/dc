using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Dassie.Helpers;

internal static class TreeExportHelper
{
    public record Node(string Value, IEnumerable<Node> Children);

    public static string ExportTextual(Node node)
    {
        return null;
    }

    public static string ExportMermaid(Node node)
    {
        int nodeId = 0;

        void AddNode(Node node, StringBuilder sb)
        {
            string nodeName = $"node{++nodeId}";

            foreach (Node child in node.Children ?? [])
            {
                int childNodeId = nodeId;
                string childName = $"node{++childNodeId}";

                sb.AppendLine($"    {nodeName}[\"{HttpUtility.HtmlEncode(node.Value)}\"] --> {childName}[\"{HttpUtility.HtmlEncode(child.Value)}\"]");
                AddNode(child, sb);
            }
        }

        StringBuilder sb = new();
        sb.AppendLine("flowchart TD");
        AddNode(node, sb);
        return sb.ToString();
    }
}
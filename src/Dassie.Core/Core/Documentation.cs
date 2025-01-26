using Dassie.Text.Tooltips;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

#pragma warning disable IDE1006
#pragma warning disable IDE0079
#pragma warning disable IL3000

namespace Dassie.Core;

/// <summary>
/// Provides functionality for extracting XML documentation for types and members.
/// </summary>
public static class Documentation
{
    private class XmlDoc
    {
        public readonly Dictionary<string, string> MethodSummaries = new();

        public XmlDoc(string xmlFile)
        {
            XDocument doc = XDocument.Load(xmlFile);
            foreach (XElement element in doc.Element("doc").Element("members").Elements())
            {
                string xmlName = element.Attribute("name").Value;
                string xmlSummary = element.Element("summary").Value.Trim();
                MethodSummaries[xmlName] = xmlSummary;
            }
        }

        public static string GetXmlName(MethodInfo info)
        {
            string declaringTypeName = info.DeclaringType.FullName;

            if (declaringTypeName is null)
                throw new NotImplementedException("inherited classes are not supported");

            string xmlName = "M:" + declaringTypeName + "." + info.Name;
            xmlName = string.Join("", xmlName.Split(']').Select(x => x.Split('[')[0]));
            xmlName = xmlName.Replace(",", "");

            if (info.IsGenericMethod)
                xmlName += "``#";

            int genericParameterCount = 0;
            List<string> paramNames = new List<string>();
            foreach (var parameter in info.GetParameters())
            {
                Type paramType = parameter.ParameterType;
                string paramName = GetXmlNameForMethodParameter(paramType);
                if (paramName.Contains("#"))
                    paramName = paramName.Replace("#", (genericParameterCount++).ToString());
                paramNames.Add(paramName);
            }
            xmlName = xmlName.Replace("#", genericParameterCount.ToString());

            if (paramNames.Any())
                xmlName += "(" + string.Join(",", paramNames) + ")";

            return xmlName;
        }

        private static string GetXmlNameForMethodParameter(Type type)
        {
            string xmlName = type.FullName ?? type.BaseType.FullName;
            bool isNullable = xmlName.StartsWith("System.Nullable");
            Type nullableType = isNullable ? type.GetGenericArguments()[0] : null;

            // special formatting for generics (also Func, Nullable, and ValueTulpe)
            if (type.IsGenericType)
            {
                var genericNames = type.GetGenericArguments().Select(x => GetXmlNameForMethodParameter(x));
                var typeName = type.FullName.Split('`')[0];
                xmlName = typeName + "{" + string.Join(",", genericNames) + "}";
            }

            // special case for generic nullables
            if (type.IsGenericType && isNullable && type.IsArray == false)
                xmlName = "System.Nullable{" + nullableType.FullName + "}";

            // special case for multidimensional arrays
            if (type.IsArray && (type.GetArrayRank() > 1))
            {
                string arrayName = type.FullName.Split('[')[0].Split('`')[0];
                if (isNullable)
                    arrayName += "{" + nullableType.FullName + "}";
                string arrayContents = string.Join(",", Enumerable.Repeat("0:", type.GetArrayRank()));
                xmlName = arrayName + "[" + arrayContents + "]";
            }

            // special case for generic arrays
            if (type.IsArray && type.FullName is null)
                xmlName = "``#[]";

            // special case for value types
            if (xmlName.Contains("System.ValueType"))
                xmlName = "`#";

            return xmlName;
        }
    }

    /// <summary>
    /// Prints the documentation for a type and its members in human-readable form.
    /// </summary>
    /// <param name="type">The type to print documentation for.</param>
    public static void help(Type type)
    {
        string docPath = Path.ChangeExtension(type.Assembly.Location, "xml");

        if (!File.Exists(docPath))
        {
            Console.WriteLine($"Error: Could not locate documentation for '{type.FullName}'.");
            return;
        }

        XmlDoc doc = new(docPath);

        StringBuilder sb = new();

        sb.AppendLine($"{type.FullName}:");

        sb.AppendLine("\tMethods:");
        sb.AppendLine();

        foreach (MethodInfo method in type.GetMethods())
        {
            try
            {
                string summary = doc.MethodSummaries[XmlDoc.GetXmlName(method)];

                if (string.IsNullOrEmpty(summary))
                    continue;

                if (string.IsNullOrWhiteSpace(summary))
                    continue;

                sb.AppendLine($"\t{string.Join("", TooltipGenerator.Function(method).Words.Select(w => w.Text))}");
                sb.AppendLine($"\t\t{summary}");
                sb.AppendLine();
            }
            catch
            {
                continue;
            }
        }

        Console.Write(sb.ToString());
    }
}
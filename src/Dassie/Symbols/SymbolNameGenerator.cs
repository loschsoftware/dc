using System;
using System.Text;

namespace Dassie.Symbols;

/// <summary>
/// Helper class to generate names for compiler-generated symbols.
/// </summary>
internal static class SymbolNameGenerator
{
    //private const string GeneratedPrefix = "<g>";
    private const string GeneratedPrefix = "";

    public static string GetDelegateTypeName(Type returnType, Type[] paramTypes)
    {
        StringBuilder sb = new();
        sb.Append($"{GeneratedPrefix}Function$(");

        if (paramTypes != null && paramTypes.Length > 0)
        {
            foreach (Type t in paramTypes[..^1])
                sb.Append($"{t.ToString().Replace('.', '_')}, ");
            sb.Append($"{paramTypes[^1].ToString().Replace('.', '_')}");
        }

        sb.Append(") ->");
        sb.Append(returnType.ToString().Replace('.', '_'));
        return sb.ToString();
    }

    // Closures
    public static string GetClosureTypeName(string methodName) => $"{GeneratedPrefix}{methodName}$Closure";
    public static string GetClosureFieldName(int fieldIndex) => $"{GeneratedPrefix}_{fieldIndex}";
    public static string GetClosureLocalName(string closure) => $"{GeneratedPrefix}{closure}$_field$";
    public static string GetAnonymousFunctionName(int index) => $"{GeneratedPrefix}func'{index}";
    public static string GetBaseInstanceName(string methodName) => $"{GeneratedPrefix}{methodName}$_BaseInstance";

    // Properties
    public static string GetPropertyBackingFieldName(string propertyName) => $"{GeneratedPrefix}_{char.ToLower(propertyName[0])}{propertyName[1..]}_BackingField";

    public static string GetInlineUnionTypeName(int index) => $"{GeneratedPrefix}$Union'{index}";
}
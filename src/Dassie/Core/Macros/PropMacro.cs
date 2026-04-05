using Dassie.Configuration;
using Dassie.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dassie.Core.Macros;

internal class PropMacro(object element) : IMacro
{
    private readonly record struct Segment(string Member, List<string> Indices);

    public string Name => "Prop";

    private readonly List<MacroParameter> _params = [new("PropertyName", true)];
    public List<MacroParameter> Parameters => _params;

    public MacroOptions Options => MacroOptions.None;

    public string Expand(Dictionary<string, string> arguments, MacroInvocationInfo info)
    {
        string expr = arguments["PropertyName"] ?? "";
        List<Segment> segments = ParseSegments(expr);

        object val = element;

        foreach (Segment segment in segments)
        {
            if (val is ConfigObject cobj)
                val = cobj.Store.Get(segment.Member);
            else if (val is XElement elem)
            {
                IEnumerable<XElement> matchingElems = elem.Elements(segment.Member);
                val = matchingElems;

                if (matchingElems == null || !matchingElems.Any())
                    val = elem.Attributes(segment.Member);

                if (val is IEnumerable enumerable)
                {
                    IEnumerable<object> elems = enumerable.Cast<object>();

                    if (!elems.Any())
                        val = null;
                    else if (elems.Count() == 1)
                        val = elems.First();
                }
            }

            if (val == null)
            {
                EmitErrorMessageFormatted(
                    info.Line,
                    info.Column,
                    expr.Length,
                    DS0282_PropMacroInvalidMemberAccess,
                    nameof(StringHelper.PropMacro_InvalidMemberReference), [segment.Member, expr],
                    info.Document);

                return "";
            }

            if (segment.Indices.Count > 0)
            {
                foreach (string indexExpr in segment.Indices)
                {
                    if (val is XElement _elem)
                        val = _elem.Value;
                    else if (val is XAttribute _attrib)
                        val = _attrib.Value;

                    if (val is not IEnumerable enumerable)
                    {
                        EmitErrorMessageFormatted(
                            info.Line,
                            info.Column,
                            expr.Length,
                            DS0281_PropMacroIndexElementNotEnumerable,
                            nameof(StringHelper.PropMacro_IndexedPropertyNotEnumerable), [indexExpr],
                            info.Document);

                        break;
                    }

                    if (!int.TryParse(indexExpr, out int idx))
                    {
                        EmitErrorMessageFormatted(
                            info.Line,
                            info.Column,
                            expr.Length,
                            DS0279_PropMacroIndexNotInteger,
                            nameof(StringHelper.PropMacro_IndexExpressionNotInteger), [indexExpr],
                            info.Document);
                    }

                    try
                    {
                        val = enumerable.Cast<object>().ElementAt(idx);
                    }
                    catch (Exception ex) when (ex is IndexOutOfRangeException or ArgumentOutOfRangeException)
                    {
                        EmitErrorMessageFormatted(
                            info.Line,
                            info.Column,
                            expr.Length,
                            DS0280_PropMacroIndexOutOfRange,
                            nameof(StringHelper.PropMacro_IndexOutOfRange), [indexExpr, enumerable.Cast<object>().Count() - 1],
                            info.Document);
                    }
                }
            }
            else if (val is not string and IEnumerable enumerable)
            {
                val = enumerable.Cast<object>().First();
            }
        }

        if (val is XElement xelem)
            val = xelem.Value;
        else if (val is XAttribute xattrib)
            val = xattrib.Value;

        return Value.formatm(val);
    }

    private static List<Segment> ParseSegments(string expression)
    {
        List<Segment> segments = [];

        foreach (string segment in expression.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] parts = segment.Split(':');
            string member = parts[0];
            List<string> indices = parts[1..].ToList();
            segments.Add(new(member, indices));
        }

        return segments;
    }
}
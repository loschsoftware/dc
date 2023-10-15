using Antlr4.Runtime.Tree;
using LoschScript.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoschScript.Lowering;

internal static class Extensions
{
    public static string ReplaceFirst(this string str, string search, string replace)
    {
        int pos = str.IndexOf(search);
        if (pos < 0)
            return str;

        return str[..pos] + replace + str[(pos + search.Length)..];
    }
}

internal class AccessModifierGroupRewriter : IRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        LoschScriptParser.Access_modifier_member_groupContext rule = (LoschScriptParser.Access_modifier_member_groupContext)tree;

        string modifier = rule.member_access_modifier().GetText();

        string[] identifiers = rule.type_member().Select(t => t.Identifier().GetText()).ToArray();
        string[] members = rule.type_member().Select(listener.GetTextForRule).ToArray();

        List<string> finalMembers = new();

        for (int i = 0; i < members.Length; i++)
            finalMembers.Add(members[i].ReplaceFirst(identifiers[i], $"{modifier} {identifiers[i]}"));

        return string.Join(Environment.NewLine, finalMembers);
    }
}
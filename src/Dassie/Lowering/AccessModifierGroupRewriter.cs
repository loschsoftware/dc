using Antlr4.Runtime.Tree;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dassie.Lowering;

internal class AccessModifierGroupRewriter : ITreeToStringRewriter
{
    public string Rewrite(IParseTree tree, LoweringListener listener)
    {
        DassieParser.Access_modifier_member_groupContext rule = (DassieParser.Access_modifier_member_groupContext)tree;

        string modifier = rule.member_access_modifier().GetText();

        string[] identifiers = rule.type_member().Select(t => t.Identifier().GetText()).ToArray();
        string[] members = rule.type_member().Select(listener.GetTextForRule).ToArray();

        List<string> finalMembers = new();

        for (int i = 0; i < members.Length; i++)
            finalMembers.Add(members[i].ReplaceFirst(identifiers[i], $"{modifier} {identifiers[i]}"));

        return string.Join(Environment.NewLine, finalMembers);
    }
}
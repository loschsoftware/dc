using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;

namespace LoschScript.Lowering;

internal interface IRewriter
{
    public List<Type> TreeTypes { get; }
    public string Rewrite(IParseTree tree);
}
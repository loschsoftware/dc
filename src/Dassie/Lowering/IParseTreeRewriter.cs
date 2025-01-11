using Antlr4.Runtime.Tree;

namespace Dassie.Lowering;

internal interface IParseTreeRewriter<TTree, TOut> : IRewriter<TTree, TOut>
    where TTree : IParseTree
    where TOut : IParseTree
{ }
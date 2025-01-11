namespace Dassie.Lowering;

internal interface IRewriter<TIn, TOut>
{
    public TOut Rewrite(TIn input);
}
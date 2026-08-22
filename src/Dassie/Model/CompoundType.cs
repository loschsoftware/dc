namespace Dassie.Model;

internal record CompoundType : DassieType
{
    public DassieType Left { get; set; }
    public DassieType Right { get; set; }
    public TypeSymbol Symbol { get; set; }

    public override string ToString()
    {
        string symbolStr = Symbol switch
        {
            TypeSymbol.Or => "|",
            TypeSymbol.And => "&",
            _ => "?"
        };

        return $"{Left} {symbolStr} {Right}";
    }
}
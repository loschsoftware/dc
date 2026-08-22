namespace Dassie.Model;

internal record AliasType : DassieType
{
    public AliasType() { }
    public AliasType(DassieType source) : base(source) { }
    
    public DassieType AliasedType { get; set; }

    public override string ToString()
    {
        return base.ToString();
    }
}
namespace Dassie.Model;

internal record NewType : AliasType
{
    public NewType() { }
    public NewType(DassieType source) : base(source) { }
}
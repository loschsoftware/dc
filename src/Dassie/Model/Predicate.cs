namespace Dassie.Model;

internal class Predicate(string expression)
{
    public string Expression => expression;

    public override string ToString()
    {
        return Expression;
    }
}
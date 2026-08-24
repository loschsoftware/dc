using System.Reflection;

namespace Dassie.Model;

internal class DassieField
{
    public static DassieField FromFieldInfo(FieldInfo field)
    {
        return null;
    }

    public string Name { get; private set; }
    public DassieType DeclaringType { get; private set; }
    public DassieType FieldType { get; private set; }
    public bool IsStatic { get; private set; }
    public object DefaultValue { get; private set; }
}
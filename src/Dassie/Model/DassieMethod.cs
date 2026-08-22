using System.Collections.Generic;
using System.Reflection;

namespace Dassie.Model;

internal class DassieMethod
{
    public static DassieMethod FromMethodInfo(MethodInfo method)
    {
        return new()
        {
            Name = method.Name,
            DeclaringType = DassieType.FromType(method.DeclaringType),
            ReturnType = DassieType.FromType(method.ReturnType)
            // TODO: Method parameters
        };
    }

    public string Name { get; private set; }
    public DassieType DeclaringType { get; private set; }
    public DassieType ReturnType { get; private set; }

    // TODO: Varargs, 
    public IEnumerable<DassieType> ParameterTypes { get; private set; }
    // TODO: ParameterizedDassieMethod; type parameters and dependent values on methods
}
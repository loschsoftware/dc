using Dassie.Core;
using Dassie.Model;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assert = Xunit.Assert;

namespace Dassie.Tests.Model;

public class DassieTypeTests
{
    [Fact]
    public static void DassieType_SystemConsole()
    {
        DassieType type = DassieType.FromType(typeof(Console));
        Assert.Equal(TypeKind.Module, type.Kind);
        Assert.Equal("Console", type.Name);
        Assert.Equal("System.Console", type.ToString());
    }

    [Fact]
    public static void DassieType_ListT()
    {
        DassieType type = DassieType.FromType(typeof(List<>));
        Assert.Equal("System.Collections.Generic.List`1[T]", type.ToString());
    }

    [Fact]
    public static void DassieType_ListInt32()
    {
        DassieType type = DassieType.FromType(typeof(List<int>));
        Assert.IsType<ParameterizedDassieType>(type);
        Assert.Equal("System.Collections.Generic.List`1[System.Int32]", type.ToString());
    }

    [Fact]
    public static void DassieType_Int32Constrained()
    {
        DassieType intType = DassieType.FromType(typeof(int));
        ParameterizedDassieType refinedIntType = new(intType)
        {
            Predicates = [new("<=10")]
        };
        
        Assert.Equal("System.Int32[<=10]", refinedIntType.ToString());
    }

    [Fact]
    public static void DassieType_int()
    {
        DassieType type = DassieType.FromType(typeof(@int));
        AliasType aliasType = Assert.IsType<AliasType>(type);
        Assert.Equal("System.Int32", aliasType.AliasedType.FullName);
        Assert.Equal("Dassie.Core.int", type.ToString());
    }

    [Fact]
    public static void DassieType_intOrString()
    {
        DassieType left = DassieType.FromType(typeof(@int));
        DassieType right = DassieType.FromType(typeof(@string));
        CompoundType compound = new()
        {
            Symbol = TypeSymbol.Or,
            Left = left,
            Right = right
        };

        Assert.Equal("Dassie.Core.int | Dassie.Core.string", compound.ToString());
    }

    [Fact]
    public static void DassieType_Modified()
    {
        DassieType baseType = DassieType.FromType(typeof(int));
        ModifiedDassieType modifiedType = new(baseType)
        {
            CustomModifiers =
            [
                new(DassieType.FromType(typeof(IsConst)), false),
                new(DassieType.FromType(typeof(IsVolatile)), true)
            ]
        };

        Assert.Equal("System.Int32 modreq(System.Runtime.CompilerServices.IsConst) modopt(System.Runtime.CompilerServices.IsVolatile)", modifiedType.ToString());
    }
}
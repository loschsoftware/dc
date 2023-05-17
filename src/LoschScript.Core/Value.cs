using System;

namespace LoschScript.Core;

public static class Value
{
    public static object id(object obj) => obj;

    public static void ignore(object obj) { }

    public static void discard(object obj) { }

    public static Type reftype(TypedReference tr) => __reftype(tr);

    public static object refvalue(TypedReference tr) => __refvalue(tr, object);
}
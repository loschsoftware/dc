﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LoschScript.Meta;

internal class MethodContext
{
    public MethodContext()
    {
        CurrentMethod = this;
    }

    public static MethodContext CurrentMethod { get; set; }

    public ILGenerator IL { get; set; }

    public List<string> FilesWhereDefined { get; } = new();

    public MethodBuilder Builder { get; set; }

    public int LocalIndex { get; set; } = -1;

    public List<(string Name, LocalBuilder Builder, bool IsConstant, int Index)> Locals { get; } = new();

    public List<Type> ArgumentTypesForNextMethodCall { get; } = new();
}

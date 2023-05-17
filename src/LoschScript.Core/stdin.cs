using System;

namespace LoschScript.Core;

public static class stdin
{
    public static string scan() => Console.ReadLine();
    public static string scanf(params object[] args) => string.Format(Console.ReadLine(), args);
}
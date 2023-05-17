using System;

namespace LoschScript.Core;

public static class stdout
{
    public static void println(string msg) => Console.WriteLine(msg);
    public static void println(int msg) => Console.WriteLine(msg);
    public static void println(double msg) => Console.WriteLine(msg);
    public static void println(float msg) => Console.WriteLine(msg);
    public static void println(char msg) => Console.WriteLine(msg);
    public static void println(bool msg) => Console.WriteLine(msg);
    public static void println(object msg) => Console.WriteLine(msg);

    public static void print(string msg) => Console.Write(msg);
    public static void print(int msg) => Console.Write(msg);
    public static void print(double msg) => Console.Write(msg);
    public static void print(float msg) => Console.Write(msg);
    public static void print(char msg) => Console.Write(msg);
    public static void print(bool msg) => Console.Write(msg);
    public static void print(object msg) => Console.Write(msg);

    public static void printf(string format, params object[] args) => Console.Write(string.Format(format, args));

    public static void printfn(string format, params object[] args) => Console.WriteLine(string.Format(format, args));
}
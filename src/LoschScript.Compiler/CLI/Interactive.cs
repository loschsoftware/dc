using System;
using System.IO;
using System.Text;

namespace LoschScript.CLI;

internal static class Interactive
{
    public static int StartInteractiveSession(string fileName = "")
    {
        Console.InputEncoding = Encoding.Unicode;
        Console.OutputEncoding = Encoding.Unicode;

        ConsoleColor defaultForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine();
        Console.WriteLine($"LoschScript Interactive{Environment.NewLine}");

        Console.ForegroundColor = defaultForeground;

        Console.WriteLine($"Press Ctrl+C to exit.");
        Console.WriteLine($"Press Shift+Enter to evaluate an expression.{Environment.NewLine}");

        using FileStream fs = new(File.Exists(fileName) ? fileName : DummyFilePath(), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using StreamWriter sw = new(fs);
        sw.AutoFlush = true;

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"˃ ");
            Console.ForegroundColor = defaultForeground;

            string input = ReadInput();

            int length = input.Length;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{Environment.NewLine}{new string('⌄', length)}");
            Console.ForegroundColor = defaultForeground;

            Console.WriteLine(input);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{new string('⌃', length)}{Environment.NewLine}");
            Console.ForegroundColor = defaultForeground;
        }
    }

    private static string ReadInput()
    {
        //StringBuilder sb = new();

        //while (true)
        //{
        //    ConsoleKeyInfo key = Console.ReadKey(false);

        //    if ((key.Modifiers & ConsoleModifiers.Shift) != 0 && key.Key == ConsoleKey.Enter)
        //        break;

        //    sb.Append(key.KeyChar);

        //    if (key.Key == ConsoleKey.Enter)
        //        Console.WriteLine();
        //}

        //return sb.ToString();

        return Console.ReadLine();
    }

    private static string DummyFilePath()
    {
        string dir = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Losch", "LSEdit", "Common", "InteractiveTemp")).FullName;
        return Path.Combine(dir, $"{DateTime.Now.Ticks}");
    }
}
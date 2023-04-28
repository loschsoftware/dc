using System;

namespace LoschScript.CLI;

internal class Program
{
    static int Main(string[] args) => args switch
    {
        _ => DisplayHelpMessage()
    };

    static int DisplayHelpMessage()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine("LoschScript Compiler Command Line (lsc.exe)");
        Console.WriteLine("Command Line Arguments:");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("<FileName> [<FileName>..]".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Compiles the specified source files.");

        Console.WriteLine();
        Console.WriteLine("The following switches are valid for this argument:");
        Console.WriteLine();

        Console.Write("-i".PadRight(25).PadRight(50));
        Console.WriteLine("Interprets the program and doesn't save an assembly to the disk.");
        Console.Write("-ts".PadRight(25).PadRight(50));
        Console.WriteLine("Measures the elapsed build time.");
        Console.Write("-default".PadRight(25).PadRight(50));
        Console.WriteLine("Uses the default configuration and ignores lsconfig.xml files.");
        Console.Write("-out:<FileName>".PadRight(25).PadRight(50));
        Console.WriteLine("Specifies the output assembly name, ignoring lsconfig.xml.");
        Console.Write("-optimize".PadRight(25).PadRight(50));
        Console.WriteLine("Applies IL optimizations to the assembly, ignoring lsconfig.xml.");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-make <Type> <Name>".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Creates the file structure of a LoschScript project.");

        Console.WriteLine();
        Console.WriteLine("Possible values for \"Type\" are:");
        Console.WriteLine();

        Console.Write("console".PadRight(25).PadRight(50));
        Console.WriteLine("Creates a (currently Windows-only) console project.");
        Console.Write("library".PadRight(25).PadRight(50));
        Console.WriteLine("Specifies a library (.dll).");
        Console.Write("script".PadRight(25).PadRight(50));
        Console.WriteLine("A script can be used to run LoschScript code embedded in LS/.NET applications.");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-build".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Compiles all .ls source files in the current directory.");
        //Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-watch, -auto".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Watches all .ls files in the current directory and automatically recompiles when files are changed.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-quit".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Stops all file watchers.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-check <FileName>".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Checks the specified file for syntax errors.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-optimize <FileName>".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Applies IL optimizations to the specified assembly.");
        //Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-merge <OutputFileName> [<FileName>..]".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Merges the specified assemblies into one.");
        //Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-config".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Creates a new lsconfig.xml file with default values.");
        //Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-p, -preprocess <FileName>".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Preprocesses <FileName>.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-interactive".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Provides a read-evaluate-print-loop to run single expressions.");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("-help, -?".PadRight(50));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Shows this page.");

        Console.WriteLine();
        Console.WriteLine("Valid prefixes for options are -, --, and /.");
        return 0;
    }
}
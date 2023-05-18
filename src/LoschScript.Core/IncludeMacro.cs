using System;
using System.IO;

namespace LoschScript.Core;

public class IncludeMacro : IMacro
{
    public string MacroName => "include";

    public string Process(string input)
    {
        if (!File.Exists(input))
            throw new FileNotFoundException("The specified source file does not exist.", input);

        return File.ReadAllText(input);
    }

    public string Process()
    {
        throw new InvalidOperationException("No file to be included specified.");
    }
}
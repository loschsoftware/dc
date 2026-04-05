using Dassie.Configuration;
using Dassie.Core.Commands;
using Dassie.Extensions;
using Dassie.Messages;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dassie.Core.Macros;

internal class EvalMacro : IMacro
{
    private static EvalMacro _instance;
    public static EvalMacro Instance => _instance ??= new();

    public string Name => "Eval";

    private readonly List<MacroParameter> _params = [new("Expression", true)];
    public List<MacroParameter> Parameters => _params;

    public MacroOptions Options => MacroOptions.None;

    private readonly DassieConfig _defaultConfig = new()
    {
        BuildDirectory = ".",
        ApplicationType = "Library",
        AssemblyFileName = "eval",
        IgnoreAllWarnings = true
    };

    // TODO: Make this more efficient!!
    public string Expand(Dictionary<string, string> arguments, MacroInvocationInfo info)
    {
        string expr = arguments["Expression"];
        if (string.IsNullOrEmpty(expr))
            return "";

        string code = $$"""
            import Dassie.Core

            module Eval = {
                GetResult (): string = {
                    value = ({ {{expr}} } <: object)
                    Value.formatm value
                }
            }
            """;

        // TODO: Create this temp file in the .temp directory of the compilation instead
        string prevWorkingDir = Directory.GetCurrentDirectory();
        string tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Dassie", Guid.NewGuid().ToString("N"))).FullName;
        Directory.SetCurrentDirectory(tempDir);

        MessageInfo[] messages = new MessageInfo[EmittedMessages.Count];
        EmittedMessages.CopyTo(messages, 0);
        EmittedMessages.Clear();

        File.WriteAllText("main.ds", code);
        CompileCommand.Instance.Invoke(["main.ds"], _defaultConfig);

        Assembly asm = Assembly.LoadFile(Path.GetFullPath("eval.dll"));
        string result = "";

        try
        {
            result = (string)asm.GetType("Eval").GetMethod("GetResult").Invoke(null, null);
        }
        catch (Exception ex)
        {
            EmitErrorMessageFormatted(
                info.Line,
                info.Column,
                expr.Length,
                DS0274_EvalMacroFailed,
                nameof(StringHelper.EvalMacro_EvaluationFailed), [expr, ex.ToString()],
                info.Document);
        }

        Directory.SetCurrentDirectory(prevWorkingDir);
        //FileSystem.DeleteDirectory(tempDir, DeleteDirectoryOption.DeleteAllContents);

        foreach (MessageInfo msg in messages)
            EmittedMessages.Add(msg);

        return result;
    }
}
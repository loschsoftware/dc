using Dassie.Configuration;
using Dassie.Core.Commands;
using Dassie.Extensions;
using Dassie.Messages;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Reflection;

namespace Dassie.Core.Actions;

internal class CodeBuildAction : IBuildAction
{
    public string Name => "Code";
    public ActionModes SupportedModes => ActionModes.All;

    private readonly DassieConfig _defaultConfig = new()
    {
        BuildDirectory = ".",
        ApplicationType = "Library",
        AssemblyFileName = "eval",
        IgnoreAllWarnings = true
    };

    public int Execute(ActionContext context)
    {
        string code = context.Text;

        string prevWorkingDir = Directory.GetCurrentDirectory();
        string tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Dassie", Guid.NewGuid().ToString("N"))).FullName;
        Directory.SetCurrentDirectory(tempDir);

        MessageInfo[] messages = new MessageInfo[EmittedMessages.Count];
        EmittedMessages.CopyTo(messages, 0);
        EmittedMessages.Clear();

        File.WriteAllText("main.ds", code);
        CompileCommand.Instance.Invoke(["main.ds"], _defaultConfig);

        byte[] asmBytes = File.ReadAllBytes(Path.GetFullPath("eval.dll"));
        Assembly asm = Assembly.Load(asmBytes);
        asm.GetType("Program").GetMethod("Main").Invoke(null, [Array.Empty<string>()]);

        Directory.SetCurrentDirectory(prevWorkingDir);
        FileSystem.DeleteDirectory(tempDir, DeleteDirectoryOption.DeleteAllContents);

        foreach (MessageInfo msg in messages)
            EmittedMessages.Add(msg);

        return 0;
    }
}
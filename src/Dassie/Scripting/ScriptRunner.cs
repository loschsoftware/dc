using Dassie.Configuration;
using Dassie.Core.Commands;
using Dassie.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Dassie.Scripting;

internal static class ScriptRunner
{
    private static readonly DassieConfig _defaultConfig = new()
    {
        BuildDirectory = ".",
        ApplicationType = "Library",
        AssemblyFileName = "eval",
        IgnoreAllWarnings = true
    };

    private static int ExecuteAssembly(string path, string[] args)
    {
        byte[] asmBytes = File.ReadAllBytes(path);
        Assembly asm = Assembly.Load(asmBytes);
        asm.GetType("Program").GetMethod("Main").Invoke(null, [args ?? []]);
        return 0;
    }

    public static int Execute(string source)
        => Execute(source, null);

    public static int Execute(string source, string[] args)
    {
        string hash = string.Join("", SHA256.HashData(Encoding.UTF8.GetBytes(source)).Select(b => b.ToString("x2")));

        string scriptCacheDirPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Dassie", "Scripts")).FullName;
        string scriptCacheFilePath = Path.Combine(scriptCacheDirPath, "scripts.dat");

        Dictionary<string, string> cache = [];

        if (File.Exists(scriptCacheFilePath))
        {
            string[] lines = File.ReadAllLines(scriptCacheFilePath);
            
            foreach (string line in lines)
            {
                if (!line.Contains(':'))
                    continue;

                string[] parts = line.Split(':');
                cache.Add(parts[0], parts[1]);

                if (parts[0] != hash)
                    continue;

                string dir = Path.Combine(scriptCacheDirPath, parts[1]);
                if (!Directory.Exists(dir))
                    continue;

                string scriptFile = Path.Combine(dir, "eval.dll");
                if (!File.Exists(scriptFile))
                    continue;

                return ExecuteAssembly(scriptFile, args);
            }
        }

        string cacheDirGuid = Guid.NewGuid().ToString();
        string prevWorkingDir = Directory.GetCurrentDirectory();
        string scriptDir = Directory.CreateDirectory(Path.Combine(scriptCacheDirPath, cacheDirGuid)).FullName;
        Directory.SetCurrentDirectory(scriptDir);

        MessageInfo[] messages = new MessageInfo[EmittedMessages.Count];
        EmittedMessages.CopyTo(messages, 0);
        EmittedMessages.Clear();

        File.WriteAllText("main.ds", source);
        CompileCommand.Instance.Invoke(["main.ds"], _defaultConfig);
        ExecuteAssembly(Path.GetFullPath("eval.dll"), args);
        Directory.SetCurrentDirectory(prevWorkingDir);

        foreach (MessageInfo msg in messages)
            EmittedMessages.Add(msg);

        cache.Add(hash, cacheDirGuid);

        StringBuilder sb = new();
        foreach ((string k, string v) in cache)
        {
            sb.AppendLine($"{k}:{v}");
        }

        File.WriteAllText(scriptCacheFilePath, sb.ToString());
        return 0;
    }
}
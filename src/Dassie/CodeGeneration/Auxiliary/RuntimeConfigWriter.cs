using System;
using System.Runtime.InteropServices;

namespace Dassie.CodeGeneration.Auxiliary;

internal static class RuntimeConfigWriter
{
    public static void GenerateRuntimeConfigFile(string path)
    {
        Version version = typeof(object).Assembly.GetName().Version;
        string target = version.ToString(2);
        
        File.WriteAllText(path, $@"{{
    ""runtimeOptions"": {{
        ""tfm"": ""net{target}"",
        ""framework"": {{
            ""name"": ""Microsoft.NETCore.App"",
            ""version"": ""{RuntimeInformation.FrameworkDescription[5..]}""
        }}
    }}
}}
");
    }
}
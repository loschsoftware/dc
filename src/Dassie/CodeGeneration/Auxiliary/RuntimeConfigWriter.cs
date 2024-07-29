using System;
using System.Runtime.InteropServices;

namespace Dassie.CodeGeneration.Auxiliary;

/// <summary>
/// Provides tools for generating .runtimeconfig.json files, which are needed to execute .NET Core applications.
/// </summary>
internal static class RuntimeConfigWriter
{
    /// <summary>
    /// Generates a .runtimeconfig.json file for the specified assembly using the .NET version of the compiler.
    /// </summary>
    /// <param name="path">The path to an assembly to generate the runtimeconfig file for.</param>
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
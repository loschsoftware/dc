using System;

namespace Dassie;

internal static class Constants
{
    public const string CompilerExecutableName = "dc";

    public const string ApplicationDataDirectoryName = "Dassie";
    public const string ProjectConfigurationFileName = "dsconfig.xml";
    public const string GlobalConfigurationFileName = "config.xml";
    public const string TemporaryBuildDirectoryName = ".temp";
    public const string AotBuildDirectoryName = "aot";
    public const string ILFilesDirectoryName = "cil";
    public const string SdkDirectoryName = "Sdk";
    public const string CoreSdkName = "Core";
    public const string CoreSdkFileName = "Core.def";
    
    public static string ApplicationDataDirectoryPath => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataDirectoryName));
}
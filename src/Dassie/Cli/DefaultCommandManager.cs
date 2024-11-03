using Dassie.Cli.Commands;
using Dassie.Extensions;
using Dassie.Templates;
using System.Collections.Generic;

namespace Dassie.Cli;

internal static class DefaultCommandManager
{
    public static List<ICompilerCommand> DefaultCommands { get; } = [
        new BuildCommand(),
        new ConfigCommand(),
        new QuitCommand(),
        new RunCommand(),
        new ScratchpadCommand(),
        new ExtensionManagerCommandLine(),
        new WatchCommand(),
        new NewCommand(),
        new WatchIndefinetlyCommand()
    ];

    public static ICompilerCommand CompileAllCommand => DefaultCommands[0];
    public static ICompilerCommand ConfigCommand => DefaultCommands[1];
    public static ICompilerCommand QuitCommand => DefaultCommands[2];
    public static ICompilerCommand RunCommand => DefaultCommands[3];
    public static ICompilerCommand ScratchpadCommand => DefaultCommands[4];
    public static ICompilerCommand PackageCommand => DefaultCommands[5];
    public static ICompilerCommand WatchCommand => DefaultCommands[6];
    public static ICompilerCommand NewCommand => DefaultCommands[7];
}
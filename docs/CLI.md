# Dassie Compiler Command Line Arguments

Usage: ``dc [Command] [Options]`` or ``dc <FileName> [FileNames]``

> [!NOTE]  
> Options encased in brackets ([ ]) are **optional**.

|Command|Arguments|Description|Example|
|---|---|---|---|
||``<FileName> [FileNames]``|Compiles the specified source files.|``dc a.ds b.ds``|
|``new``|``<Type> <Name>``|Creates the folder structure for a Dassie application. ``Type`` can either be ``console`` or ``library``.|``dc new console MyApplication``|
|``build``|``[BuildProfile]``|Executes the specified build profile, or compiles all .ds source files in the current directory if none is specified.|``dc build``|
|``watch``, ``auto``||Watches all .ds source files in the current folder structure for changes and automatically recompiles when files are changed.|``dc watch``|
|``quit``||Stops all file watchers.|``dc quit``|
|``scratchpad``|``[Command] [Options]``|Opens or manages *scratches*, which allow compiling and running source code from the console. For more information regarding this command, use ``dc scratchpad help``.|``dc scratchpad``|
|``check``, ``verify``|``[FileNames]``|Checks all specified source files, or all source files in the current folder structure, for syntax errors.|``dc check test.ds``|
|``config``||Creates a new ``dsconfig.xml`` file with default values.|``dc config``|
|``package``|``[Command] [Options]``|Used to manage compiler extensions. See [this page](./Extensions.md) for more information.|``dc package list``|
|``help``, ``?``||Shows this list of commands.|``dc help``|

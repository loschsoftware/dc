# Scratchpad

The Dassie Compiler includes a **scratchpad** feature that allows you to quickly write, compile, and execute Dassie code without creating a full project. This is useful for testing ideas, learning the language, or prototyping.

## Overview

A *scratch* is a temporary code snippet that is stored in a dedicated directory. Scratches can be created, listed, loaded, and deleted using the `dc scratchpad` command.

## Creating a Scratch

To create and open a new scratch, simply run:

```bash
dc scratchpad
```

Or explicitly:

```bash
dc scratchpad new
```

### Default Editor Mode

By default, the scratchpad uses console input. You'll see a prompt where you can type your Dassie code directly:

```
Dassie Compiler for .NET 10
Version X.X, Build XXX (XX/XX/XXXX)

To mark the end of the input, press Ctrl+Z in an empty line and hit Enter.

import System
Console.WriteLine "Hello from scratchpad!"
^Z
```

After pressing Ctrl+Z (on Windows) or Ctrl+D (on Unix), the code will be compiled and executed.

### Using a Custom Editor

You can configure the scratchpad to use your preferred text editor. Set the global configuration property:

```bash
dc config --global core.scratchpad.editor=code    # Visual Studio Code
dc config --global core.scratchpad.editor=vim     # Vim
dc config --global core.scratchpad.editor=notepad # Notepad
```

When a custom editor is configured, running `dc scratchpad` will open the editor with a temporary file. After you save and close the file, the code will be compiled and executed.

## Naming Scratches

By default, scratches are named sequentially (`scratch000`, `scratch001`, etc.). You can specify a custom name:

```bash
dc scratchpad new --name=my-experiment
```

## Managing Scratches

### Listing Saved Scratches

To see all saved scratches:

```bash
dc scratchpad list
```

Example output:

```
Saved scratches:

Last modified           Name
03/15/2024 10:30:00    scratch000
03/15/2024 11:45:00    scratch001
03/16/2024 09:15:00    my-experiment
```

### Loading a Scratch

To continue working on an existing scratch:

```bash
dc scratchpad load my-experiment
```

> [!NOTE]
> The `load` command requires a custom editor to be configured. It is not available when using the default console input mode.

### Deleting a Scratch

To delete a specific scratch:

```bash
dc scratchpad delete my-experiment
```

### Clearing All Scratches

To delete all saved scratches:

```bash
dc scratchpad clear
```

## Using a Configuration File

If your scratch needs specific compiler settings (such as references to external libraries), you can provide a configuration file:

```bash
dc scratchpad new --config=./my-config.xml
```

The configuration file will be copied to the scratch directory and used during compilation.

## Piping Input

You can pipe source code directly to the scratchpad:

```bash
echo 'println "Hello!"' | dc scratchpad
```

Or from a file:

```bash
cat my-code.ds | dc scratchpad
```

This is useful for quick one-liners or scripting scenarios.

## Storage Location

Scratches are stored in the system's temporary directory:

- **Windows**: `%LOCALAPPDATA%\Temp\Dassie\Scratchpad\`
- **Linux/macOS**: `/tmp/Dassie/Scratchpad/` (or `$TMPDIR/Dassie/Scratchpad/`)

Each scratch has its own subdirectory containing:
- `scratch.ds` - The source file
- `dsconfig.xml` - Configuration file (if provided)
- Build output files

## Tips

1. **Quick Testing**: Use scratchpad to quickly test language features or API calls before incorporating them into a larger project.

2. **Learning**: The scratchpad is great for following along with tutorials or experimenting with new concepts.

3. **Prototyping**: Sketch out algorithms or logic before implementing them in a full project.

4. **No Cleanup Needed**: Since scratches are stored in the temp directory, they won't clutter your working directories.

## See Also

- [Command-Line Reference](./cli.md) - Full reference for the `dc scratchpad` command
- [Getting Started](./getting-started.md) - Introduction to the Dassie Compiler

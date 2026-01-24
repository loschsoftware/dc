# Dassie Compiler Command Line Reference

The Dassie Compiler (`dc`) provides a comprehensive set of commands for building, running, testing, and managing Dassie projects.

## Basic Usage

```
dc [Command] [Options]
dc <FileName> [FileNames]
```

> [!NOTE]
> Options enclosed in brackets ([ ]) are **optional**.

## Quick Reference

| Command | Description |
|---------|-------------|
| `dc build` | Build the current project |
| `dc run` | Build and run the current project |
| `dc new` | Create a new project from a template |
| `dc test` | Run unit tests |
| `dc watch` | Auto-rebuild on file changes |
| `dc help` | Display help information |

## Commands

### build

Executes the specified build profile, or compiles all source files in the current folder structure if none is specified.

```
dc build [BuildProfile] [Options]
```

This is the primary command for building Dassie projects. By default, this command will compile all Dassie source files in the current directory as well as all subdirectories. If no project file is present in the root directory, the default configuration is used.

**Arguments:**

| Argument | Description |
|----------|-------------|
| `BuildProfile` | Specifies the build profile to execute. If not set, the default profile is executed. |
| `Options` | Additional options to pass to the compiler. For a list of available options, use `dc help -o`. |

**Examples:**

```bash
dc build                              # Build with default profile
dc build CustomProfile                # Build with 'CustomProfile' build profile
dc build CustomProfile -r Aot         # Build with AOT compilation
```

---

### run

Compiles a project or project group and then runs the output executable with the specified arguments.

```
dc run [Arguments]
dc run -p|--profile=<Profile> -- [Arguments]
```

This command requires the presence of a project or project group. If it is executed on a project group, the project that is executed is determined by the `<Executable>` property in the project group definition.

This command only recompiles the project if the source files have been updated since the last compilation or the output files have been deleted. Otherwise, the executable is launched immediately.

**Arguments:**

| Argument | Description |
|----------|-------------|
| `Arguments` | Command-line arguments passed to the program that is executed. |
| `-p\|--profile=<Profile>` | The build profile to use for compilation. If not specified, the default profile is used. |

**Examples:**

```bash
dc run                        # Build and run without arguments
dc run arg1 arg2              # Build and run with arguments
dc run -p=CustomProfile       # Build with specific profile and run
```

---

### new

Creates the file structure of a Dassie project.

```
dc new <Template> <Name> [-f|--force]
dc new --list-templates
```

This command creates a new directory with the specified project name and creates the file structure of the specified project template inside. Project templates are installed as part of compiler extensions (packages). Managing compiler extensions is facilitated through the `dc package` command. The Dassie compiler includes the templates `Console` and `Library` by default.

**Arguments:**

| Argument | Description |
|----------|-------------|
| `Template` | The template to use for the new project. |
| `Name` | The name of the project. |
| `-f\|--force` | If enabled, directories colliding with the path of the new project will be deleted. |
| `--list-templates` | Lists all installed project templates. |

**Examples:**

```bash
dc new console MyApp                    # Create a new console application
dc new library MyLibrary                # Create a new library project
dc new custom-template MyProject -f     # Create from custom template, overwriting existing
dc new --list-templates                 # List all available templates
```

---

### test

Runs unit tests defined for the current project or project group.

```
dc test [(-a|--assembly)=<Assembly>] [(-m|--module)=<Module>] [--failed]
```

If ran on a project, this command first compiles the project and then collects and runs all unit tests defined in the project or specified module. A 'test module' is a module decorated with the `<TestModule>` attribute from the Dassie unit test library (Dassie.Tests). A test is a method of a test module decorated with the `<Test>` attribute.

**Arguments:**

| Argument | Description |
|----------|-------------|
| `-a\|--assembly` | Run tests from the specified assembly. |
| `-m\|--module` | Run tests from the specified test module. Multiple modules can be specified by using the option multiple times. |
| `--failed` | Only display failed tests. |

**Examples:**

```bash
dc test                                    # Run all tests in current project
dc test --failed                           # Run tests, show only failures
dc test -m=MyNamespace.MyTestModule        # Run tests from specific module
dc test -a=./path/to/assembly.dll          # Run tests from specific assembly
```

---

### watch

Watches all .ds files in the current folder structure and automatically recompiles when files are changed.

```
dc watch
dc watch -c|--command <Command>
dc watch -p|--profile <Profile>
dc watch <Directory>
dc watch --quit
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `-c\|--command <Command>` | Specifies the compiler command that is executed when files are changed. The default value is `build`. |
| `-p\|--profile <Profile>` | Specifies the build profile that is used when files are changed. If this option is set, the `--command` option cannot be used. |
| `<Directory>` | Specifies the directory that is watched for changed source files. Cannot be combined with the `--command` and `--profile` options. |
| `--quit` | Stops all currently running watchers. |

**Examples:**

```bash
dc watch                  # Rebuild whenever any .ds file changes
dc watch -c run           # Re-run the application automatically on save
dc watch -p Release       # Watch and rebuild using the Release profile
dc watch ./src            # Monitor the ./src folder only
dc watch --quit           # Stop all running watchers
```

---

### analyze

Runs code analyzers on the current project or on a list of source files.

```
dc analyze [(--analyzer|-a)=<Name>]
dc analyze <Files> [(--analyzer|-a)=<Name>]
dc analyze --markers [--marker:<Marker>] [--exclude:<Marker>] [Files]
```

A code analyzer is a tool that examines source code for potential issues and style violations. Code analyzers other than the default one are installed as part of compiler extensions (packages).

The `--markers` option provides a simple way to scan for code comments with marker symbols such as `TODO`, `NOTE` or `FIXME`. It searches through the current project or specified files and displays all according comments in a structured list.

**Arguments:**

| Argument | Description |
|----------|-------------|
| `(--analyzer \| -a)=<Name>` | The name of the code analyzer to run. If none is specified, the default analyzer is used. |
| `Files` | A list of source files to analyze. If this option is not used, all source files in the current project will be analyzed. |
| `--markers [Options] [Files]` | Extracts and displays all comments containing markers such as TODO from the current project or the specified source files. |
| `--marker:<Marker>` | Specifies a custom marker to include in the search. Multiple can be specified. |
| `--exclude:<Marker>` | Specifies a marker to ignore in the search. Multiple can be specified. |

**Examples:**

```bash
dc analyze                                        # Run default analyzer on project
dc analyze --analyzer=CustomAnalyzer              # Run custom analyzer
dc analyze ./src/File1.ds ./src/File2.ds          # Analyze specific files
dc analyze --markers                              # Find TODO/FIXME comments
dc analyze --markers --marker:HACK --exclude:NOTE # Custom markers
```

---

### deploy

Builds and deploys a project group.

```
dc deploy [--ignore-missing] [--fail-fast] [Options]
```

This is the primary command for interacting with project groups. The `deploy` command first builds all component projects and then executes all targets defined in the project group file. A project group is defined using the `<ProjectGroup>` tag inside of a compiler configuration file (`dsconfig.xml`).

**Arguments:**

| Argument | Description |
|----------|-------------|
| `--ignore-missing` | Ignore missing targets and resume deployment. |
| `--fail-fast` | Cancel deployment immediately if any target fails. |
| `Options` | Additional options passed to the compiler for each project being built. |

**Examples:**

```bash
dc deploy                    # Build and deploy project group
dc deploy --ignore-missing   # Deploy, ignoring missing targets
dc deploy --fail-fast        # Stop on first failure
dc deploy -l                 # Pass '-l' flag to each project build
```

---

### scratchpad

Opens or manages *scratches*, which allow compiling and running source code from the console.

```
dc scratchpad [Command] [Options]
```

Scratches are temporary code snippets that can be quickly compiled and executed without creating a full project.

**Subcommands:**

| Command | Description |
|---------|-------------|
| `new` | Create and open a new scratch (default if no command specified) |
| `list` | List all saved scratches |
| `load <Name>` | Load an existing scratch |
| `delete <Name>` | Delete a specific scratch |
| `clear` | Delete all saved scratches |

**Examples:**

```bash
dc scratchpad              # Open a new scratch
dc scratchpad list         # List saved scratches
dc scratchpad load test    # Load scratch named 'test'
dc scratchpad delete test  # Delete scratch named 'test'
dc scratchpad clear        # Delete all scratches
```

For more information, see [Scratchpad](./scratchpad.md).

---

### clean

Clears build artifacts and temporary files of a project or project group.

```
dc clean
```

This command only works when executed at the root level of a project or project group. It deletes all output and temporary files generated by the compiler.

> [!WARNING]
> This command will delete all contents of the build directory, even those created by the user!

**Examples:**

```bash
dc clean    # Clean build artifacts
```

---

### config

Manages compiler settings and project configurations.

```
dc config [<Property>=[Value]]...
dc config --global [--reset] [--import <Path>] [<Property>=[Value]]...
```

This command is used to display or change global or project-specific compiler settings. If this command is called without arguments in a directory containing a project file, it will display the current project configuration. Similarly, the `--global` flag is used to change or show the global configuration.

If `dc config` is called in a directory not containing a project file, a new project file will be created.

**Arguments:**

| Argument | Description |
|----------|-------------|
| `Property=[Value]` | The property to modify. Multiple can be specified, separated by spaces. The value is optional; if omitted, the default value is used. Note that the equals sign (=) is still required. |
| `--global` | Indicates that the operation displays or modifies the global configuration, as opposed to a project file. |
| `--reset` | Resets all global properties to their default value. |
| `--import <Path>` | Imports the global configuration from the specified file. |

**Examples:**

```bash
dc config                                        # Show project config or create new
dc config MeasureElapsedTime=true Verbosity=2    # Change project settings
dc config --global                               # Show global configuration
dc config --global core.scratchpad.editor=vim    # Change global setting
```

---

### package

Manages compiler extensions.

```
dc package [Command] [Options]
```

Used to install and manage compiler extensions. For more information, see [Compiler Extensions](./extensions.md).

**Subcommands:**

| Command | Description |
|---------|-------------|
| `list` | Displays a list of all installed extensions |
| `info <Name>` | Displays advanced information about the specified extension |
| `install <Name> [-g]` | Installs the specified extension from the package repository. Use `-g` for global tool installation. |
| `import <Path> [-o] [-g]` | Installs an extension from the specified file path. Use `-o` to overwrite existing. |
| `remove <Name>` | Uninstalls the specified extension package |
| `update <Name>` | Updates the specified extension to the newest version |

**Examples:**

```bash
dc package list                      # List installed extensions
dc package info MyExtension          # Show extension details
dc package install MyExtension       # Install from repository
dc package import ./extension.dll    # Install from file
dc package remove MyExtension        # Uninstall extension
```

---

### help

Lists all available commands and shows help for specific commands or compiler features.

```
dc help
dc help <Command>
dc help <(--options | --simple | --no-external | --commands)>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `Command` | The name of a compiler command to show help for. |
| `-o\|--options` | Shows a list of all available project file properties. |
| `-s\|--simple` | Shows a simplified selection of commands suitable for minimalist developers. |
| `--commands` | Prints a comma-separated list of available commands. |
| `--no-external` | Does not display commands defined by external packages. |

**Examples:**

```bash
dc help                  # Show all commands
dc help build            # Show help for 'build' command
dc help --options        # Show all project file properties
dc help --no-external    # Show only built-in commands
```

**Aliases:** `?`, `-h`, `-help`, `--help`, `-?`, `/?`, `/help`

---

### Direct File Compilation

You can also compile source files directly without using a command:

```
dc <FileName> [FileNames]
```

**Examples:**

```bash
dc main.ds               # Compile a single file
dc a.ds b.ds c.ds        # Compile multiple files
```

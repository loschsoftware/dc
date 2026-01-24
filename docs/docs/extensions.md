# Compiler Extensions

The Dassie compiler provides a rich API for extending its functionality with custom plug-ins, which are called **extensions**. This document provides an overview of the capabilities of the Extension API as well as guidance on implementing custom extensions.

## Overview

Extensions can add various features to the compiler:
- Custom commands (`dc mycommand`)
- Project templates for `dc new`
- Code analyzers
- Build log writers
- Compiler directives
- Deployment targets
- And more

## Format

An **extension** is a .NET type implementing the [``IPackage``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IPackage.cs) interface. An **extension package** is a .NET assembly containing one or more extensions. For convenience, the API also provides the abstract base class [``Extension``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/Extension.cs) which implements ``IPackage``.

## Extension Lifecycle

Compiler extensions can be loaded in one of two modes:

### Global Mode
The extension is enabled globally and is initialized as soon as the compiler is launched. It is active for every build. Managing global extensions is done through the ``dc package`` command.

### Transient Mode
The extension is enabled only for a single build process. Managing transient extensions is done through the [``<Extensions>``](#transient-extensions) tag in a project file.

### Lifecycle Methods

``IPackage`` contains virtual methods for custom initialization and cleanup:

| Method | Description |
|--------|-------------|
| ``InitializeGlobal()`` | Called when the extension is loaded in global mode |
| ``InitializeTransient()`` | Called when the extension is loaded in transient mode |
| ``Unload()`` | Called when the extension is unloaded (both modes) |

> [!CAUTION]
> Do not rely on ``IDisposable`` for freeing resources of extensions, as the Dassie compiler does **not** invoke the ``Dispose`` method when they are unloaded. Always call ``Dispose`` in ``Unload`` to ensure unmanaged resources are cleaned up properly.

## Installing Extensions

### From a Local File

Use the ``dc package import`` command:

```bash
dc package import ./path/to/extension.dll
```

Options:
- ``-o``: Overwrite if already installed
- ``-g``: Install as a global tool (adds to PATH)

### From the Package Repository

Use the ``dc package install`` command:

```bash
dc package install MyExtension
```

> [!NOTE]
> The online extension registry is not yet available. Currently, only local imports are supported.

### Managing Extensions

```bash
dc package list              # List installed extensions
dc package info MyExtension  # Show extension details
dc package remove MyExtension # Uninstall extension
dc package update MyExtension # Update to latest version
```

## Extension API Features

All features of the API are supported through interfaces in the ``Dassie.Extensions`` namespace. The following interfaces are available:

| Interface | Description |
|-----------|-------------|
| [``ICompilerCommand``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/ICompilerCommand.cs) | Defines a custom compiler command |
| [``IProjectTemplate``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IProjectTemplate.cs) | Defines a project template for ``dc new`` |
| [``IAnalyzer``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IAnalyzer.cs) | Defines a code analyzer for ``dc analyze`` |
| [``IBuildLogWriter``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IBuildLogWriter.cs) | Redirects build messages to a custom destination |
| [``IBuildLogDevice``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IBuildLogDevice.cs) | Custom serialization for build messages |
| [``ICompilerDirective``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/ICompilerDirective.cs) | Defines a custom compiler directive |
| [``IDocumentSource``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IDocumentSource.cs) | Injects source code into compilation |
| [``IDeploymentTarget``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IDeploymentTarget.cs) | Defines a deployment target for ``dc deploy`` |
| [``ISubsystem``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/ISubsystem.cs) | Defines application type characteristics |
| [``IConfigurationProvider``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IConfigurationProvider.cs) | Provides configuration templates for project files |
| [``GlobalConfigProperty``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/GlobalConfigProperty.cs) | Defines a global configuration property |

## Creating a Custom Extension

The following example implements a complete compiler extension that adds an "echo" command.

### Step 1: Create a Class Library Project

Create a new .NET class library project and add a reference to `dc.dll`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="dc">
      <HintPath>path/to/dc.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

### Step 2: Create the Extension Package Class

Create a class implementing ``IPackage``:

```csharp
using Dassie.Extensions;
using System;

namespace EchoExtension;

public class EchoExtensionPackage : IPackage
{
    public PackageMetadata Metadata => new()
    {
        Name = "EchoExtension",
        Author = "Your Name",
        Version = new(1, 0, 0, 0),
        Description = "A demo extension that echoes text to the console."
    };

    public Type[] Commands => [typeof(EchoCommand)];
    
    // Optional: Override lifecycle methods
    public override void InitializeGlobal()
    {
        Console.WriteLine("EchoExtension loaded globally!");
    }
}
```

### Step 3: Create the Command Class

Implement the ``ICompilerCommand`` interface (or extend ``CompilerCommand``):

```csharp
using Dassie.Extensions;
using System;
using System.Collections.Generic;

namespace EchoExtension;

public class EchoCommand : CompilerCommand
{
    public override string Command => "echo";
    
    public override string Description => "Echoes the specified text to the console.";
    
    public override List<string> Aliases => ["print", "say"];
    
    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage = ["dc echo <Text>", "dc echo -n <Text>"],
        Options =
        [
            ("Text", "The text to echo."),
            ("-n", "Do not print a trailing newline.")
        ],
        Examples =
        [
            ("dc echo \"Hello, World!\"", "Prints 'Hello, World!' to the console."),
            ("dc echo -n \"No newline\"", "Prints without a trailing newline.")
        ]
    };
    
    public override int Invoke(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine("Usage: dc echo <Text>");
            return 1;
        }
        
        bool noNewline = args[0] == "-n";
        string text = string.Join(" ", noNewline ? args[1..] : args);
        
        if (noNewline)
            Console.Write(text);
        else
            Console.WriteLine(text);
        
        return 0;
    }
}
```

### Step 4: Build and Install

Build the project and install the extension:

```bash
dotnet build
dc package import ./bin/Debug/net10.0/EchoExtension.dll
```

### Step 5: Test the Extension

```bash
dc echo "Hello from my extension!"
dc help echo
```

## Transient Extensions

Projects can declare dependencies on extensions that are only active during compilation:

```xml
<Extensions>
  <Extension Path="./tools/MyExtension.dll"/>
  <Extension Path="./tools/AnotherExtension.dll" CustomOption="value"/>
</Extensions>
```

Extensions can receive configuration through XML attributes and child elements, which are accessible via the extension's initialization methods.

> [!IMPORTANT]
> Not every extension supports transient mode. Extension developers can customize which loading modes are supported through the ``IPackage.SupportedModes`` property.

## Advanced Topics

### Custom Project Templates

Implement ``IProjectTemplate`` to add templates for ``dc new``:

```csharp
public class MyTemplate : IProjectTemplate
{
    public string Name => "mytemplate";
    public string Description => "My custom project template";
    
    public void Create(string projectName, string path, string[] args)
    {
        // Create project structure
        Directory.CreateDirectory(path);
        File.WriteAllText(
            Path.Combine(path, "dsconfig.xml"),
            GenerateProjectFile(projectName));
        // ...
    }
}
```

### Custom Code Analyzers

Implement ``IAnalyzer`` to add analyzers for ``dc analyze``:

```csharp
public class MyAnalyzer : IAnalyzer
{
    public string Name => "MyAnalyzer";
    
    public AnalysisResult Analyze(string sourceFile, DassieConfig config)
    {
        // Analyze the source file
        // Return results
    }
}
```

### Custom Deployment Targets

Implement ``IDeploymentTarget`` for ``dc deploy``:

```csharp
public class MyDeployTarget : IDeploymentTarget
{
    public string Name => "MyTarget";
    
    public int Execute(DeploymentContext context)
    {
        // Perform deployment actions
        return 0; // Success
    }
}
```

## Best Practices

1. **Use descriptive metadata**: Provide clear name, description, and version information.

2. **Handle errors gracefully**: Return appropriate exit codes and display helpful error messages.

3. **Support help text**: Implement ``HelpDetails`` for all commands so users can use ``dc help <command>``.

4. **Clean up resources**: Always implement ``Unload()`` to release resources.

5. **Document your extension**: Provide README and examples for users.

6. **Version carefully**: Follow semantic versioning for compatibility.

## See Also

- [Command-Line Reference](./cli.md) - ``dc package`` command
- [Project Files](./projects.md) - Transient extension configuration
- [Contributing: Extension API](./contributing/extensions.md) - Adding new features to the Extension API

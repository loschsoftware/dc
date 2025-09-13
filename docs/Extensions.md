# Dassie Compiler Extensions
The Dassie compiler provides a rich API for extending its functionality with custom plug-ins, which are called **extensions**. This document intends to give an overview of the capabilities of the Extension API as well as what to look out for when implementing custom extensions.

|**Table of contents**|
|---|
|[Format](./Extensions.md#format)|
|[Extension lifecycle](./Extensions.md#extension-lifecycle)|
|[Installing compiler extensions](./Extensions.md#installing-compiler-extensions)|
|[Extension API features](./Extensions.md#extension-api-features)|
|[Creating a custom extension](./Extensions.md#creating-a-custom-extension)|

## Format
An **extension** is a .NET type implementing the [``Dassie.Extensions.IPackage``](../src/Dassie/Extensions/IPackage.cs) interface. An **extension package** is a .NET assembly containing one or more extensions. For an improved developer experience, the API also provides the abstract base class [``Extension``](../src/Dassie/Extensions/Extension.cs) which implements ``IPackage``.

## Extension lifecycle
Compiler extensions can be loaded in one of two modes:
- **Global:** The extension is enabled globally and is initialized as soon as the compiler is launched. It is active for every build. Managing global extensions is done through the ``dc package`` command.
- **Transient:** The extension is enabled only for a single build process. Managing transient extensions is done through the [``<Extensions>``](./Projects.md#transient-extensions) tag in a project file.

``IPackage`` contains the virtual methods [``InitializeGlobal``](../src/Dassie/Extensions/IPackage.cs#L34) and [``InitializeTransient``](../src/Dassie/Extensions/IPackage.cs#L43) that can be overridden to implement custom behavior when the extension is loaded. Similarly, the [``Unload``](../src/Dassie/Extensions/IPackage.cs#L48) method provides a way of performing an action when the extension is unloaded, regardless of loading mode.

> [!CAUTION]
> Do not rely on ``IDisposable`` for freeing resources of extensions, as the Dassie compiler does **not** invoke the ``Dispose`` method when they are unloaded. Always call ``Dispose`` in ``Unload`` to ensure unmanaged resources are cleaned up properly.

## Installing compiler extensions
The command ``dc package`` is used to manage extensions in global mode. To install extensions from an extension package stored on disk, use the ``dc package import`` subcommand. To install them from the online extension registry, use the ``dc package install`` command. This command is not supported yet.

## Extension API features
All features of the API are supported through interfaces in the [``Dassie.Extensions``](../src/Dassie/Extensions) namespace. All features of an extension are centrally registered in ``IPackage`` through its various virtual methods. The following is a list of all available interfaces:
- **[``ICompilerCommand``](../src/Dassie/Extensions/ICompilerCommand.cs):** Defines a custom compiler command to integrate external tools with the compiler.
- **[``IProjectTemplate``](../src/Dassie/Extensions/IProjectTemplate.cs):** Defines a custom project template used in combination with the ``dc new`` command.
- **[``IConfigurationProvider``](../src/Dassie/Extensions/IConfigurationProvider.cs):** Defines a configuration provider that serves as a template for a project file. This is used in combination with the [``Import``](./Projects.md#importing-project-files) attribute of project files.
- **[``IAnalyzer``](../src/Dassie/Extensions/IAnalyzer.cs):** Defines a custom code analyzer used in combination with the ``dc analyze`` command.
- **[``IBuildLogWriter``](../src/Dassie/Extensions/IBuildLogWriter.cs):** Allows the redirection of build message to an arbitrary ``TextWriter`` object. Requires the usage of the default build log device.
- **[``IBuildLogDevice``](../src/Dassie/Extensions/IBuildLogDevice.cs):** Enables the implementation of custom logic for serializing build messages. Can be configured with XML attributes and elements in project files.
- **[``ICompilerDirective``](../src/Dassie/Extensions/ICompilerDirective.cs):** Defines a custom **compiler directive** that can be used in code.

## Creating a custom extension
The following example implements a minimal compiler extension that adds a new command to the compiler.

1. Create a class library project and add a reference to ``dc.dll``.
2. Add a type implementing the ``IPackage`` interface and implement the necessary properties ``Metadata`` and ``Commands``:
   
   ````csharp
   using Dassie.Extensions;
   using System;

   namespace DemoExtension;

   public class DemoExtensionPackage : IPackage
   {
       public PackageMetadata Metadata { get; } = new()
       {
           Name = "EchoExtension",
           Author = "Losch",
           Version = new(1, 0, 0, 0),
           Description = "A demo extension that repeats the specified text when executed."
       };

       public Type[] Commands { get; } = throw new NotImplementedException(); // Update after step 3
   }
   ````
3. Add a type implementing the ``ICompilerCommand`` interface and reference it in the extension package.

   ````csharp
   using Dassie.Extensions;
   using System;

   namespace DemoExtension;

   public class EchoCommand : ICompilerCommand
   {
       public string Command { get; } = "echo";
       public string UsageString { get; } = "echo <Text>";
       public string Description { get; } = "Repeats the specified text.";

       public int Invoke(string[] args)
       {
           Console.WriteLine(string.Join(" ", args));
           return 0;
       }
   }
   ````
4. To import the extension, compile the project and use the ``dc package import`` command.

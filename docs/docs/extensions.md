# Compiler Extensions
The Dassie compiler provides a rich API for extending its functionality with custom plug-ins, which are called **extensions**. This document intends to give an overview of the capabilities of the Extension API as well as what to look out for when implementing custom extensions.

## Format
An **extension** is a .NET type implementing the [``Dassie.Extensions.IPackage``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IPackage.cs) interface. An **extension package** is a .NET assembly containing one or more extensions. For an improved developer experience, the API also provides the abstract base class [``Extension``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/Extension.cs) which implements ``IPackage``.

## Extension lifecycle
Compiler extensions can be loaded in one of two modes:
- **Global:** The extension is enabled globally and is initialized as soon as the compiler is launched. It is active for every build. Managing global extensions is done through the ``dc package`` command.
- **Transient:** The extension is enabled only for a single build process. Managing transient extensions is done through the [``<Extensions>``](https://loschsoftware.github.io/dc/docs/projects.html#transient-extensions) tag in a project file.

``IPackage`` contains the virtual methods [``InitializeGlobal``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IPackage.cs#L34) and [``InitializeTransient``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IPackage.cs#L43) that can be overridden to implement custom behavior when the extension is loaded. Similarly, the [``Unload``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IPackage.cs#L48) method provides a way of performing an action when the extension is unloaded, regardless of loading mode.

> [!CAUTION]
> Do not rely on ``IDisposable`` for freeing resources of extensions, as the Dassie compiler does **not** invoke the ``Dispose`` method when they are unloaded. Always call ``Dispose`` in ``Unload`` to ensure unmanaged resources are cleaned up properly.

## Installing compiler extensions
The command ``dc package`` is used to manage extensions in global mode. To install extensions from an extension package stored on disk, use the ``dc package import`` subcommand. To install them from the online extension registry, use the ``dc package install`` command. This command is not supported yet.

## Extension API features
All features of the API are supported through interfaces in the [``Dassie.Extensions``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions) namespace. All features of an extension are centrally registered in ``IPackage`` through its various virtual methods. The following is a list of all available interfaces:
- **[``GlobalConfigProperty``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/GlobalConfigProperty.cs):** Defines a global configuration property.
- **[``ICompilerCommand``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/ICompilerCommand.cs):** Defines a custom compiler command to integrate external tools with the compiler.
- **[``IProjectTemplate``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IProjectTemplate.cs):** Defines a custom project template used in combination with the ``dc new`` command.
- **[``IConfigurationProvider``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IConfigurationProvider.cs):** Defines a configuration provider that serves as a template for a project file. This is used in combination with the [``Import``](https://github.com/loschsoftware/dc/blob/main/Projects.md#importing-project-files) attribute of project files.
- **[``IAnalyzer``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IAnalyzer.cs):** Defines a custom code analyzer used in combination with the ``dc analyze`` command.
- **[``IBuildLogWriter``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IBuildLogWriter.cs):** Allows the redirection of build message to an arbitrary ``TextWriter`` object. Requires the usage of the default build log device.
- **[``IBuildLogDevice``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IBuildLogDevice.cs):** Enables the implementation of custom logic for serializing build messages. Can be configured with XML attributes and elements in project files.
- **[``ICompilerDirective``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/ICompilerDirective.cs):** Defines a custom **compiler directive** that can be used in code.
- **[``IDocumentSource``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IDocumentSource.cs):** Defines a document source which is used to inject arbitrary Dassie source code into a compilation.
- **[``IDeploymentTarget``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/IDeploymentTarget.cs):** Defines a deployment target that is used in conjunction with the ``dc deploy`` compiler command.
- **[``ISubsystem``](https://github.com/loschsoftware/dc/blob/main/src/Dassie/Extensions/ISubsystem.cs):** Defines the characteristics of the application type of a Dassie project.

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
       public PackageMetadata Metadata => new()
       {
           Name = "EchoExtension",
           Author = "Losch",
           Version = new(1, 0, 0, 0),
           Description = "A demo extension that repeats the specified text when executed."
       };

       public Type[] Commands => throw new NotImplementedException(); // Update after step 3
   }

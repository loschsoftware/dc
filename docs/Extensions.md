# Extending the compiler with custom commands
The Dassie compiler provides the necessary infrastructure for declaring custom commands that can be called from the command line just like internal commands. This page provides an overview on how to install and create these compiler extensions.

## Format
A Dassie compiler extension is a .NET type implementing the ``Dassie.Extensions.IPackage`` interface. One assembly can contain multiple extension packages.

## Installing compiler extensions
The command ``dc package`` is used to manage extensions. In the future, there will be an online extension registry, enabling the sub-command ``dc package install``, but until then extensions can only be installed from local files using the command ``dc package import``.

## Creating a custom extension
Use the following steps to create a custom compiler extension. All code examples use C#, but any .NET language including Dassie can be used.

1. Create a class library project.
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

       public Type[] Commands { get; } = throw new NotImplementedException();
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
4. To import the extension, compile the project and use the ``dc package`` command line.

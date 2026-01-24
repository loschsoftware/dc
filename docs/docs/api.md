# Calling the Dassie compiler programmatically - The ``Dassie.Compiler`` library
The [Dassie.Compiler](https://github.com/loschsoftware/dc/tree/main/src/Dassie.Compiler) project contains a simple API for interacting with the Dassie compiler from .NET applications. Here is a quick example on how to use it, a more thorough documentation will follow later.
````csharp
using Dassie.Compiler;
using Dassie.Configuration;

DassieConfig config = new()
{
    AssemblyName = "demo"
};

string source = """
    import System
    Console.WriteLine "Hello World!"
    """;

CompilationContext ctx = CompilationContextBuilder.CreateBuilder()
    .WithConfiguration(config)
    .AddSourceFromText(source, "main.ds")
    .Build();

CompilationResult result = ctx.Compile();
````

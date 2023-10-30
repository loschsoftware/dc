# Dassie Command Line Compiler (dc.exe)
<img alt="GitHub commit activity (branch)" src="https://img.shields.io/github/commit-activity/m/loschsoftware/dc"> <img alt="GitHub issues" src="https://img.shields.io/github/issues/loschsoftware/dc">

A .NET Framework implementation of a Dassie compiler. For more information about the language, including code examples, visit the [dassie](https://github.com/loschsoftware/dassie) repository (will become public soon). For now, here's "Hello World!" in Dassie:

````dassie
println "Hello World!"
````
This uses the built-in ``println`` function. Since Dassie runs on the CLR, you can also use the .NET ``Console`` module.
````dassie
import System
Console.WriteLine "Hello World!"
````

Assuming the above code is contained in a file called ``hello.ds``, it can be compiled using the command ``dc hello.ds``, yielding an executable called ``hello.exe``. Alternatively, the command ``dc build`` can be used to compile all .ds source files in the current folder structure.

> [!NOTE]  
> Once the documentation repository is public, the code examples will be removed from this repo.

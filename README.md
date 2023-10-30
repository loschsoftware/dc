# Dassie Command Line Compiler (dc.exe)
<img alt="GitHub commit activity (branch)" src="https://img.shields.io/github/commit-activity/m/loschsoftware/dc">
A .NET Framework implementation of a Dassie compiler. For more information about the language, including code examples, visit the [dassie](https://github.com/loschsoftware/dassie) repository (will become public soon). For now, here's "Hello World!" in Dassie:

````dassie
println "Hello World!"
````
This uses the built-in ``println`` function. Since Dassie runs on the CLR, you can also use the .NET ``Console`` module.
````dassie
import System
Console.WriteLine "Hello World!"
````

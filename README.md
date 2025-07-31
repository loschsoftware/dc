# Dassie Compiler
<img alt="GitHub commit activity (branch)" src="https://img.shields.io/github/commit-activity/m/loschsoftware/dc"> <img alt="GitHub issues" src="https://img.shields.io/github/issues/loschsoftware/dc">

The official .NET compiler for the [Dassie](https://github.com/loschsoftware/dassie) programming language.

## Quick start
To get started with Dassie development, either download a binary from the 'Releases' section of this repository or build the compiler from source. To build from source, clone the repository and either run the appropriate build script or invoke MSBuild/Roslyn manually.

After the compilation is completed, the directory ``build`` will contain the compiler executables. It is recommended to add this directory to the ``Path`` environment variable to allow access to the compiler from anywhere.

You can also try out the language without downloading the compiler by using the online editor at [RyuGod](https://ryugod.com/pages/ide/dassie).

## Using the compiler
Here is the classic "Hello World" program in Dassie:
````dassie
println "Hello World!"
````
Assuming the above code is contained in a file called ``hello.ds``, the compiler can be invoked using the command ``dc hello.ds``, yielding a .NET assembly called ``hello.dll``, which can be executed using the ``dotnet`` command line. To automatically compile all ``.ds`` source files in the current folder structure, use the command ``dc build`` instead.

## Project system
For configuring compiler behavior and to simplify the build process of larger projects, the Dassie compiler includes a built-in project system. Here's how to initialize a new project:
````
dc new console Project01
````
The above command creates the folder structure for a console application called ``Project01`` as well as the main project file, ``dsconfig.xml``. It is used to configure compiler settings as well as to manage the resources and references associated with the application. All features offered by this project file are documented [here](https://github.com/loschsoftware/dc/blob/main/docs/Projects.md).

## Documentation
Further documentation on this compiler, including a list of all error codes, can be found in the [docs](https://github.com/loschsoftware/dc/blob/main/docs) directory. Documentation on the language itself, including more code examples, can be found in the [dassie](https://github.com/loschsoftware/dassie) repository.

## Contributing
Code contributions to the compiler are welcome. Just build the project from source as described above and open a pull request. To report bugs or request a feature, use the [issues](https://github.com/loschsoftware/dc/issues) section of this repository. If you have a feature request for the language itself, open an issue [here](https://github.com/loschsoftware/dassie/issues) instead.

If you wish to support the project monetarily instead, you can donate [here](https://www.paypal.com/donate/?hosted_button_id=R6XM6EX8WU9RN).

# Dassie Compiler
<img alt="GitHub commit activity (branch)" src="https://img.shields.io/github/commit-activity/m/loschsoftware/dc"> <img alt="GitHub issues" src="https://img.shields.io/github/issues/loschsoftware/dc">

This project aims to implement a .NET compiler for the [Dassie](https://github.com/loschsoftware/dassie) programming language. It acts as the reference implementation of the language.

## Getting started
To get started with Dassie development, either download a binary from the 'Releases' section of this repository or build the compiler from source. An installation of .NET 9 is required if compiling from source or using a framework-dependent compiler binary.

### Compiling from source
> [!IMPORTANT]
> Sometimes, the build script (``build.sh`` or ``build.cmd``) will fail on the first attempt. *This is normal and expected!* Simply run the script again and it should successfully compile. This only needs to be done once, every subsequent build will work normally.

**Linux:**
````bash
git clone https://github.com/loschsoftware/dc.git
cd dc
./build.sh
````
**Windows:**
````cmd
git clone https://github.com/loschsoftware/dc.git
cd dc
build
````

## Using the compiler
Here is the classic "Hello World" program in Dassie:
````dassie
println "Hello World!"
````
To compile the above source code, save it to a file (subsequently called ``main.ds``) and compile it as follows:
````
dc main.ds
````
This will generate a .NET assembly called ``main.dll`` as well as a native executable ``main`` (``main.exe`` on Windows).

## Project system
For configuring compiler behavior and to simplify the build process of larger projects, the Dassie compiler includes a built-in project system. Here's how to initialize a new project:
````
dc new console Project01
````
The above command creates the folder structure for a console application called ``Project01`` as well as the main project file, ``dsconfig.xml``. It is used to configure compiler settings as well as to manage the resources and references associated with the application. All features offered by the project system are documented [here](https://github.com/loschsoftware/dc/blob/main/docs/Projects.md).

## Documentation
Further documentation on the compiler can be found in the [docs](./docs) directory. Documentation on the language itself, including more code examples, can be found in the [dassie](https://github.com/loschsoftware/dassie) repository.

## Contributing
Code contributions to the compiler are welcome. Useful information for contributors can be found [here](./docs/Contributing). To report bugs or request a feature, use the [issues](https://github.com/loschsoftware/dc/issues) section of this repository. If you have a feature request for the language itself, open an issue [here](https://github.com/loschsoftware/dassie/issues) instead.

If you wish to support the project monetarily instead, you can donate [here](https://www.paypal.com/donate/?hosted_button_id=R6XM6EX8WU9RN).

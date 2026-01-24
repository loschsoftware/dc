# Getting Started with the Dassie Compiler

This guide will help you get started with the Dassie Compiler and create your first Dassie project.

## Prerequisites

- **.NET Runtime**: The Dassie Compiler requires .NET 10 or later to be installed on your system. Download it from the [official .NET website](https://dotnet.microsoft.com/download).

## Installation

### Option 1: Download Pre-built Binary

Download the latest release from the [GitHub releases page](https://github.com/loschsoftware/dc/releases) and extract it to a directory of your choice. Add the directory to your system's PATH environment variable for easy access.

### Option 2: Build from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/loschsoftware/dc.git
   cd dc
   ```

2. Build the compiler:
   ```bash
   dotnet build src/Dassie/Dassie.csproj -c Release
   ```

3. The compiled binaries will be in `src/Dassie/bin/Release/net10.0/`.

## Verify Installation

Run the following command to verify the installation:

```bash
dc --version
```

You should see output displaying the compiler version and environment information.

## Your First Project

### Step 1: Create a New Project

Use the `dc new` command to create a new console application:

```bash
dc new console HelloWorld
cd HelloWorld
```

This creates a new directory `HelloWorld` with the following structure:

```
HelloWorld/
??? dsconfig.xml    # Project configuration file
??? main.ds         # Main source file
```

### Step 2: Explore the Generated Code

Open `main.ds` to see the generated code:

```dassie
import System

Console.WriteLine "Hello, World!"
```

### Step 3: Build the Project

Build your project using:

```bash
dc build
```

The compiler will create a `bin/` directory containing the compiled output.

### Step 4: Run the Project

Run your compiled application:

```bash
dc run
```

You should see "Hello, World!" printed to the console.

## Project Configuration

The `dsconfig.xml` file in your project root controls how your project is built. Here's a basic example:

```xml
<?xml version="1.0" encoding="utf-8"?>
<DassieConfig FormatVersion="1.0">
  <ApplicationType>Console</ApplicationType>
  <AssemblyFileName>HelloWorld</AssemblyFileName>
</DassieConfig>
```

For a complete reference of all configuration options, see [Project Files](./projects.md).

## Common Tasks

### Adding References

To reference an external .NET assembly:

```xml
<References>
  <AssemblyReference>path/to/library.dll</AssemblyReference>
</References>
```

To reference a NuGet package:

```xml
<References>
  <PackageReference Version="1.0.0">PackageName</PackageReference>
</References>
```

### Building for Release

Create a build profile in your `dsconfig.xml`:

```xml
<BuildProfiles>
  <BuildProfile Name="Release">
    <Settings>
      <Configuration>Release</Configuration>
      <IlOptimizations>true</IlOptimizations>
    </Settings>
  </BuildProfile>
</BuildProfiles>
```

Then build with:

```bash
dc build Release
```

### Watching for Changes

During development, use the watch command to automatically rebuild when files change:

```bash
dc watch
```

Or automatically run your application on changes:

```bash
dc watch -c run
```

### Running Tests

If your project includes unit tests, run them with:

```bash
dc test
```

For more information on testing, see [Unit Testing](./testing.md).

## Next Steps

- **[Command-Line Reference](./cli.md)**: Learn about all available compiler commands
- **[Project Files](./projects.md)**: Detailed configuration options
- **[Compiler Extensions](./extensions.md)**: Extend the compiler with custom functionality
- **[Error Codes](./errors.md)**: Reference for compiler messages

## Getting Help

- Use `dc help` to see all available commands
- Use `dc help <command>` for detailed help on a specific command
- Visit the [GitHub repository](https://github.com/loschsoftware/dc) for issues and discussions

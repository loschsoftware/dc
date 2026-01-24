# Dassie Compiler Documentation

Welcome to the official documentation for the **Dassie Compiler** (`dc`). This documentation covers everything you need to know about configuring, using, and extending the compiler.

> [!TIP]
> Looking for the Dassie language documentation? Visit the [Dassie language repository](https://github.com/loschsoftware/dassie) for syntax, semantics, and language feature documentation.

## What is the Dassie Compiler?

The Dassie Compiler is a modern, extensible compiler for the Dassie programming language. It compiles Dassie source code (`.ds` files) to .NET assemblies, supporting both Just-In-Time (JIT) and Ahead-Of-Time (AOT) compilation modes.

## Key Features

- **Modern .NET Integration**: Targets the latest .NET runtime with full interoperability with .NET libraries
- **Flexible Build System**: Project files (`dsconfig.xml`) with support for build profiles, macros, and custom build events
- **Project Groups**: Organize multiple related projects and deploy them together
- **Extensibility**: Rich extension API for custom commands, analyzers, project templates, and more
- **Built-in Tooling**: Code analysis, unit testing, file watching, and interactive scratchpad
- **Editor Support**: API for semantic highlighting, tooltips, navigation, and folding regions

## Documentation Overview

| Section | Description |
|---------|-------------|
| [Getting Started](./getting-started.md) | Quick start guide for new users |
| [Command-Line Reference](./cli.md) | Complete reference for all compiler commands |
| [Project Files](./projects.md) | Configuration options for `dsconfig.xml` |
| [Project Groups](./project-groups.md) | Managing multi-project solutions |
| [Error Codes](./errors.md) | Reference for all compiler messages and errors |
| [Scratchpad](./scratchpad.md) | Interactive code execution environment |
| [Unit Testing](./testing.md) | Writing and running tests |
| [Compiler API](./api.md) | Using the compiler from .NET applications |
| [Compiler Extensions](./extensions.md) | Creating and managing compiler plug-ins |
| [Editor API](./editors.md) | Integrating with code editors |

## Contributing

Interested in contributing to the Dassie compiler? Check out the [Contributing](./contributing/) section for code style guidelines, architecture overview, and extension API documentation.

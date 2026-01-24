# Contributing to the Dassie Compiler

Thank you for your interest in contributing to the Dassie compiler! This section contains all the information you need to get started.

## Getting Started

### Prerequisites

- .NET 10 SDK or later
- Git
- A code editor (Visual Studio, VS Code, Rider, etc.)

### Setting Up the Development Environment

1. **Clone the repository:**
   ```bash
   git clone https://github.com/loschsoftware/dc.git
   cd dc
   ```

2. **Build the solution:**
   ```bash
   dotnet build
   ```

3. **Run tests:**
   ```bash
   dotnet test
   ```

## Project Structure

The solution consists of several projects:

| Project | Description |
|---------|-------------|
| `Dassie` | The main compiler executable |
| `Dassie.Core` | Standard library and runtime types |
| `Dassie.Text` | Editor integration APIs |
| `Dassie.Configuration` | Project file handling and configuration |
| `Dassie.Compiler` | Compiler API for .NET applications |

## How to Contribute

### Reporting Issues

- Check if the issue already exists in the [issue tracker](https://github.com/loschsoftware/dc/issues)
- Include a minimal reproduction case
- Provide version information (`dc --version`)
- Describe expected vs. actual behavior

### Submitting Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Make your changes following the [code style guidelines](./codestyle.md)
4. Ensure all tests pass
5. Commit with clear, descriptive messages
6. Push to your fork and create a pull request

### Code Review Process

All contributions go through code review. Reviewers will check:
- Adherence to code style guidelines
- Test coverage
- Documentation updates
- Backward compatibility

## Documentation

### Guidelines

- Keep documentation up to date with code changes
- Use clear, concise language
- Include code examples where helpful
- Follow the existing documentation style

### Building Documentation

The documentation uses [DocFX](https://dotnet.github.io/docfx/). To build locally:

```bash
cd docs
docfx build
```

To preview:
```bash
docfx serve _site
```

## Areas for Contribution

### Good First Issues

Look for issues labeled `good first issue` in the issue tracker. These are suitable for newcomers to the project.

### Documentation

- Improving existing documentation
- Adding examples
- Fixing typos and clarifications

### Compiler Features

- Bug fixes
- Performance improvements
- New language features (coordinate with maintainers first)

### Tooling

- IDE integrations
- Build system improvements
- Compiler extensions

## Resources

- [Code Style Guidelines](./codestyle.md) - Required coding conventions
- [Extension API Guide](./extensions.md) - Adding features to the extension API
- [GitHub Repository](https://github.com/loschsoftware/dc) - Source code and issues

## Questions?

If you have questions about contributing, feel free to:
- Open a discussion on GitHub
- Ask in an issue (label it as a question)

Thank you for contributing to the Dassie compiler!

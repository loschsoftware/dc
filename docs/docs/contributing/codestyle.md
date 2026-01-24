# Code style guidelines

Follow these code style guidelines whenever editing C# code of the compiler. Compliance with these guidelines is required for pull requests to be merged.

## Naming conventions
- Use ``_camelCase`` prefixed with an underscore for private fields.
- Use ``camelCase`` for method parameters and local variables.
- Use ``PascalCase`` for all functions and public type members.
- Use ``PascalCase`` for all types and type parameters.
- Prefix all interface names with ``I``.

**❌ Wrong:**
````csharp
public class worker
{
  private string Something;
  public void doWork(string _str)
  {
    Something = _str;
  }
}
````

**✔️ Correct:**
````csharp
public class Worker
{
  private string _something;
  public void DoWork(string str)
  {
    _something = str;
  }
}
````

## Namespaces
Always use file-scoped namespaces. Separate namespace declarations from using directives and type declarations with an empty line.

**❌ Wrong:**
````csharp
namespace ExampleNamespace
{
  class ExampleType
  {
  }
}
````

**✔️ Correct:**
````csharp
namespace ExampleNamespace;

class ExampleType
{
}
````

## Braces
Use Allman style braces.

**❌ Wrong:**
````csharp
private void DoWork() {
  // ...
}
````

**✔️ Correct:**
````csharp
private void DoWork()
{
  // ...
}
````

## Access modifiers
Explicitly specify access modifiers on types and members, even if they are the default.

**❌ Wrong:**
````csharp
class Worker
{
  void DoWork()
  {
    // ...
  }
}
````

**✔️ Correct:**
````csharp
internal class Worker
{
  private void DoWork()
  {
    // ...
  }
}
````

## Control flow statements
If the body of a control flow statement consists of only one line, omit the braces. Always use braces if the body is more than one line, even if it is just a single statement.

**❌ Wrong:**
````csharp
if (something)
{
    DoWork();
}

if (something)
  obj = new()
  {
    Property = "Example"
  };
````

**✔️ Correct:**
````csharp
if (something)
  DoWork();

if (something)
{
  obj = new()
  {
    Property = "Example"
  };
}
````

## Object instantiation
Use target-typed new expressions wherever possible. Do not use ``var`` except for LINQ queries with complicated return types.

**❌ Wrong:**
````csharp
StreamWriter writer = new StreamWriter("file.txt");
var writer = new StreamWriter("file.txt");
````

**✔️ Correct:**
````csharp
StreamWriter writer = new("file.txt");
````

## Collection initialization
Use C# 12 collection expressions.

**❌ Wrong:**
````csharp
List<string> items = new();
int[] numbers = { 1, 2, 3 };
````

**✔️ Correct:**
````csharp
List<string> items = [];
int[] numbers = [1, 2, 3];
````

## LINQ
For most general cases, use method syntax. Avoid using query syntax unless strictly necessary for readability.

**❌ Wrong:**
````csharp
int evenCount = (from n in numbers
  where n % 2 == 0
  select n).Count();
````

**✔️ Correct:**
````csharp
int evenCount = numbers.Count(n => n % 2 == 0);
````

## Expression-bodied members
Use expression-bodied methods and property accessors wherever possible.

**❌ Wrong:**
````csharp
private string _name;
public string Name
{
  get
  {
    return _name;
  }

  set
  {
    SetProperty(ref _name, value);
  }
}
````

**✔️ Correct:**
````csharp
private string _name;
public string Name
{
  get => _name;
  set => SetProperty(ref _name, value);
}
````

## Documentation comments
Document **all** public types and their members with XML documentation comments.

**❌ Wrong:**
````csharp
public interface ICompilerCommand
{
}
````

**✔️ Correct:**
````csharp
/// <summary>
/// Defines a command used to add additional features to the Dassie compiler.
/// </summary>
public interface ICompilerCommand
{
}
````

## String interpolation
Prefer string interpolation over string concatenation or `String.Format`.

**❌ Wrong:**
````csharp
string message = "Error in file " + fileName + " at line " + lineNumber;
string formatted = String.Format("Error in file {0} at line {1}", fileName, lineNumber);
````

**✔️ Correct:**
````csharp
string message = $"Error in file {fileName} at line {lineNumber}";
````

## Null handling
Use null-conditional and null-coalescing operators where appropriate.

**❌ Wrong:**
````csharp
if (config != null && config.Settings != null)
    return config.Settings.Value;
return defaultValue;
````

**✔️ Correct:**
````csharp
return config?.Settings?.Value ?? defaultValue;
````

## Pattern matching
Use pattern matching for type checks and null checks.

**❌ Wrong:**
````csharp
if (obj != null && obj is MyType)
{
    MyType typed = (MyType)obj;
    typed.DoSomething();
}
````

**✔️ Correct:**
````csharp
if (obj is MyType typed)
    typed.DoSomething();
````

## File organization
Organize file contents in the following order:
1. Using directives
2. Namespace declaration
3. Types (classes, interfaces, enums, etc.)

Within types, organize members in this order:
1. Constants
2. Fields
3. Constructors
4. Properties
5. Methods
6. Nested types

## Error handling
Prefer specific exception types over generic ones. Include meaningful error messages.

**❌ Wrong:**
````csharp
throw new Exception("Something went wrong");
````

**✔️ Correct:**
````csharp
throw new InvalidOperationException($"Cannot process file '{fileName}': file not found.");
````

## Async/Await
Use `async`/`await` for asynchronous operations. Suffix async method names with `Async`.

**✔️ Correct:**
````csharp
public async Task<string> ReadFileAsync(string path)
{
    return await File.ReadAllTextAsync(path);
}
````

## Comments
- Use comments sparingly—code should be self-documenting where possible
- Write comments that explain *why*, not *what*
- Keep comments up to date with code changes
- Use `// TODO:` for planned improvements
- Use `// FIXME:` for known issues

## General principles
1. **Keep methods short**: Methods should do one thing well. If a method exceeds ~30 lines, consider refactoring.
2. **Avoid magic numbers**: Use named constants or enums instead of hardcoded values.
3. **Fail fast**: Validate inputs early and throw meaningful exceptions.
4. **Prefer composition over inheritance**: Use interfaces and composition for flexibility.
5. **Write testable code**: Design components that can be easily unit tested.

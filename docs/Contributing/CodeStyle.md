# Dassie Compiler Contribution Guidelines – Code Style

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
Always use file-scoped namespaces. Separate namespace declarations from using directives and type declarations by an empty line.

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
Follow the Microsoft convention of placing each brace in its own line.

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
var items = List<string>();
List<string> items = new List<string>();
````

**✔️ Correct:**
````csharp
List<string> items = new();
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

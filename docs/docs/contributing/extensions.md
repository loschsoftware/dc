# Adding new features to the Compiler Extension API
A **compiler extension** is a .NET assembly containing a public type implementing the ``IPackage`` interface in the ``Dassie.Extensions`` namespace. All features of the API are supported through special interfaces in this namespace. To add a new feature to the extension API, follow the below steps. The code below uses the ``IBuildLogWriter`` interface as an example.
<hr/>

1. **Create a new public interface in the [``Dassie.Extensions``](../../src/Dassie/Extensions) namespace:**
````csharp
using System.IO;

namespace Dassie.Extensions;

/// <summary>
/// Defines a mechanism to write build logs using <see cref="System.IO.TextWriter"/>.
/// </summary>
public interface IBuildLogWriter
{
    /// <summary>
    /// The name of the log writer.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The error severities at which the log writer is active.
    /// </summary>
    public BuildLogSeverity Severity { get; }

    /// <summary>
    /// All text writers making up the build log writer.
    /// </summary>
    public TextWriter[] Writers { get; }
}
````

2. **Add a new virtual method to [``IPackage``](../../src/Dassie/Extensions/IPackage.cs) in order not to break existing implementations:**
````csharp
/// <summary>
/// An array of build log writers.
/// </summary>
public virtual IBuildLogWriter[] BuildLogWriters() => [];
````

3. **Add a corresponding property to [``ExtensionLoader``](../../src/Dassie/Extensions/ExtensionLoader.cs):**
````csharp
public static IEnumerable<IBuildLogWriter> BuildLogWriters => InstalledExtensions.Select(a => a.BuildLogWriters()).SelectMany(a => a);
````

4. **Add a new section to the [``dc package info``](../../src/Dassie/Cli/Commands/PackageCommand.cs#L129) command output:**
````csharp
if (package.BuildLogWriters().Length != 0)
{
  sb.AppendLine();
  WriteHeading("Build log writers");

  foreach (IBuildLogWriter writer in package.BuildLogWriters())
    sb.AppendLine($"    {writer.Name}");
}
````

5. **Modify relevant code parts of the compiler to support the new API feature.**

<hr/>

Following these steps ensures the new API feature is correctly integrated and recognized by the compiler.

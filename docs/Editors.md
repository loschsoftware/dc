# Tools for code editors - working with the ``Dassie.Text`` library
The Dassie compiler includes the library ``Dassie.Text`` to support modern editor features such as semantic highlighting, folding regions and smart tool tips for code elements. To get access to these features, the compiler provides the method ``FileCompiler.GetEditorInfo()`` that generates the necessary metadata on a per-file basis. Here is a basic example on how to use it:
````csharp
using Dassie.CodeGeneration;
using Dassie.Configuration;

string sourceFile = "main.ds";
DassieConfig config = GetConfig(); // Not implemented, just deserializes an XML file into a DassieConfig object

EditorInfo info = FileCompiler.GetEditorInfo(sourceFile, config);
````
The following chapters deal with all features offered by ``EditorInfo``.

|**Table of contents**|
|---|
|[Error messages](./Editors.md#error-messages)|
|[Semantic highlighting](./Editors.md#semantic-highlighting)|
|[Tooltips](./Editors.md#tooltips)|
|[Navigation](./Editors.md#navigation)|
|[Folding regions and structure guide lines](./Editors.md#folding-regions-and-structure-guide-lines)|

## Error messages
The property ``Errors`` allows editors to access all error messages generated by the compiler. It uses the same ``ErrorInfo`` type that is used by the compiler internally. Adding to the above example, here is a program that writes all error messages to a file:
````csharp
using Dassie.CodeGeneration;
using Dassie.Configuration;
using System.IO;

string sourceFile = "main.ds";
DassieConfig config = GetConfig(); // Not implemented, just deserializes an XML file into a DassieConfig object
EditorInfo info = FileCompiler.GetEditorInfo(sourceFile, config);

string logFile = "errors.log";
using StreamWriter sw = File.AppendText(logFile);

foreach (ErrorInfo error in info.Errors)
  sw.WriteLine(error.ToString());
````

## Semantic highlighting using fragments
``Dassie.Text`` defines the structure ``Fragment`` as a section of text with a specific color and additional metadata. In addition to semantic highlighting, fragments also allow a symbol to have a tool tip. They also support navigation, which is used to implement the "Go to definition" feature of many editors. This section only deals with semantic highlighting, the other features offered by fragments are explained further down.

The enumeration ``Dassie.Text.Color`` defines a list of code structures that all have a specific color inside the editor. The library does not mandate any specific color values, to allow editors to have multiple color schemes. If a fragment needs a custom color, the property ``SpecialColor`` is used.

Here is an example that extracts all fragments from a source file and displays them in the console:
````csharp
using Dassie.CodeGeneration;
using Dassie.Configuration;
using Dassie.Text;
using System.Collections.Generic;

string sourceFile = "main.ds";
DassieConfig config = GetConfig(); // Not implemented, just deserializes an XML file into a DassieConfig object

EditorInfo info = FileCompiler.GetEditorInfo(sourceFile, config);
List<Fragment> fragments = info.Fragments.Fragments; // Not a typo

foreach (Fragment fragment in fragments) {
  string line = fragment.Line.ToString();
  string column = fragment.Column.ToString();
  string length = fragment.Length.ToString();
  string color = fragment.Color.ToString();

  Console.Write($"[{line},{column}-{column + length}] => {color}");
}
````

## Tooltips
Tooltips are used to preview the signature of types and methods inside an editor when hovering over a reference to such code elements. In the ``Dassie.Text`` library, a tooltip is represented by a list of ``Word`` objects. A word is the same as a ``Fragment``, except that it does not refer to a location inside of a file but instead to the actual text itself.

Creating custom tooltips is simplified with the ``TooltipGenerator`` class, that automatically generates tooltips for types, local variables and members.

> [!IMPORTANT]  
> Internally, ``Word`` is implemented as a structure containing a ``Fragment`` and a ``Text`` property. Since the location properties of the fragment are not needed in this context, the properties ``Line``, ``Column`` and ``Length`` of the fragment will always be ``0``.

## Navigation
``Fragment`` contains a property called ``IsNavigationTarget`` that determines if the fragment can be the target of a navigation command inside a code editor. For example, if the file contains multiple references to a specific field, only the declaration of the field will be marked as a navigation target. ``Fragment`` also defines a property called ``NavigationTargetKind`` that declares the code structure behind the navigation operation in case of ambiguity.

## Folding regions and structure guide lines
Folding regions are represented by the ``Dassie.Text.Regions.FoldingRegion`` class and can be accessed from the ``FoldingRegions`` property of ``EditorInfo``. They define the start and end position of the region.

> [!CAUTION]
> In some cases, due to internal parser limitations, the ``EndColumn`` property of ``FoldingRegion`` might be off by 2. In such cases, it is always higher than it is supposed to be, never lower.

Structure guide lines are supported by the ``GuideLine`` class. They are very similar to folding regions, except that the text column is always the same at the start and the end.

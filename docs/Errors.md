# Compiler error codes
Every compiler message (information, warning or error) has a unique error code that is used to identify the issue causing the message. The following table lists all error codes emitted by the Dassie compiler along with a code example causing the error and a way to fix it.

<table>
  <tr>
    <th>Code</th>
    <th>Type</th>
    <th>Cause</th>
    <th>Code example</th>
    <th>Fix</th>
  </tr>
  <tr>
    <td>DS0000</td>
    <td>Error</td>
    <td>Internal compiler error. This error is thrown if an unhandled exception occurs in the compiler. When you encounter this error, please open an [issue](https://github.com/loschsoftware/dc/issues) in this repository.</td>
    <td></td>
    <td></td>
  </tr>
  <tr>
    <td>DS0001</td>
    <td>Error</td>
    <td>Emitted when the parser encounters syntactically incorrect source code.</td>
    <td>
      <pre lang="Dassie">
println "Hello World! # DS0001</pre>
    </td>
    <td>The code throws DS0001 because the string literal is not closed. Add another quotation mark at the end to fix it.</td>
  </tr>
  <tr>
    <td>DS0002</td>
    <td>Error</td>
    <td>Emitted when a function or method is called with illegal arguments, or if an operator is not defined for the specified types.</td>
    <td>
      <pre>
x = 2
y = true
z = x + y # DS0002</pre>
    </td>
    <td>Call the function with the correct argument types, or find a conversion that allows the operator to be called.</td>
  </tr>
  <tr>
    <td>DS0006</td>
    <td>Error</td>
    <td>Emitted when a value of the wrong type is assigned to a variable.</td>
    <td>
      <pre>
var x = 2
x = 2.5 # DS0006</pre>
    </td>
    <td>Assign a value of the correct type or convert the value to the correct type.</td>
  </tr>
  <tr>
    <td>DS0009</td>
    <td>Error</td>
    <td>Emitted when a referenced name cannot be resolved. Often caused by missing import directives.</td>
    <td>
      <pre>
Console.WriteLine "Hello World!" # DS0009</pre>
    </td>
    <td>Import the 'System' namespace.</td>
  </tr>
  <tr>
    <td>DS0018</td>
    <td>Error</td>
    <td>Emitted when an immutable value is being reassigned.</td>
    <td>
      <pre>
x = 2
x = 3 # DS0018</pre>
    </td>
    <td>Mark the value as mutable using <code>var</code>.</td>
  </tr>
  <tr>
    <td>DS0022</td>
    <td>Error</td>
    <td>Emitted when an assembly referenced in <code>dsconfig.xml</code> does not exist.</td>
    <td>
      <pre lang="xml">
&lt;!--DS0022--&gt;
&lt;AssemblyReference&gt;NonExistent.dll&lt;/AssemblyReference&gt;</pre>
    </td>
    <td>Correct the file path to make sure the file exists.</td>
  </tr>
  <tr>
    <td>DS0023</td>
    <td>Error</td>
    <td>Emitted when a file referenced in <code>dsconfig.xml</code> does not exist.</td>
    <td>
      <pre lang="xml">
&lt;!--DS0022--&gt;
&lt;FileReference&gt;NonExistent.png&lt;/FileReference&gt;</pre>
    </td>
    <td>Correct the file path to make sure the file exists.</td>
  </tr>
  <tr>
    <td>DS0027</td>
    <td>Error</td>
    <td>Emitted when the program contains no executable code.</td>
    <td>
      <pre># DS0027</pre>
    </td>
    <td>Add some code to the program.</td>
  </tr>
  <tr>
    <td>DS0029</td>
    <td>Error</td>
    <td>Emitted when the compiler has insufficient permission to read a file.</td>
    <td>
    </td>
    <td>Make sure the compiler has permission to read the file.</td>
  </tr>
  <tr>
    <td>DS0030</td>
    <td>Error</td>
    <td>Emitted when the program has no entry point.</td>
    <td>
      <pre># DS0030
module Program = {
	Main (): int32 = {
		println "Hello World!"
		0
	}
}</pre>
    </td>
    <td>Mark <code>Main</code> with the <code>&lt;EntryPoint&gt;</code> attribute</td>
  </tr>
  <tr>
    <td>DS0035</td>
    <td>Error</td>
    <td>Emitted when the application entry point is not static.</td>
    <td>
      <pre>type Program = {
	&lt;EntryPoint&gt;
	Main (): int32 = { # DS0035
		println "Hello World!"
		0
	}
}</pre>
    </td>
    <td>Mark <code>Main</code> with the <code>static</code> modifier.</td>
  </tr>
  <tr>
    <td>DS0036</td>
    <td>Error</td>
    <td>Emitted when a comparison operator is not implemented for the specified types.</td>
    <td>
      <pre>x = 2
y = "text"
z = x < y # DS0036</pre>
    </td>
    <td>Use a different operator or convert one of the operators to an allowed type.</td>
  </tr>
  <tr>
    <td>DS0038</td>
    <td>Error</td>
    <td>Emitted when the condition of a conditional expression is not a boolean.</td>
    <td>
      <pre>? x = 2 = { # DS0038
	println "x is equal to 2."
}</pre>
    </td>
    <td>Change the type of the condition or fix a typo.</td>
  </tr>
  <tr>
    <td>DS0041</td>
    <td>Error</td>
    <td>Emitted when an array or list contains multiple types of values.</td>
    <td>
      <pre>items = @[ 1, 2, 3.5, 4 ] # DS0041</pre>
    </td>
    <td>Use only one type of value in the list or use a tuple.</td>
  </tr>
  <tr>
    <td>DS0042</td>
    <td>Error</td>
    <td>Emitted when trying to index into an array or list with a value that is not an integer.</td>
    <td>
      <pre>items = @[ 1, 2, 3 ]
println items::"a" # DS0042</pre>
    </td>
    <td>Only use integers as indices on arrays and lists.</td>
  </tr>
  <tr>
    <td>DS0043</td>
    <td>Warning</td>
    <td>Warning emitted when a while loop runs indefinetly, possibly on accident.</td>
    <td>
      <pre>@ "x" = { # DS0043
	println "Loop"
}</pre>
    </td>
    <td>Fix the loop condition if the loop was not intended to run indefinetly. Otherwise, the warning can be ignored.</td>
  </tr>
  <tr>
    <td>DS0045</td>
    <td>Error</td>
    <td>Emitted when an inline IL instruction uses an invalid CIL opcode.</td>
    <td>
      <pre>il "ldc.j4.s 23" # DS0045</pre>
    </td>
    <td>Use a valid IL opcode.</td>
  </tr>
  <tr>
    <td>DS0047</td>
    <td>Warning</td>
    <td>Warning emitted when a union type contains duplicate cases.</td>
    <td>
      <pre>x: (string | string) = () # DS0047</pre>
    </td>
    <td>Remove duplicate cases from the union type.</td>
  </tr>
  <tr>
    <td>DS0048</td>
    <td>Error</td>
    <td>Emitted when a source file to be compiled does not exist.</td>
    <td>
      <pre>dc file.ds # DS0048</pre>
    </td>
    <td>Make sure the path to the source file is correct.</td>
  </tr>
  <tr>
    <td>DS0050</td>
    <td>Error</td>
    <td>Emitted when the return type of an auto-generated entry point is not 'int32' or 'null'.</td>
    <td>
      <pre>println "Hello World!"
"Goodbye" # DS0050</pre>
    </td>
    <td>Ensure the last expression in the program is of type 'int32' or 'null'.</td>
  </tr>
  <tr>
    <td>DS0052</td>
    <td>Error</td>
    <td>Emitted when a type member has a modifier that is invalid in the current context.</td>
    <td>
      <pre>type Point = {
	closed X: int32 # DS0052
	closed Y: int32 # DS0052
}</pre>
    </td>
    <td>Remove or replace the invalid modifier.</td>
  </tr>
  <tr>
    <td>DS0053</td>
    <td>Error</td>
    <td>Emitted when a type member returns a different type than specified.</td>
    <td>
      <pre>type Worker = {
	DoWork (): int32 = {
		"Done" # DS0053
	}
}</pre>
    </td>
    <td>Change the return type to match returned expression.</td>
  </tr>
  <tr>
    <td>DS0054</td>
    <td>Error</td>
    <td>Same as DS0053, but for fields.</td>
    <td>
      <pre>type Point = {
	X: int32 = 2.5 # DS0054
}</pre>
    </td>
    <td>Change the type of the field to its default value.</td>
  </tr>
  <tr>
    <td>DS0055</td>
    <td>Error</td>
    <td>Emitted when a program contains multiple methods marked as <code>&lt;EntryPoint&gt;</code>.</td>
    <td>
      <pre>module App = {
  &lt;EntryPoint&gt;
  Main1 (): null = {}

 
  &lt;EntryPoint&gt; # DS0055
  Main2 (): null = {}
}</pre>
    </td>
    <td>Remove the <code>&lt;EntryPoint&gt;</code> attribute from all but one of the methods.</td>
  </tr>
  <tr>
    <td>DS0056</td>
    <td>Error</td>
    <td>Emitted when a symbol referenced inside a code block cannot be resolved.</td>
    <td>
      <pre>y = 10
println x # DS0056</pre>
    </td>
    <td>Make sure the referenced symbol exists. Correct a typo if necessary.</td>
  </tr>
  <tr>
    <td>DS0057</td>
    <td>Error</td>
    <td>Emitted when a variable with a type annotation is initialized with a value of the wrong type.</td>
    <td>
      <pre>x: int32 = 2.5 # DS0057</pre>
    </td>
    <td>Correct the type annotation or assigned value.</td>
  </tr>
  <tr>
    <td>DS0058</td>
    <td>Information</td>
    <td>Emitted when a type or member specifies a modifier that is applied by default.</td>
    <td>
      <pre>module Tools = {
	static Tool1 = {} # DS0058
}</pre>
    </td>
    <td>Remove the redundant modifier.</td>
  </tr>
  <tr>
    <td>DS0059</td>
    <td>Error</td>
    <td>Emitted when the <code>throw</code> keyword is used without arguments outside of a <code>catch</code> block.</td>
    <td>
      <pre>throw # DS0059</pre>
    </td>
    <td>Add an exception to throw or move the statement into a <code>catch</code> block.</td>
  </tr>
  <tr>
    <td>DS0060</td>
    <td>Error</td>
    <td>Emitted when the <code>throw</code> keyword is used to throw a value type.</td>
    <td>
      <pre>throw 3.14 # DS0060</pre>
    </td>
    <td>Box the value to be thrown.</td>
  </tr>
  <tr>
    <td>DS0061</td>
    <td>Error</td>
    <td>Emitted when a <code>try</code> block is not immediately followed by a <code>catch</code> block.</td>
    <td>
      <pre>try = {
	DangerousCall
} # DS0061</pre>
    </td>
    <td>Add a <code>catch</code> block.</td>
  </tr>
  <tr>
    <td>DS0063</td>
    <td>Error</td>
    <td>Emitted when trying to use a feature that is in development, but not yet supported by the compiler.</td>
    <td></td>
    <td>Fall back to an alternative.</td>
  </tr>
  <tr>
    <td>DS0064</td>
    <td>Error</td>
    <td>Emitted when trying to use a <code>try</code> block as an expression.</td>
    <td><pre>x = try = {} # DS0064</pre>
    </td>
    <td>Don't use <code>try</code> as an expression.</td>
  </tr>
  <tr>
    <td>DS0065</td>
    <td>Error</td>
    <td>Emitted when the left side of an assignment is invalid.</td>
    <td><pre>4 = 5 # DS0065</pre>
    </td>
    <td>Make sure the left side of the assignment is a symbol.</td>
  </tr>
  <tr>
    <td>DS0067</td>
    <td>Error</td>
    <td>Emitted when a referenced resource file does not exist.</td>
    <td><pre lang="xml">&lt;!-- DS0067 --&gt;
&lt;ManagedResource&gt;invalid.txt&lt;/ManagedResource&gt;</pre>
    </td>
    <td>Make sure the referenced file exists.</td>
  </tr>
  <tr>
    <td>DS0069</td>
    <td>Error</td>
    <td>Emitted when an external tool required to perform an action is not installed.</td>
    <td></td>
    <td>Install the required external component.</td>
  </tr>
  <tr>
    <td>DS0070</td>
    <td>Information</td>
    <td>Emitted when the <code>&lt;VersionInfo&gt;</code> tag is used in a project file.</td>
    <td><pre lang="xml">&lt;!-- DS0070 --&gt;
&lt;VersionInfo/&gt;</pre></td>
    <td>The usage of this tag worsens compilation performance. To decrease compile times, pre-compile your version information and reference it as a native resource.</td>
  </tr>
  <tr>
    <td>DS0071</td>
    <td>Warning</td>
    <td>Emitted when an error code is ignored that cannot be ignored.</td>
    <td><pre lang="xml">&lt;IgnoredMessages&gt;
	&lt;!-- DS0071 --&gt;
	&lt;Message&gt;DS0065&lt;/Message&gt;
&lt;/IgnoredMessages&gt;</pre></td>
    <td>Remove or correct the ignored error code.</td>
  </tr>
  <tr>
    <td>DS0072</td>
    <td>Error</td>
    <td>Emitted when <code>dc build</code> is called but there are no source files in the current folder structure.</td>
    <td></td>
    <td>Make sure the current folder structure contains source files ending in <code>.ds</code>.</td>
  </tr>
  <tr>
    <td>DS0073</td>
    <td>Error</td>
    <td>Emitted when the length of a fully-qualified type name exceeds 1024 characters.</td>
    <td><pre># Just imagine the ellipsis
# representing a very long string
type ThisIsAVeryLong...Name = {} # DS0073</pre></td>
    <td>Reduce the length of the type name.</td>
  </tr>
  <tr>
    <td>DS0074</td>
    <td>Error</td>
    <td>Emitted when a function or member contains more than 65534 locals.</td>
    <td><pre>x = 0
y = 0
z = 0
.
.
.
zzzzzzzzzzzzzz = 0 # DS0074</pre></td>
    <td>Reduce the amount of locals in the function.</td>
  </tr>
  <tr>
    <td>DS0075</td>
    <td>Error</td>
    <td>Emitted when the value of a local exceeds the largest allowed value of its type.</td>
    <td><pre>x: int32 = 2147483648 # DS0075</pre></td>
    <td>Use a larger type.</td>
  </tr>
  <tr>
    <td>DS0076</td>
    <td>Error</td>
    <td>Emitted when a character literal is left empty.</td>
    <td><pre>c = '' # DS0076</pre></td>
    <td>Fill the literal with a character.</td>
  </tr>
  <tr>
    <td>DS0077</td>
    <td>Error</td>
    <td>Emitted when attempting to import something other than namespaces or modules.</td>
    <td><pre># DS0077
import System.IO.TextWriter</pre></td>
    <td>Only import namespaces and modules. For types, you can set an alias instead.</td>
  </tr>
  <tr>
    <td>DS0078</td>
    <td>Warning</td>
    <td>Emitted when an access modifier group uses the default modifier for the containing type.</td>
    <td><pre>type Point = {
	global = { # DS0078
		X: int32
		Y: int32
	}
}</pre></td>
    <td>Remove the redundant modifier group.</td>
  </tr>
  <tr>
    <td>DS0079</td>
    <td>Error</td>
    <td>Emitted when an array has more than 32 dimensions.</td>
    <td><pre># DS0079
arr: int@[,,,...,] = ()</pre></td>
    <td>Reduce the amount of dimensions or use an alternative data structure.</td>
  </tr>
  <tr>
    <td>DS0081</td>
    <td>Error</td>
    <td>Emitted when there are problems with the project file of a referenced project.</td>
    <td></td>
    <td>Fix all problems as suggested by the individual error message.</td>
  </tr>
  <tr>
    <td>DS0082</td>
    <td>Warning</td>
    <td>Emitted when a macro that does not exist is used in a project file.</td>
    <td><pre>$(TimeExakt) # DS0082</pre></td>
    <td>Make sure the spelling of the macro is correct, or add a definition for the macro.</td>
  </tr>
  <tr>
    <td>DS0083</td>
    <td>Error</td>
    <td>Emitted when the modifier <code>var</code> is used on a method.</td>
    <td><pre>type Tools = {
	# DS0083
	var Print (msg): null = {
		println msg
	}
}</pre></td>
    <td>Remove the modifier. Methods are never mutable.</td>
  </tr>
  <tr>
    <td>DS0084</td>
    <td>Error</td>
    <td>Emitted when attempting to access <code>this</code> from inside a static method.</td>
    <td><pre>module M = {
	F (): null = {
		this.x = 2 # DS0084
	}
}</pre></td>
    <td>Remove the reference to <code>this</code> or convert the member to an instance method.</td>
  </tr>
  <tr>
    <td>DS0085</td>
    <td>Error</td>
    <td>Emitted when a type symbol cannot be associated with any internal compiler data.</td>
    <td></td>
    <td>This error code indicates a bug in the compiler. If you encounter this error, please open an <a href="https://github.com/loschsoftware/dc/issues">issue</a>.</td>
  </tr>
  <tr>
    <td>DS0087</td>
    <td>Error</td>
    <td>Emitted when calling <code>dc build</code> with a build profile that does not exist.</td>
    <td><pre>dc build Profile01 # DS0087</pre></td>
    <td>Make sure the specified profile exists.</td>
  </tr>
  <tr>
    <td>DS0089</td>
    <td>Warning</td>
    <td>Emitted when a property set in <code>dsconfig.xml</code> does not exist.</td>
    <td><pre lang="xml">&lt;!-- DS0089 --&gt;
&lt;RotNamespace&gt;App&lt;/RotNamespace&gt;</pre></td>
    <td>Fix the spelling of the property.</td>
  </tr>
  <tr>
    <td>DS0090</td>
    <td>Error</td>
    <td>Emitted when a project file is either syntactically or semantically malformed.</td>
    <td><pre lang="xml">&lt;!-- DS0090 --&gt;
&lt;DassieConfig FormatVersion="1,0"/&gt;</pre></td>
    <td>Make sure the project file is syntactically correct XML and the format version is a correct version string.</td>
  </tr>
  <tr>
    <td>DS0091</td>
    <td>Error</td>
    <td>Emitted when a project file uses a newer format than supported by the current compiler version.</td>
    <td><pre lang="xml">&lt;!-- DS0091 --&gt;
&lt;DassieConfig FormatVersion="2.0"/&gt;</pre></td>
    <td>Update the compiler or downgrade to an older project file format.</td>
  </tr>
  <tr>
    <td>DS0092</td>
    <td>Warning</td>
    <td>Emitted when a project file uses an outdated format.</td>
    <td><pre lang="xml">&lt;!-- DS0092 --&gt;
&lt;DassieConfig FormatVersion="0.0"/&gt;</pre></td>
    <td>Update the file format.</td>
  </tr>
  <tr>
    <td>DS0093</td>
    <td>Error</td>
    <td>Emitted when a constructor has a return value.</td>
    <td><pre>type Point = {
	X: int32
	Y: int32
	
  #DS0093
	Point (x: int32, y: int32) = {
		X = x
		Y = y
	}
}</pre></td>
    <td>Remove the return value. Use <code>ignore</code> to discard a value.</td>
  </tr>
  <tr>
    <td>DS0094</td>
    <td>Error</td>
    <td>Emitted when a field marked as read-only using the <code>val</code> keyword is modified outside of a constructor.</td>
    <td><pre>type Point = {
	val X: int32
	val Y: int32
	Point (x: int32, y: int32) = ignore {
		X = x
		Y = y
	}
}
module App = {
	&lt;EntryPoint&gt;
	Main (): int = {
		point = Point 10, 20
		point.X = 30 # DS0094
		0
	}
}</pre></td>
    <td>Remove the assignment or remove the <code>val</code> modifier from the field.</td>
  </tr>
  <tr>
    <td>DS0095</td>
    <td>Error</td>
    <td>Emitted when a an immutable symbol is passed by reference.</td>
    <td><pre>module App = {
    Test (var x: int32&): null = ignore {
        x += 1
    }

    <EntryPoint>
    Main (): null = {
        x = 10
		Test &x # DS0095
    }
}</pre></td>
    <td>Mark the symbol as mutable using the <code>var</code> keyword.</td>
  </tr>
  <tr>
    <td>DS0096</td>
    <td>Error</td>
    <td>Emitted when a symbol is passed by reference without the '&' operator.</td>
    <td><pre>module App = {
    Test (var x: int32&): null = ignore {
        x += 1
    }

    <EntryPoint>
    Main (): null = {
        x = 10
		Test x # DS0096
    }
}</pre></td>
    <td>Use the <code>&amp;</code> to pass the symbol by reference.</td>
  </tr>
  <tr>
    <td>DS0097</td>
    <td>Error</td>
    <td>Emitted when an illegal expression is passed by reference.</td>
    <td><pre>module App = {
    Test (var x: int32&): null = ignore {
        x += 1
    }

    <EntryPoint>
    Main (): null = {
		Test &2 # DS0097
    }
}</pre></td>
    <td>Only assignable symbols can be passed by reference. Assign the value to a local variable before passing it by reference.</td>
  </tr>
  <tr>
    <td>DS0098</td>
    <td>Error</td>
    <td>Emitted when a <i>scratch</i> cannot be found.</td>
    <td></td>
    <td>Check if the name was spelled correctly.</td>
  </tr>
  <tr>
    <td>DS0099</td>
    <td>Warning</td>
    <td>Emitted when multiple installed extensions define a command with the same name.</td>
    <td></td>
    <td>Remove all but one of the duplicate extensions.</td>
  </tr>
</table>

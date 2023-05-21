using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Losch.LoschScript.Configuration;
using LoschScript.Errors;
using LoschScript.Meta;
using LoschScript.Parser;
using LoschScript.Validation;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LoschScript.CodeGeneration;

internal static class FileCompiler
{
    public static ErrorInfo[] CompileSingleFile(string path, LSConfig config)
    {
        Context.Files.Add(new(path));
        CurrentFile = Context.GetFile(path);

        string source = File.ReadAllText(path);

        ICharStream charStream = CharStreams.fromString(source);
        ITokenSource lexer = new LoschScriptLexer(charStream);
        ITokenStream tokens = new CommonTokenStream(lexer);

        LoschScriptParser parser = new(tokens);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new SyntaxErrorListener());

        Reference[] refs = ReferenceValidation.ValidateReferences(config.References);
        var refsToAdd = refs.Where(r => r is AssemblyReference).Select(r => Assembly.LoadFrom((r as AssemblyReference).AssemblyPath));

        if (refsToAdd != null)
            Context.ReferencedAssemblies.AddRange(refsToAdd);

        IParseTree compilationUnit = parser.compilation_unit();
        Visitor v = new();
        v.VisitCompilation_unit((LoschScriptParser.Compilation_unitContext)compilationUnit);

        LogOut.WriteLine($"\r\nCompilation of source file '{path}' {(CurrentFile.Errors.Any() ? "failed" : "successful")}.");
        return CurrentFile.Errors.ToArray();
    }
}
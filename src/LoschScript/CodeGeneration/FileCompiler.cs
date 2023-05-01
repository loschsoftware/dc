using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Losch.LoschScript.Configuration;
using LoschScript.Errors;
using LoschScript.Parser;
using LoschScript.Validation;
using System.IO;
using System.Linq;

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

        ReferenceValidation.ValidateReferences(config.References);

        Listener listener = new();
        IParseTree compilationUnit = parser.compilation_unit();
        ParseTreeWalker.Default?.Walk(listener, compilationUnit);

        LogOut.WriteLine($"Compilation of source file '{path}' {(CurrentFile.Errors.Any() ? "failed" : "successful")}.");
        return CurrentFile.Errors.ToArray();
    }
}
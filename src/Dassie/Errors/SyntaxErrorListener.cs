using Antlr4.Runtime;
using System.IO;

namespace Dassie.Errors;

internal class ParserErrorListener : BaseErrorListener
{
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);

        EmitErrorMessage(new()
        {
            CodePosition = (line, charPositionInLine),
            ErrorCode = DS0001_SyntaxError,
            ErrorMessage = msg,
            File = CurrentFile.Path,
            Severity = Severity.Error
        }, true);
    }
}

internal class LexerErrorListener : IAntlrErrorListener<int>
{
    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        EmitErrorMessage(new()
        {
            CodePosition = (line, charPositionInLine),
            ErrorCode = DS0001_SyntaxError,
            ErrorMessage = msg,
            File = CurrentFile.Path,
            Severity = Severity.Error
        }, true);
    }
}
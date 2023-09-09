using Antlr4.Runtime;
using System.IO;

namespace LoschScript.Errors;

internal class SyntaxErrorListener : BaseErrorListener
{
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);

        EmitErrorMessage(new()
        {
            CodePosition = (line, charPositionInLine),
            ErrorCode = LS0001_SyntaxError,
            ErrorMessage = msg,
            File = CurrentFile.Path,
            Severity = Severity.Error
        }, true);
    }
}
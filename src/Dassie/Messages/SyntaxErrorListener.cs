using Antlr4.Runtime;
using System.IO;

namespace Dassie.Messages;

internal class ParserErrorListener : BaseErrorListener
{
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);

        Emit(new()
        {
            Location = (line, charPositionInLine),
            Length = offendingSymbol.Text.Length,
            Code = DS0002_SyntaxError,
            Text = new([char.ToUpperInvariant(msg[0]), .. msg[1..], '.']),
            File = CurrentFile.Path,
            Severity = Severity.Error
        });
    }
}

internal class LexerErrorListener : IAntlrErrorListener<int>
{
    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Emit(new()
        {
            Location = (line, charPositionInLine),
            Code = DS0002_SyntaxError,
            Text = new([char.ToUpperInvariant(msg[0]), .. msg[1..], '.']),
            File = CurrentFile.Path,
            Severity = Severity.Error
        });
    }
}
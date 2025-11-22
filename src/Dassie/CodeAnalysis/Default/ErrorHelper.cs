using Dassie.Configuration;
using Dassie.Messages;
using System.Linq;

namespace Dassie.CodeAnalysis.Default;

internal class ErrorHelper
{
    public static MessageInfo GetError(AnalysisErrorKind kind, int line, int col, int length, string message, string file = null, Severity severity = Severity.Information, string tip = "", CodeAnalysisConfiguration config = null)
    {
        string code = kind.ToString().Split('_')[0];
        if (config != null && config.MessageConfigurations.Any(c => c.Code == code))
        {
            severity = config.MessageConfigurations.First(c => c.Code == code).Severity switch
            {
                CodeAnalysisConfiguration.Configure.MessageSeverity.Information => Severity.Information,
                CodeAnalysisConfiguration.Configure.MessageSeverity.Warning => Severity.Warning,
                _ => Severity.Error
            };
        }

        return new()
        {
            Code = Custom,
            CustomCode = code,
            Location = (line, col),
            Length = length,
            File = Path.GetFileName(file ?? CurrentFile.Path),
            Text = message,
            Severity = severity,
            Source = MessageSource.Analysis,
            Tip = tip
        };
    }
}
using System;

namespace LoschScript.Shell;

internal class ShellContext
{
    public ShellContext()
    {
        Current = this;
    }

    public static ShellContext Current { get; set; }

    public string CurrentDirectory { get; set; }

    public string PromptFormat { get; set; } = "$dir>";

    public static string FormatPrompt(string format)
    {
        return format
            .Replace("$dir", Current.CurrentDirectory)
            .Replace("$$", "$")
            .Replace("$date", DateTime.Now.ToShortDateString())
            .Replace("$time", DateTime.Now.TimeOfDay.ToString());
    }
}
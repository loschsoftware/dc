using Antlr4.Runtime;
using Dassie.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dassie.CodeAnalysis.Default;

internal class TaskAnalyzer
{
    public record Task(
        string File,
        int Line,
        string FormattedText,
        bool MultiComment);

    private readonly string _basePath;
    private readonly IEnumerable<string> _files;
    private Dictionary<string, int> _markers = new()
    {
        ["NOTE"] = 105,
        ["TODO"] = 178,
        ["HACK"] = 214,
        ["BUG"] = 202,
        ["FIXME"] = 202,
        ["DEPRECATED"] = 229
    };

    public TaskAnalyzer(string projectBasePath)
    {
        _basePath = projectBasePath;
        _files = Directory.EnumerateFiles(projectBasePath, "*.ds", SearchOption.AllDirectories);
    }

    public TaskAnalyzer(IEnumerable<string> files)
    {
        _basePath = Directory.GetCurrentDirectory();
        _files = files;
    }

    public void ConfigureMarkers(Dictionary<string, int> custom, List<string> excluded)
    {
        _markers = _markers.Concat(custom).Where(k => !excluded.Contains(k.Key)).ToDictionary();
    }

    public void PrintAll()
    {
        List<Task> tasks = [];
        foreach (string file in _files)
            tasks.AddRange(GetTasks(file));

        PrintTasks(tasks, _basePath);
    }

    private List<Task> GetTasks(string path)
    {
        List<Task> tasks = [];

        string source = File.ReadAllText(path);
        ICharStream charStream = CharStreams.fromString(source);
        DassieLexer lexer = new(charStream);
        List<IToken> tokens = lexer
            .GetAllTokens()
            .Where(t => t.Type != DassieLexer.Ws && t.Type != DassieLexer.NewLine)
            .ToList();

        int followUpComments = 0;
        foreach ((int i, IToken token) in tokens.Index())
        {
            if (token.Type != DassieLexer.Single_Line_Comment && token.Type != DassieLexer.Delimited_Comment)
                continue;

            if (followUpComments > 0)
            {
                followUpComments--;
                continue;
            }

            string rawText = token.Text;

            string matchedMarker = _markers.Keys.FirstOrDefault(m => rawText.Contains($"{m}:", StringComparison.OrdinalIgnoreCase));
            if (matchedMarker == null)
                continue;

            int nextIndex = i + 1;
            IToken nextToken = token;
            bool multiComment = false;

            while (nextToken != tokens.Last())
            {
                nextToken = tokens[nextIndex++];
                if (nextToken.Type != DassieLexer.Single_Line_Comment && nextToken.Type != DassieLexer.Delimited_Comment)
                    break;

                bool nextCommentHasMarker = _markers.Keys.Any(m => nextToken.Text.Contains($"{m}:", StringComparison.OrdinalIgnoreCase));
                if (nextCommentHasMarker)
                    break;

                multiComment = true;
                rawText += $"{Environment.NewLine}{nextToken.Text}";
                followUpComments++;
            }

            int colorCode = _markers[matchedMarker];
            string text = new([.. "\e[1m", .. rawText.Replace($"{matchedMarker}:", $"\e[38;5;{colorCode}m{matchedMarker}:\e[0m\e[1m"), .. "\e[0m"]);
            tasks.Add(new(path, token.Line, text, multiComment));
        }

        return tasks;
    }

    private static void PrintTasks(List<Task> tasks, string basePath)
    {
        if (tasks == null || tasks.Count == 0)
        {
            Console.WriteLine("No comments with markers found.");
            return;
        }

        foreach ((int i, Task task) in tasks.Index())
            PrintTask(task, basePath, i);
    }

    private static void PrintTask(Task task, string baseDir, int index)
    {
        StringBuilder sb = new();
        sb.AppendLine($"\e[38;5;240m[#{index + 1}] .{Path.DirectorySeparatorChar}{Path.GetRelativePath(baseDir, task.File)}:{task.Line}\e[0m");
        sb.AppendLine($"\t{task.FormattedText.Replace(Environment.NewLine, $"{Environment.NewLine}\t")}");
        sb.AppendLine();
        Console.Write(sb.ToString());
    }
}
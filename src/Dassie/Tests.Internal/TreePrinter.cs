using System;
using System.Collections.Generic;
using System.Text;

namespace Dassie.Tests.Internal;

internal static class TreePrinter
{
    private const string Indent = "    ";
    private const string StructureLine = "│";
    private const string StructureEntry = "├────";
    private const string StructureLast = "└────";

    private const string Square = "■";
    private const string Circle = "⬤";

    private readonly static Dictionary<int, bool> TreeEnds = new()
    {
        [0] = false,
        [1] = false,
        [2] = false,
        [3] = false
    };

    public static void PrintStructure(List<TestProject> projects)
    {
        StringBuilder sb = new();

        foreach (TestProject proj in projects[..^1])
        {
            PrintProject(proj, sb);
            sb.AppendLine();
        }

        PrintProject(projects[^1], sb);
        Console.Write(sb.ToString());
    }

    private static string FormatStructure(int depth, bool end)
    {
        TreeEnds[depth] = end;

        StringBuilder sb = new();
        sb.Append("\e[38;5;239m");

        for (int i = 1; i < depth; i++)
        {
            bool ancestorIsLast = TreeEnds.GetValueOrDefault(i);
            sb.Append($"{(ancestorIsLast ? " " : StructureLine)}{Indent}");
        }

        sb.Append($"{(end ? StructureLast : StructureEntry)}\e[0m");
        return sb.ToString();
    }

    private static string FormatStatus(Status status, string symbol) => $"\e[9{status switch
    {
        Status.Passed => 2,
        Status.Failed => 1,
        Status.InProgress => 4,
        _ => 7
    }}m{symbol}\u001b[0m";

    private static void PrintProject(TestProject project, StringBuilder sb)
    {
        sb.AppendLine($"{FormatStatus(project.Status, Square)} {project.Name}");

        if (project.Modules != null && project.Modules.Count > 0)
        {
            foreach (TestModule mod in project.Modules[..^1])
                PrintModule(mod, sb);

            PrintModule(project.Modules[^1], sb, true);
        }
    }

    private static void PrintModule(TestModule module, StringBuilder sb, bool end = false)
    {
        sb.AppendLine($"{FormatStructure(1, end)}{FormatStatus(module.Status, Square)} {module.Name}");

        if (module.Tests != null && module.Tests.Count > 0)
        {
            foreach (Test test in module.Tests[..^1])
                PrintTest(test, sb);

            PrintTest(module.Tests[^1], sb, true);
        }
    }

    private static void PrintTest(Test test, StringBuilder sb, bool end = false)
    {
        sb.AppendLine($"{FormatStructure(2, end)}{FormatStatus(test.Status, Circle)} {test.Name}");

        if (test.Cases != null && test.Cases.Count > 0)
        {
            foreach (TestCase cs in test.Cases[..^1])
                PrintCase(cs, sb);

            PrintCase(test.Cases[^1], sb, true);
        }
    }

    private static void PrintCase(TestCase cs, StringBuilder sb, bool end = false)
    {
        sb.AppendLine($"{FormatStructure(3, end)}{FormatStatus(cs.Status, Circle)} {cs.Parameters}");
    }
}
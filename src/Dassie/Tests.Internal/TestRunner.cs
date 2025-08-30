using System;
using System.Diagnostics;
using System.Reflection;

namespace Dassie.Tests.Internal;

internal static class TestRunner
{
    public static void RunProject(TestProject project, bool hidePassedTests)
    {
        foreach (TestModule mod in project.Modules)
        {
            int failedCount = 0;

            foreach (Test test in mod.Tests)
            {
                if (!RunTest(mod, test, hidePassedTests))
                    failedCount++;
            }

            if ((!hidePassedTests && mod.Tests.Count > 0) || failedCount > 0)
                Console.WriteLine();

            Console.WriteLine($"\e[1;{(failedCount == 0 ? "32" : "31")}mModule '{mod.Name}': {mod.Tests.Count - failedCount} of {mod.Tests.Count} test{(mod.Tests.Count > 1 ? "s" : "")} passed\e[0m");
            Console.WriteLine();
            Console.WriteLine($"\e[38;5;244m{new string('▬', Console.WindowWidth)}\e[0m");
            Console.WriteLine();
        }

        PrintStructure(project);
    }

    private static void PrintStructure(TestProject project)
    {
        TreePrinter.PrintStructure([project]);
    }

    private static bool RunTest(TestModule mod, Test test, bool hideIfPassed)
    {
        if (test.Method == null)
            return false;

        bool passed = true;

        string stackTrace = "";
        string error = "";
        long timestamp = 0;
        TimeSpan elapsed;

        try
        {
            try
            {
                timestamp = Stopwatch.GetTimestamp();
                test.Method.Invoke(null, []);
            }
            catch (TargetInvocationException tie)
            {
                passed = false;
                stackTrace = tie.InnerException.StackTrace;
                throw tie.InnerException;
            }
        }
        catch (Exception ex)
        {
            error = $"\e[38;5;174m{ex.GetType()}: {ex.Message}{Environment.NewLine}{stackTrace}\e[0m";
        }
        finally
        {
            elapsed = Stopwatch.GetElapsedTime(timestamp);
        }

        if (passed && hideIfPassed)
        {
            test.Status = Status.Passed;
            return true;
        }

        Console.Write($"\e[38;5;244m[{mod.Tests.IndexOf(test) + 1}/{mod.Tests.Count}]\e[0m [{mod.Name}.{test.Name}] \e[38;5;244m({elapsed.TotalMilliseconds} ms)\e[0m ");

        if (passed)
        {
            Console.WriteLine("\e[1;32m✓ Passed\e[0m");
            test.Status = Status.Passed;
        }
        else
        {
            Console.WriteLine("\e[1;31m✘ Failed\e[0m");
            Console.WriteLine(error);
            test.Status = Status.Failed;
        }

        if (hideIfPassed)
        {
            Console.CursorTop--;
            Console.CursorLeft = 0;
        }

        return test.Status == Status.Passed;
    }

    private static void RunCase(TestCase testCase)
    {

    }
}
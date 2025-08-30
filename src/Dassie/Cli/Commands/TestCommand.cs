using Dassie.Extensions;
using Dassie.Tests;
using Dassie.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dassie.Cli.Commands;

internal class TestCommand : ICompilerCommand
{
    private static TestCommand _instance;
    public static TestCommand Instance => _instance ??= new();

    public string Command => "test";

    public string UsageString => "test";

    public string Description => "Runs unit tests defined for the current project or project group.";

    public CommandHelpDetails HelpDetails() => new()
    {
        Description = Description,
        Usage = ["dc test [(-a|--assembly)=<Assembly>] [(-m|--module)=<Module>] [--failed]"],
        Remarks = $"If ran on a project, this command first compiles the project and then collects and runs all unit tests defined in the project or specified module. "
                  + "If ran on a project group, the command collects and runs all unit tests from all projects in the group.",
        Options =
        [
            ("-a|--assembly", "Run tests from the specified assembly."),
            ("-m|--module", "Run tests from the specified test module. Multiple modules can be specified by using the option multiple times."),
            ("--failed", "Only display failed tests")
        ]
    };

    public int Invoke(string[] args)
    {
        string assemblyPath;
        string projectName;

        if (args.Any(a => a.StartsWith("-a=") || a.StartsWith("--assembly=")))
        {
            assemblyPath = string.Join('=', args.First(a => a.StartsWith("-a=") || a.StartsWith("--assembly=")).Split('=')[1..]);

            if (!File.Exists(assemblyPath))
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0247_DCTestAssemblyNotFound,
                    $"The assembly '{assemblyPath}' could not be found.",
                    CompilerExecutableName);

                return -1;
            }

            projectName = Path.GetFileNameWithoutExtension(assemblyPath);
        }
        else
        {
            projectName = Directory.GetCurrentDirectory().Split(Path.DirectorySeparatorChar).Last();
            (int status, assemblyPath, _, _, bool isProjectGroup) = RunCommand.Compile(true);

            if (isProjectGroup)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0249_DCTestProjectGroup,
                    "The command 'dc test' is not yet supported for project groups.",
                    CompilerExecutableName);

                return -1;
            }

            if (status != 0)
                return status;
        }

        bool failed = false;
        Assembly asm = Assembly.LoadFile(Path.GetFullPath(assemblyPath));
        IEnumerable<Type> testModules = [];

        if (args.Any(a => a.StartsWith("-m=") || a.StartsWith("--module=")))
        {
            List<Type> types = [];

            foreach (string module in args.Where(a => a.StartsWith("-m=") || a.StartsWith("--module=")).Select(m => string.Join('=', m.Split('=')[1..])))
            {
                Type type = asm.GetType(module);

                if (type == null)
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0248_DCTestInvalidModule,
                        $"The assembly '{Path.GetFileName(assemblyPath)}' contains no test module named '{module}'.",
                        CompilerExecutableName);

                    failed = true;
                    continue;
                }

                if (type.GetCustomAttribute<TestModuleAttribute>() == null)
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0248_DCTestInvalidModule,
                        $"The type '{module}' in assembly '{Path.GetFileName(assemblyPath)}' is not a valid test module, since it is not marked with the '<TestModule>' attribute.",
                        CompilerExecutableName);

                    failed = true;
                    continue;
                }

                if (!type.IsSealed || !type.IsAbstract)
                {
                    EmitErrorMessage(
                        0, 0, 0,
                        DS0248_DCTestInvalidModule,
                        $"'{module}' from assembly '{Path.GetFileName(assemblyPath)}' cannot be used as a test module since it is not a module. Test modules must not be instantiable.",
                        CompilerExecutableName);

                    failed = true;
                    continue;
                }

                types.Add(type);
            }

            testModules = types;
        }
        else
            testModules = asm.GetTypes().Where(t => t.GetCustomAttribute<TestModuleAttribute>() != null);

        if (!testModules.Any() && !failed)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0246_DCTestNoTestModules,
                $"The assembly '{Path.GetFileName(assemblyPath)}' contains no test modules.",
                CompilerExecutableName);

            return -1;
        }

        if (failed)
            return -1;

        TestProject testProj = new()
        {
            Name = projectName,
            Assembly = asm,
            Modules = testModules.Select(t => new TestModule()
            {
                Name = t.Name,
                Type = t,
                Tests = t.GetMethods().Where(t => t.GetCustomAttribute<TestAttribute>() != null).Select(m => new Test()
                {
                    Name = m.Name,
                    Method = m,
                    Status = Status.Todo
                }).ToList()
            }).ToList()
        };

        TestRunner.RunProject(testProj, args.Any(a => a == "--failed"));
        return 0;
    }
}
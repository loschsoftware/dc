using Dassie.Extensions;
using Dassie.Tests;
using Dassie.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dassie.Core.Commands;

internal class TestCommand : CompilerCommand
{
    private static TestCommand _instance;
    public static TestCommand Instance => _instance ??= new();

    public override string Command => "test";

    public override string Description => StringHelper.TestCommand_Description;

    public override CommandHelpDetails HelpDetails => new()
    {
        Description = Description,
        Usage = ["dc test [(-a|--assembly)=<Assembly>] [(-m|--module)=<Module>] [--failed]"],
        Remarks = StringHelper.TestCommand_Remarks,
        Options =
        [
            ("-a|--assembly", StringHelper.TestCommand_AssemblyOption),
            ("-m|--module", StringHelper.TestCommand_ModuleOption),
            ("--failed", StringHelper.TestCommand_FailedOption)
        ],
        Examples =
        [
            ("dc test", StringHelper.TestCommand_Example1),
            ("dc test --failed", StringHelper.TestCommand_Example2),
            ("dc test -m=MyNamespace.MyTestModule", StringHelper.TestCommand_Example3),
            ("dc test -a=./path/to/assembly.dll", StringHelper.TestCommand_Example4)
        ]
    };

    public override int Invoke(string[] args)
    {
        string assemblyPath;
        string projectName;

        if (args.Any(a => a.StartsWith("-a=") || a.StartsWith("--assembly=")))
        {
            assemblyPath = string.Join('=', args.First(a => a.StartsWith("-a=") || a.StartsWith("--assembly=")).Split('=')[1..]);

            if (!File.Exists(assemblyPath))
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0247_DCTestAssemblyNotFound,
                    nameof(StringHelper.TestCommand_AssemblyNotFound), [assemblyPath],
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
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0249_DCTestProjectGroup,
                    nameof(StringHelper.TestCommand_NotSupportedForProjectGroup), [],
                    CompilerExecutableName);

                return -1;
            }

            if (status != 0)
                return status;
        }

        assemblyPath = Path.GetFullPath(assemblyPath);
        if (!File.Exists(assemblyPath))
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0247_DCTestAssemblyNotFound,
                nameof(StringHelper.TestCommand_AssemblyNotFoundProjectFile), [assemblyPath],
                CompilerExecutableName);

            return -1;
        }

        bool failed = false;
        Assembly asm = Assembly.LoadFile(assemblyPath);
        IEnumerable<Type> testModules = [];

        if (args.Any(a => a.StartsWith("-m=") || a.StartsWith("--module=")))
        {
            List<Type> types = [];

            foreach (string module in args.Where(a => a.StartsWith("-m=") || a.StartsWith("--module=")).Select(m => string.Join('=', m.Split('=')[1..])))
            {
                Type type = asm.GetType(module);

                if (type == null)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0248_DCTestInvalidModule,
                        nameof(StringHelper.TestCommand_TestModuleNotFound), [Path.GetFileName(assemblyPath), module],
                        CompilerExecutableName);

                    failed = true;
                    continue;
                }

                if (type.GetCustomAttribute<TestModuleAttribute>() == null)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0248_DCTestInvalidModule,
                        nameof(StringHelper.TestCommand_TypeNotValidTestModuleMissingAttribute), [module, Path.GetFileName(assemblyPath)],
                        CompilerExecutableName);

                    failed = true;
                    continue;
                }

                if (!type.IsSealed || !type.IsAbstract)
                {
                    EmitErrorMessageFormatted(
                        0, 0, 0,
                        DS0248_DCTestInvalidModule,
                        nameof(StringHelper.TestCommand_TypeNotValidTestModuleNotModule), [module, Path.GetFileName(assemblyPath)],
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
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0246_DCTestNoTestModules,
                nameof(StringHelper.TestCommand_AssemblyContainsNoTestModules), [Path.GetFileName(assemblyPath)],
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
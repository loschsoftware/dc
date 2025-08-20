using Dassie.Configuration;
using Dassie.Configuration.ProjectGroups;
using System;
using System.Linq;

namespace Dassie.Helpers;

internal static class ProjectGroupHelpers
{
    public static (int ExitCode, string Path) GetExecutableProject(DassieConfig config, bool requireExecutable = true)
    {
        if (string.IsNullOrEmpty(config.ProjectGroup.ExecutableComponent))
        {
            if (requireExecutable)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0106_DCRunInsufficientInfo,
                    "Project group does not define an executable project.",
                    CompilerExecutableName);
            }

            return (-1, null);
        }

        Func<Component, bool> predicate = c =>
        {
            string path;

            if (c is Project p)
                path = p.Path;
            else
                path = ((ProjectGroupComponent)c).Path;

            return Path.GetFullPath(path) == Path.GetFullPath(config.ProjectGroup.ExecutableComponent);
        };

        if (!config.ProjectGroup.Components.Any(predicate))
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0210_ProjectGroupExecutableInvalid,
                $"The executable component '{config.ProjectGroup.ExecutableComponent}' could not be found.",
                CompilerExecutableName);

            return (-1, null);
        }

        Component com = config.ProjectGroup.Components.First(predicate);

        if (com is ProjectGroupComponent)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0210_ProjectGroupExecutableInvalid,
                "Currently, project group executables can only be projects.",
                CompilerExecutableName);

            return (-1, null);
        }

        return (0, ((Project)com).Path);
    }
}
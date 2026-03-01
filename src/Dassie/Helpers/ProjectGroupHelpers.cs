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
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0106_DCRunInsufficientInfo,
                    nameof(StringHelper.ProjectGroupHelpers_NoExecutableProject), [],
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
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0210_ProjectGroupExecutableInvalid,
                nameof(StringHelper.ProjectGroupHelpers_ExecutableNotFound), [config.ProjectGroup.ExecutableComponent],
                CompilerExecutableName);

            return (-1, null);
        }

        Component com = config.ProjectGroup.Components.First(predicate);

        if (com is ProjectGroupComponent)
        {
            EmitErrorMessageFormatted(
                0, 0, 0,
                DS0210_ProjectGroupExecutableInvalid,
                nameof(StringHelper.ProjectGroupHelpers_OnlyProjectsAsExecutable), [],
                CompilerExecutableName);

            return (-1, null);
        }

        return (0, ((Project)com).Path);
    }
}
using Dassie.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dassie.Meta;

internal static class AssemblyResolver
{
    private static IEnumerable<string> GetResolverDirectories()
    {
        yield return Path.Combine(ApplicationDataDirectoryPath, SdkDirectoryName);
        yield return Path.Combine(VersionCommand.AssemblyDirectory, SdkDirectoryName);

        if (Environment.GetEnvironmentVariable("DC_PROBE_DIR") is string pd)
        {
            foreach (string dir in pd.Split(Path.PathSeparator).Where(Directory.Exists))
                yield return dir;
        }
    }

    // TODO: This should probably validate the assembly version
    public static string GetAssemblyPath(Assembly assembly)
    {
        string loc = assembly.Location;
        if (!string.IsNullOrEmpty(loc))
            return loc;

        foreach (string dir in GetResolverDirectories())
        {
            loc = Path.Combine(dir, $"{assembly.GetName().Name}.dll");
            if (File.Exists(loc))
                return loc;
        }

        return null;
    }
}
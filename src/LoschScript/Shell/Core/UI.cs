using System;

namespace LoschScript.Shell.Core;

/// <summary>
/// Provides shell commands concering the UI of LShell.
/// </summary>
public static class UI
{
    /// <summary>
    /// Exits the shell with exit code 0.
    /// </summary>
    public static int exit()
    {
        Environment.Exit(0);
        return 0;
    }

    /// <summary>
    /// Exits the shell with the specified exit code.
    /// </summary>
    /// <param name="exitCode">The exit code.</param>
    public static int exit(int exitCode)
    {
        Environment.Exit(exitCode);
        return 0;
    }
}
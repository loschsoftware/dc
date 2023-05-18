using System;

namespace LoschScript.Core;

/// <summary>
/// Provides functionality for interacting with the current process.
/// </summary>
public static class Process
{
    /// <summary>
    /// Exits the current process with the specified exit code.
    /// </summary>
    /// <param name="exitCode">The exit code.</param>
    public static void exit(int exitCode) => Environment.Exit(exitCode);

    /// <summary>
    /// Exits the current process with exit code 0.
    /// </summary>
    public static void exit() => Environment.Exit(0);
}
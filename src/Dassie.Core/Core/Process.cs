using System;

#pragma warning disable IDE1006

namespace Dassie.Core;

/// <summary>
/// Provides functionality for interacting with the currently running process.
/// </summary>
public static class Process
{
    /// <summary>
    /// Ends the current process with exit code zero.
    /// </summary>
    public static void exit() => Environment.Exit(0);

    /// <summary>
    /// Ends the current process with the specified exit code.
    /// </summary>
    /// <param name="code">The exit code.</param>
    public static void exit(int code) => Environment.Exit(code);
}
using System;
using System.Diagnostics;

namespace LoschScript.Core;

/// <summary>
/// Provides a ready to use instance of <see cref="Stopwatch"/> for measuring time. This type is not suitable for exact measurements.
/// </summary>
public static class Timer
{
    private static readonly Stopwatch sw = new();

    /// <summary>
    /// Resets the stopwatch.
    /// </summary>
    public static void Reset()
    {
        sw.Reset();
    }

    /// <summary>
    /// Restarts the stopwatch.
    /// </summary>
    public static void Start()
    {
        sw.Restart();
    }

    /// <summary>
    /// Stops the stopwatch.
    /// </summary>
    public static void Stop()
    {
        sw.Stop();
    }

    /// <summary>
    /// Gets the time measured by the stopwatch.
    /// </summary>
    public static TimeSpan Elapsed => sw.Elapsed;
}
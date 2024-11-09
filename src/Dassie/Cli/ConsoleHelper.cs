using System;
using System.Runtime.InteropServices;

namespace Dassie.Cli;

internal static partial class ConsoleHelper
{
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetStdHandle(int nStdHandle);

    private const int STD_OUTPUT_HANDLE = -11;
    private const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    [LibraryImport("libc")]
    private static partial int isatty(int fd);

    private const int STDOUT_FILENO = 1;
    private const int STDERR_FILENO = 2;

    private static bool? _ansiEscapeSequenceSupport = null;
    public static bool AnsiEscapeSequenceSupported => _ansiEscapeSequenceSupport ??= CheckAnsiEscapeSequenceSupport();

    private static bool CheckAnsiEscapeSequenceSupport()
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            string term = Environment.GetEnvironmentVariable("TERM");
            return isatty(STDOUT_FILENO) != 0
                    && isatty(STDERR_FILENO) != 0
                    && term != null
                    && (term.Contains("xterm") || term.Contains("color") || term.Contains("ansi") || term.Contains("screen") || term.Contains("linux"));
        }

        if (OperatingSystem.IsWindows())
        {
            IntPtr stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(stdOutHandle, out int mode))
                return false;

            if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == ENABLE_VIRTUAL_TERMINAL_PROCESSING)
                return true;

            if (!SetConsoleMode(stdOutHandle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING))
                return false;

            return true;
        }

        // Better safe than sorry
        return false;
    }
}
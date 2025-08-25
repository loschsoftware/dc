using System;
using System.IO;

namespace Dassie.Data;

/// <summary>
/// Helpers to construct instances of <see cref="InputDocument"/> from various data sources.
/// </summary>
internal static class DocumentHelpers
{
    /// <summary>
    /// Creates an instance of <see cref="InputDocument"/> from a file.
    /// </summary>
    /// <param name="path">The path to the file to create a document for.</param>
    /// <returns>An instance of <see cref="InputDocument"/> corresponding to the specified file.</returns>
    public static InputDocument FromFile(string path)
    {
        string text = "";

        try
        {
            FileInfo fi = new(path);
            if (fi.Exists && fi.Length > int.MaxValue)
            {
                EmitErrorMessage(
                    0, 0, 0,
                    DS0240_SourceFileTooLarge,
                    $"The source file '{path}' is too large: The Dassie compiler is limited to input documents smaller than 2 GiB.",
                    path);

                return new("", path);
            }

            text = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0030_FileAccessDenied,
                ex.Message,
                CompilerExecutableName);
        }

        return new InputDocument(text, path);
    }
}
using System;
using System.IO;

namespace Dassie.Data;

/// <summary>
/// Helpers to construct instances of <see cref="Document"/> from various data sources.
/// </summary>
internal static class DocumentHelpers
{
    /// <summary>
    /// Creates an instance of <see cref="Document"/> from a file.
    /// </summary>
    /// <param name="path">The path to the file to create a document for.</param>
    /// <returns>An instance of <see cref="Document"/> corresponding to the specified file.</returns>
    public static Document FromFile(string path)
    {
        string text = "";

        try
        {
            FileInfo fi = new(path);
            if (fi.Exists && fi.Length > int.MaxValue)
            {
                EmitErrorMessageFormatted(
                    0, 0, 0,
                    DS0240_SourceFileTooLarge,
                    nameof(StringHelper.DocumentHelpers_SourceFileTooLarge), [path],
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

        return new Document(text, path, path);
    }
}
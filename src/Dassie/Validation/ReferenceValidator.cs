using Dassie.Configuration;
using Dassie.Meta;
using System.Collections.Generic;

namespace Dassie.Validation;

/// <summary>
/// Checks assembly and file references for validity.
/// </summary>
internal class ReferenceValidator
{
    /// <summary>
    /// Validates the specified array of references.
    /// </summary>
    /// <param name="references">An array containing the references to validate.</param>
    /// <returns>The valid references.</returns>
    public static Reference[] ValidateReferences(Reference[] references)
    {
        if (references == null)
            return [];

        List<Reference> valid = [];

        foreach (Reference reference in references)
        {
            if (reference is AssemblyReference ar && !File.Exists(Path.GetFullPath(Path.Combine(GlobalConfig.RelativePathResolverDirectory, ar.AssemblyPath))))
                EmitErrorMessage(0, 0, 0, DS0022_InvalidAssemblyReference, $"The referenced assembly '{(reference as AssemblyReference).AssemblyPath} does not exist.", ProjectConfigurationFileName);
            else
                valid.Add(reference);

            if (reference is FileReference fr && !File.Exists(Path.GetFullPath(Path.Combine(GlobalConfig.RelativePathResolverDirectory, fr.FileName))))
                EmitErrorMessage(0, 0, 0, DS0023_InvalidFileReference, $"The referenced file '{(reference as FileReference).FileName} does not exist.", ProjectConfigurationFileName);
            else
                valid.Add(reference);
        }

        return valid.ToArray();
    }
}
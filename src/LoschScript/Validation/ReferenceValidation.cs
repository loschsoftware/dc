using Losch.LoschScript.Configuration;
using System.Collections.Generic;
using System.IO;

namespace LoschScript.Validation;

internal class ReferenceValidation
{
    public static Reference[] ValidateReferences(Reference[] references)
    {
        if (references == null)
            return new Reference[] { };

        List<Reference> valid = new();

        foreach (Reference reference in references)
        {
            if (reference is AssemblyReference && !File.Exists((reference as AssemblyReference).AssemblyPath))
                EmitErrorMessage(0, 0, LS0022_InvalidAssemblyReference, $"The referenced assembly '{(reference as AssemblyReference).AssemblyPath} does not exist.", "lsconfig.xml");
            else
                valid.Add(reference);

            if (reference is FileReference && !File.Exists((reference as FileReference).FileName))
                EmitErrorMessage(0, 0, LS0023_InvalidFileReference, $"The referenced file '{(reference as FileReference).FileName} does not exist.", "lsconfig.xml");
            else
                valid.Add(reference);
        }

        return valid.ToArray();
    }
}
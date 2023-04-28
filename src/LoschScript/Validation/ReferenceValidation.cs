using Losch.LoschScript.Configuration;
using System.IO;

namespace LoschScript.Validation;

internal class ReferenceValidation
{
    public static void ValidateReferences(Reference[] references)
    {
        foreach (Reference reference in references)
        {
            if (reference is AssemblyReference && !File.Exists((reference as AssemblyReference).AssemblyPath))
                EmitErrorMessage(0, 0, LS0022_InvalidAssemblyReference, $"The referenced assembly '{(reference as AssemblyReference).AssemblyPath} does not exist.", "lsconfig.xml");

            if (reference is FileReference && !File.Exists((reference as FileReference).FileName))
                EmitErrorMessage(0, 0, LS0023_InvalidFileReference, $"The referenced file '{(reference as FileReference).FileName} does not exist.", "lsconfig.xml");
        }
    }
}
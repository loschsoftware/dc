using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RedFlag.Symbols
{
    class PESignature
    {
        /// <summary>
        /// Fetch the PDB signature for the exe.
        /// Return null if there is a failure because the file doesn't exist or if the
        /// there is no signature.
        /// </summary>
        public static PdbSignature GetPdbSignatureForPeFile(string file, out bool x86)
        {
            bool x86Result = false;
            PdbSignature result =
                GetSignatureFromFile(file,
                                     delegate(Stream peStream)
                                     {
                                         Pe32 peFile = new Pe32(peStream);
                                         x86Result = !peFile.Is64Bit;
                                         return peFile.Signature;
                                     });
            x86 = x86Result;
            return result;
        }
        private delegate PdbSignature GetSignatureDelegate(Stream input);
        private static PdbSignature GetSignatureFromFile(string file, GetSignatureDelegate action)
        {
            if (File.Exists(file))
            {
                using (FileStream peStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        PdbSignature signature = action(peStream);
                        return signature;
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
            return null;
        }
    }
}

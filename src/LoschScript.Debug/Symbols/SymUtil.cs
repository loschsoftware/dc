using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;
using System.Runtime.InteropServices;

namespace RedFlag
{
    [Flags]
    public enum PdbFailureCode : uint
    {
        E_PDB_OK = 0x806d0001,
        E_PDB_USAGE = 0x806d0002,
        E_PDB_OUT_OF_MEMORY = 0x806d0003,
        E_PDB_FILE_SYSTEM = 0x806d0004,
        E_PDB_NOT_FOUND = 0x806d0005,
        E_PDB_INVALID_SIG = 0x806d0006,
        E_PDB_INVALID_AGE = 0x806d0007,
        E_PDB_PRECOMP_REQUIRED = 0x806d0008,
        E_PDB_OUT_OF_TI = 0x806d0009,
        E_PDB_NOT_IMPLEMENTED = 0x806d0010,
        E_PDB_V1_PDB = 0x806d0011,
        E_PDB_FORMAT = 0x806d0012,
        E_PDB_LIMIT = 0x806d0013,
        E_PDB_CORRUPT = 0x806d0014,
        E_PDB_TI16 = 0x806d0015,
        E_PDB_ACCESS_DENIED = 0x806d0016,
        E_PDB_ILLEGAL_TYPE_EDIT = 0x806d0017,
        E_PDB_INVALID_EXECUTABLE = 0x806d0018,
        E_PDB_DBG_NOT_FOUND = 0x806d0019,
        E_PDB_NO_DEBUG_INFO = 0x806d0020,
        E_PDB_INVALID_EXE_TIMESTAMP = 0x806d0021,
        E_PDB_RESERVED = 0x806d0022,
        E_PDB_DEBUG_INFO_NOT_IN_PDB = 0x806d0023,
        E_PDB_SYMSRV_BAD_CACHE_PATH = 0x806d0024,
        E_PDB_SYMSRV_CACHE_FULL = 0x806d0025,
        E_PDB_MAX = 0x806d0026
    }
}

namespace RedFlag.Symbols
{
    #region Get a symbol reader for the given module
    // Encapsulate a set of helper classes to get a symbol reader from a file.
    // The symbol interfaces require an unmanaged metadata interface.
    public static class SymUtil
    {
        static class NativeMethods
        {
            [DllImport("ole32.dll")]
            public static extern int CoCreateInstance([In] ref Guid rclsid,
                                                       [In, MarshalAs(UnmanagedType.IUnknown)] Object pUnkOuter,
                                                       [In] uint dwClsContext,
                                                       [In] ref Guid riid,
                                                       [Out, MarshalAs(UnmanagedType.Interface)] out Object ppv);
        }

        // Wrapper.
        public static ISymbolReader GetSymbolReaderForFile(string pathModule, string searchPath)
        {
            return SymUtil.GetSymbolReaderForFile(new SymbolBinder(), pathModule, searchPath);
        }

        // We demand Unmanaged code permissions because we're reading from the file system and calling out to the Symbol Reader
        // @TODO - make this more specific.
        [System.Security.Permissions.SecurityPermission(
            System.Security.Permissions.SecurityAction.Demand,
            Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
        public static ISymbolReader GetSymbolReaderForFile(SymbolBinder binder, string pathModule, string searchPath)
        {
            // Guids for imported metadata interfaces.
            Guid dispenserClassID = new Guid(0xe5cb7a31, 0x7512, 0x11d2, 0x89, 0xce, 0x00, 0x80, 0xc7, 0x92, 0xe5, 0xd8); // CLSID_CorMetaDataDispenser
            Guid dispenserIID = new Guid(0x809c652e, 0x7396, 0x11d2, 0x97, 0x71, 0x00, 0xa0, 0xc9, 0xb4, 0xd5, 0x0c); // IID_IMetaDataDispenser
            Guid importerIID = new Guid(0x7dac8207, 0xd3ae, 0x4c75, 0x9b, 0x67, 0x92, 0x80, 0x1a, 0x49, 0x7d, 0x44); // IID_IMetaDataImport

            // First create the Metadata dispenser.
            object objDispenser;
            NativeMethods.CoCreateInstance(ref dispenserClassID, null, 1, ref dispenserIID, out objDispenser);

            // Now open an Importer on the given filename. We'll end up passing this importer straight
            // through to the Binder.
            object objImporter;
            IMetaDataDispenser dispenser = (IMetaDataDispenser)objDispenser;
            dispenser.OpenScope(pathModule, 0, ref importerIID, out objImporter);

            IntPtr importerPtr = IntPtr.Zero;
            ISymbolReader reader;
            try
            {
                // This will manually AddRef the underlying object, so we need to be very careful to Release it.
                importerPtr = Marshal.GetComInterfaceForObject(objImporter, typeof(IMetadataImport));

                reader = binder.GetReader(importerPtr, pathModule, searchPath);
            }
            catch (COMException)
            {
                throw;
            }
            finally
            {
                if (importerPtr != IntPtr.Zero)
                {
                    Marshal.Release(importerPtr);
                }
            }
            return reader;
        }
        /// <summary>
        /// Attempt to work out why the PDB failed. If DIA doesn't give us the name of the last PDB
        /// we'll fudge it and look for the PDB in the same folder as the EXE or DLL. Not perfect,
        /// but much better than nothing.
        /// </summary>
        /// <param name="modFile"></param>
        /// <returns></returns>
        public static uint GetLastReaderFailureCode(string modFile)
        {
            object objDiaDataSource;
            Guid IID_IDiaDataSource = new Guid("79F1BB5F-B66E-48e5-B6A9-1545C323CA3D");
            Guid CLSID_DiaSource = new Guid("B86AE24D-BF2F-4ac9-B5A2-34B14E4CE11D");
            NativeMethods.CoCreateInstance(ref CLSID_DiaSource, null, 1, ref IID_IDiaDataSource, out objDiaDataSource);
            IDiaDataSource ds = (IDiaDataSource)objDiaDataSource;
            // You will only have this object if you have msdia80.dll (vcruntime installed?)
            if (ds == null) return (uint)PdbFailureCode.E_PDB_NOT_IMPLEMENTED; // the interface does not exist (no dll?)
            string pdbPath=null;
            // Using some modified Reflector code, try to get the sig and age from the module
            // so we can verify the PDB
            bool isX86;
            PdbSignature moduleSig = PESignature.GetPdbSignatureForPeFile(modFile, out isX86);
            if (moduleSig == null) return (uint) PdbFailureCode.E_PDB_INVALID_EXECUTABLE;
            Guid sigGuid = moduleSig.Guid;
            uint hr2 = 0;
            string modPdb = modFile.Substring(0, modFile.LastIndexOf('.')) + ".pdb";
            uint hr = (uint) ds.loadAndValidateDataFromPdb(modPdb, ref sigGuid, Convert.ToInt32(moduleSig.Signature), Convert.ToInt32(moduleSig.Age));          
            // This is SUPPOSED to return the PDB path. But it does not.
            //if (hr==0) hr = (uint) ds.get_lastError(out pdbPath); // try once again to get the error
            Marshal.ReleaseComObject(ds); // Otherwise we lock the file.
            if (pdbPath != null) modPdb = pdbPath; // We actually know the name of the corrupt PDB
            if (hr==0 || hr!=(uint)PdbFailureCode.E_PDB_NOT_FOUND)
            {
                PdbFile pdbFile = new PdbFile(new System.IO.FileStream(modPdb, System.IO.FileMode.Open));
                // is it the age or the sig?
                if (!moduleSig.Age.Equals(pdbFile.Signature.Age)) hr2 = hr2 | (uint) PdbFailureCode.E_PDB_INVALID_AGE;
                if (!moduleSig.Guid.Equals(pdbFile.Signature.Guid)) hr2 = hr2 | (uint) PdbFailureCode.E_PDB_INVALID_SIG;
            }
            if (hr2 == 0) return (uint) hr; // validate didn't find a problem.
            else return hr2;
        }
    }
    #region Metadata Imports
        [Guid("79F1BB5F-B66E-48e5-B6A9-1545C323CA3D"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
       
        interface IDiaDataSource
        {
            [PreserveSig]
            int loadDataFromPdb([In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR")] string pdbPath);
            [PreserveSig] int loadAndValidateDataFromPdb([In, ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR")] string pdbPath, [In] ref Guid pcsig70, Int32 sig, Int32 age);
            [PreserveSig]
            int get_lastError([Out,MarshalAs(UnmanagedType.BStr)] out string pdbPath);
        }
    // We can use reflection-only load context to use reflection to query for metadata information rather
    // than painfully import the com-classic metadata interfaces.
    [Guid("809c652e-7396-11d2-9771-00a0c9b4d50c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    interface IMetaDataDispenser
    {
        // We need to be able to call OpenScope, which is the 2nd vtable slot.
        // Thus we need this one placeholder here to occupy the first slot..
        void DefineScope_Placeholder();

        //STDMETHOD(OpenScope)(                   // Return code.
        //LPCWSTR     szScope,                // [in] The scope to open.
        //  DWORD       dwOpenFlags,            // [in] Open mode flags.
        //  REFIID      riid,                   // [in] The interface desired.
        //  IUnknown    **ppIUnk) PURE;         // [out] Return interface on success.
        void OpenScope([In, MarshalAs(UnmanagedType.LPWStr)] String szScope, [In] Int32 dwOpenFlags, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.IUnknown)] out Object punk);

        // Don't need any other methods.
    }

    // Since we're just blindly passing this interface through managed code to the Symbinder, we don't care about actually
    // importing the specific methods.
    // This needs to be public so that we can call Marshal.GetComInterfaceForObject() on it to get the
    // underlying metadata pointer.
    [Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    [CLSCompliant(true)]
    public interface IMetadataImport
    {
        // Just need a single placeholder method so that it doesn't complain about an empty interface.
        void Placeholder();
    }
    #endregion

    #endregion Get a symbol reader for the given module


}

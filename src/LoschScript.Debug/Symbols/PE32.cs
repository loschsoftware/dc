using System;
using System.Collections.Generic;
using System.IO;

namespace RedFlag.Symbols
{
        /// <summary>
        /// Interface implemented by objects that can read from PE files
        /// </summary>
        internal interface IPeFile
        {
            /// <summary>
            /// The signature of this file
            /// </summary>
            PdbSignature Signature { get; }

            /// <summary>
            /// Determine whether we are looking at a 64 bit assembly.
            /// </summary>
            bool Is64Bit { get; }
        }

    /// <summary>
    /// Class for reading from 32-bit PE files
    /// </summary>
    public sealed class Pe32 : IPeFile
    {
        #region File structure

        //typedef struct _IMAGE_DOS_HEADER {      // DOS .EXE header
        //    WORD   e_magic;                     // Magic number				== 0x4D5A, 'MZ'
        //    WORD   e_cblp;                      // Bytes on last page of file			2
        //    WORD   e_cp;                        // Pages in file						4
        //    WORD   e_crlc;                      // Relocations						6
        //    WORD   e_cparhdr;                   // Size of header in paragraphs		8
        //    WORD   e_minalloc;                  // Minimum extra paragraphs needed	10
        //    WORD   e_maxalloc;                  // Maximum extra paragraphs needed	12
        //    WORD   e_ss;                        // Initial (relative) SS value		14
        //    WORD   e_sp;                        // Initial SP value					16
        //    WORD   e_csum;                      // Checksum							18
        //    WORD   e_ip;                        // Initial IP value					20
        //    WORD   e_cs;                        // Initial (relative) CS value		22
        //    WORD   e_lfarlc;                    // File address of relocation table	24
        //    WORD   e_ovno;                      // Overlay number						26
        //    WORD   e_res[4];                    // Reserved words						28
        //    WORD   e_oemid;                     // OEM identifier (for e_oeminfo)		36
        //    WORD   e_oeminfo;                   // OEM information; e_oemid specific	38
        //    WORD   e_res2[10];                  // Reserved words						40
        //    LONG   e_lfanew;                    // File address of new exe header		60
        //  } IMAGE_DOS_HEADER, *PIMAGE_DOS_HEADER;										64

        //typedef struct _IMAGE_FILE_HEADER {
        //    WORD    Machine;															0
        //    WORD    NumberOfSections;													2
        //    DWORD   TimeDateStamp;													4
        //    DWORD   PointerToSymbolTable;												8
        //    DWORD   NumberOfSymbols;													12
        //    WORD    SizeOfOptionalHeader;												16
        //    WORD    Characteristics;													18
        //} IMAGE_FILE_HEADER, *PIMAGE_FILE_HEADER;										20

        //typedef struct _IMAGE_DATA_DIRECTORY {
        //    DWORD   VirtualAddress;													0
        //    DWORD   Size;																4
        //} IMAGE_DATA_DIRECTORY, *PIMAGE_DATA_DIRECTORY;								8

        //typedef struct _IMAGE_OPTIONAL_HEADER {
        //    //
        //    // Standard fields.
        //    //

        //    WORD    Magic;															0
        //    BYTE    MajorLinkerVersion;												2
        //    BYTE    MinorLinkerVersion;												3
        //    DWORD   SizeOfCode;														4
        //    DWORD   SizeOfInitializedData;											8
        //    DWORD   SizeOfUninitializedData;											12
        //    DWORD   AddressOfEntryPoint;												16
        //    DWORD   BaseOfCode;														20
        //    DWORD   BaseOfData;														24

        //    //
        //    // NT additional fields.
        //    //

        //    DWORD   ImageBase;														28
        //    DWORD   SectionAlignment;													32
        //    DWORD   FileAlignment;													36
        //    WORD    MajorOperatingSystemVersion;										40
        //    WORD    MinorOperatingSystemVersion;										42
        //    WORD    MajorImageVersion;												44
        //    WORD    MinorImageVersion;												46
        //    WORD    MajorSubsystemVersion;											48
        //    WORD    MinorSubsystemVersion;											50
        //    DWORD   Win32VersionValue;												52
        //    DWORD   SizeOfImage;														56
        //    DWORD   SizeOfHeaders;													60
        //    DWORD   CheckSum;															64
        //    WORD    Subsystem;														68
        //    WORD    DllCharacteristics;												70
        //    DWORD   SizeOfStackReserve;												72
        //    DWORD   SizeOfStackCommit;												76
        //    DWORD   SizeOfHeapReserve;												80
        //    DWORD   SizeOfHeapCommit;													84
        //    DWORD   LoaderFlags;														88
        //    DWORD   NumberOfRvaAndSizes;												92
        //    IMAGE_DATA_DIRECTORY DataDirectory[IMAGE_NUMBEROF_DIRECTORY_ENTRIES];		96	(IMAGE_NUMBEROF_DIRECTORY_ENTRIES == 16)
        //} IMAGE_OPTIONAL_HEADER32, *PIMAGE_OPTIONAL_HEADER32;							224

        //typedef struct _IMAGE_NT_HEADERS {
        //    DWORD Signature;															0
        //    IMAGE_FILE_HEADER FileHeader;												4
        //    IMAGE_OPTIONAL_HEADER32 OptionalHeader;									24
        //} IMAGE_NT_HEADERS32, *PIMAGE_NT_HEADERS32;									248

        //typedef struct _IMAGE_SECTION_HEADER {
        //    BYTE    Name[IMAGE_SIZEOF_SHORT_NAME];									0
        //    union {																	8
        //            DWORD   PhysicalAddress;
        //            DWORD   VirtualSize;
        //    } Misc;
        //    DWORD   VirtualAddress;													12
        //    DWORD   SizeOfRawData;													16
        //    DWORD   PointerToRawData;													20
        //    DWORD   PointerToRelocations;												24
        //    DWORD   PointerToLinenumbers;												28
        //    WORD    NumberOfRelocations;												32
        //    WORD    NumberOfLinenumbers;												34
        //    DWORD   Characteristics;													36
        //} IMAGE_SECTION_HEADER, *PIMAGE_SECTION_HEADER;								40

        //typedef struct _IMAGE_DEBUG_DIRECTORY {
        //    DWORD   Characteristics;													0
        //    DWORD   TimeDateStamp;													4
        //    WORD    MajorVersion;														8
        //    WORD    MinorVersion;														10
        //    DWORD   Type;																12
        //    DWORD   SizeOfData;														16
        //    DWORD   AddressOfRawData;													20
        //    DWORD   PointerToRawData;													24
        //} IMAGE_DEBUG_DIRECTORY, *PIMAGE_DEBUG_DIRECTORY;								28

        /// <summary>
        /// Position of the 'MZ' magic word relative to the beginning of the file
        /// </summary>
        const int c_Magic = 0;

        /// <summary>
        /// Position of the new EXE header word
        /// </summary>
        const int c_LfaNew = 60;

        /// <summary>
        /// Position of the signature word within the IMAGE_NT_HEADERS structure
        /// </summary>
        const int c_NtSignature = 0;

        /// <summary>
        /// Position of the FileHeader structure within the IMAGE_NT_HEADERS structure
        /// </summary>
        const int c_FileHeader = 4;

        /// <summary>
        /// Position of the 'FileHeader.Machine' word within the IMAGE_NT_HEADERS structure
        /// </summary>
        const int c_NtMachine = c_FileHeader + 0;

        /// <summary>
        /// Position of the 'FileHeader.NumberOfSections' word within the IMAGE_NT_HEADERS structure
        /// </summary>
        const int c_NtNumberOfSections = c_FileHeader + 2;

        /// <summary>
        /// Position of the 'FileHeader.SizeOfOptionalHeader' block
        /// </summary>
        const int c_NtOptionalSize = c_FileHeader + 16;

        /// <summary>
        /// Position of the optional file header within the IMAGE_NT_HEADERS structure
        /// </summary>
        const int c_OptionalHeader = c_FileHeader + 20;

        /// <summary>
        /// Position of the first data directory entry within the IMAGE_NT_HEADERS optional header structure
        /// </summary>
        const int c_DataDirectory_32 = c_OptionalHeader + 96;
        const int c_DataDirectory_64 = c_OptionalHeader + 112;

        /// <summary>
        /// Number of bytes in a data directory entry
        /// </summary>
        const int c_DataDirectoryEntrySize = 8;

        /// <summary>
        /// The offset into a section of the virtual address value
        /// </summary>
        const int c_SectionVirtualAddress = 12;

        /// <summary>
        /// The offset into a section of the length of this section within the file
        /// </summary>
        const int c_SectionRawDataLength = 16;

        /// <summary>
        /// The offset into a section of the offset into the file
        /// </summary>
        const int c_SectionRawData = 20;

        /// <summary>
        /// Number of bytes in a section
        /// </summary>
        const int c_SectionSize = 40;

        /// <summary>
        /// Size of a debug directory entry
        /// </summary>
        const int c_DebugEntrySize = 28;

        /// <summary>
        /// Offset into a debug directory entry of the type word
        /// </summary>
        const int c_DebugEntryType = 12;

        /// <summary>
        /// Offset into a debug directory entry of the data size word
        /// </summary>
        const int c_DebugEntryDataSize = 16;

        /// <summary>
        /// Offset into a debug directory entry of the RVA of the data for this entry
        /// </summary>
        const int c_DebugEntryDataAddress = 20;

        /// <summary>
        /// The signature word that indicates a PE file when put in IMAGE_NT_HEADER
        /// </summary>
        const uint c_PeSignature = 0x00004550;

        /// <summary>
        /// Constant machine value indicating I386
        /// </summary>
        const ushort c_MachineI386 = 0x014c;

        /// <summary>
        /// constant machine value indicating X64
        /// </summary>
        const ushort c_MachineAMD64 = 0x8664;

        #endregion

        #region Internal classes

        /// <summary>
        /// Represents a section within a PE file
        /// </summary>
        class Section
        {
            /// <summary>
            /// The RVA where this will be loaded in
            /// </summary>
            public readonly uint VirtualAddress;

            /// <summary>
            /// The offset into the file where this section starts
            /// </summary>
            public readonly uint FileOffset;

            /// <summary>
            /// The length of this section
            /// </summary>
            public readonly uint Length;

            public Section(uint virtualAddress, uint fileOffset, uint length)
            {
                VirtualAddress = virtualAddress;
                FileOffset = fileOffset;
                Length = length;
            }
        }

        #endregion

        #region Member variables

        /// <summary>
        /// Are we looking at a 32-bit PE file?
        /// </summary>
        private readonly bool m_32Bit = true;

        /// <summary>
        /// The stream that contains the file
        /// </summary>
        private readonly Stream m_FileStream;

        /// <summary>
        /// Position of the NT headers structure
        /// </summary>
        private readonly uint m_LfaNew;

        /// <summary>
        /// The sections that are defined in the file
        /// </summary>
        private readonly List<Section> m_Sections = new List<Section>();

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialises a new object that can read from 32-bit PE files
        /// </summary>
        /// <param name="fileStream">The stream to read from. This must support random access to be of any use at all.</param>
        public Pe32(Stream fileStream)
        {
            if (fileStream == null) throw new ArgumentNullException("fileStream");
            m_FileStream = fileStream;

            // Sanity check the file
            if (m_FileStream.Length < 224)
            {
                throw new InvalidOperationException("Stream is too short to be a PE file");
            }
            if (m_FileStream.ReadWord(c_Magic) != 0x5A4D)				// First word must be 'MZ'
            {
                throw new InvalidOperationException("This is not a PE file");
            }

            // Read the location of the NT header
            m_LfaNew = m_FileStream.ReadLong(c_LfaNew);
            if (m_LfaNew < 64 || m_LfaNew > m_FileStream.Length - 224)
            {
                throw new InvalidOperationException("This is not a PE file");
            }

            // Read the signature from the NT block: must be 'PE' for this to be a PE file
            ulong signature = m_FileStream.ReadLong(m_LfaNew + c_NtSignature);
            if (signature != c_PeSignature)
            {
                throw new InvalidOperationException("This is not a PE file");
            }

            // Read the machine word: must be IMAGE_FILE_MACHINE_I386 to be a 32-bit PE file
            ushort machine = m_FileStream.ReadWord(m_LfaNew + c_NtMachine);
            if (machine != c_MachineI386)							// Only support 32-bit PE files
            {
                if (machine == c_MachineAMD64)
                {
                    m_32Bit = false;
                }
                else
                {
                    throw new InvalidOperationException("Unknown type of machine in PE file");
                }
            }

            // Read in the size of the optional header
            ushort optionalHeaderSize = m_FileStream.ReadWord(m_LfaNew + c_NtOptionalSize);
            if (optionalHeaderSize < 224)
            {
                throw new InvalidOperationException("PE file optional header is the wrong size");
            }

            // Read in the header sections (we need these to convert RVAs later on)
            ushort numSections = m_FileStream.ReadWord(m_LfaNew + c_NtNumberOfSections);
            long firstSection = m_LfaNew + c_OptionalHeader + optionalHeaderSize;
            for (int x = 0; x < numSections; x++)
            {
                // Read the addresses of this section
                long sectionAddress = firstSection + x * c_SectionSize;

                uint virtualAddress = m_FileStream.ReadLong(sectionAddress + c_SectionVirtualAddress);
                uint fileOffset = m_FileStream.ReadLong(sectionAddress + c_SectionRawData);
                uint length = m_FileStream.ReadLong(sectionAddress + c_SectionRawDataLength);

                // Add it to the list
                m_Sections.Add(new Section(virtualAddress, fileOffset, length));
            }
        }

        #endregion

        #region Reading parts of the file

        /// <summary>
        /// Finds the file offset for the specified RVA
        /// </summary>
        private long OffsetForRVA(uint rva)
        {
            foreach (Section sec in m_Sections)
            {
                if (rva >= sec.VirtualAddress && rva < (sec.VirtualAddress + sec.Length))
                {
                    return sec.FileOffset + (rva - sec.VirtualAddress);
                }
            }
            throw new InvalidOperationException("Tried to retrieve the offset for an RVA that is outside the PE file");
        }

        #endregion

        #region IPeFile implementation

        /// <summary>
        /// The signature of this file
        /// </summary>
        public PdbSignature Signature
        {
            get
            {
                // The constructor verifies that we've got a 32-bit PE file, so all we need to do here is actually read out the debugger
                // signature.

                // Entry index 6 contains the offset of the DBEUG_DIRECTORY structure
                long entryPos = m_LfaNew + (m_32Bit ? c_DataDirectory_32 : c_DataDirectory_64) + c_DataDirectoryEntrySize * 6;

                // Read the location of the debug directory
                uint virtualAddress = m_FileStream.ReadLong(entryPos);
                uint size = m_FileStream.ReadLong(entryPos + 4);

                // Sanity check it
                if (virtualAddress == 0 || size == 0 || size > 0x10000)
                {
                    throw new InvalidOperationException("PE file does not contain debugger signature");
                }

                if (size < 28)
                {
                    throw new InvalidOperationException("Debug directory is not in a recognised format");
                }

                // Work out the position of the debug directory
                long debugDirPos = OffsetForRVA(virtualAddress);
                uint dataRva = 0xffffffff;
                uint dataSize = 0xffffffff;

                // Find the directory entry containing a CodeView block
                for (int x = 0; x + c_DataDirectoryEntrySize <= size; x += c_DataDirectoryEntrySize)
                {
                    // Get the debug entry offset
                    long thisEntryPos = debugDirPos + x;

                    // Read the type
                    uint entryType = m_FileStream.ReadLong(thisEntryPos + c_DebugEntryType);
                    if (entryType != 2) continue;

                    // Read this as the final entry
                    dataRva = m_FileStream.ReadLong(thisEntryPos + c_DebugEntryDataAddress);
                    dataSize = m_FileStream.ReadLong(thisEntryPos + c_DebugEntryDataSize);
                    break;
                }

                // Blow up if we could't find the location of this entry
                if (dataRva == 0xffffffff || dataSize > 0x100000)
                {
                    throw new InvalidOperationException("Could not locate data entry for PDB debug data");
                }

                // Read the data for this PDB entry
                byte[] data = new byte[dataSize];
                m_FileStream.Position = OffsetForRVA(dataRva);
                m_FileStream.Read(data, 0, (int)dataSize);

                // Generate the signature
                return new PdbSignature(data);
            }
        }

        /// <summary>
        /// Return true if the assembly is 64bit.
        /// </summary>
        public bool Is64Bit
        {
            get
            {
                return !m_32Bit;
            }
        }

        #endregion
    }
    /// <summary>
    /// Some handy routines for applying to streams
    /// </summary>
    internal static class StreamUtils
    {
        /// <summary>
        /// Reads a 16-bit word from the stream
        /// </summary>
        internal static ushort ReadWord(this Stream stream, long pos)
        {
            // Seek the position
            stream.Position = pos;

            // Read 2 bytes
            byte[] twoBytes = new byte[2];
            stream.Read(twoBytes, 0, 2);

            // Generate the word
            return (ushort)((((ushort)twoBytes[1]) << 8) | ((ushort)twoBytes[0]));
        }

        /// <summary>
        /// Reads a 32-bit word from the stream
        /// </summary>
        internal static uint ReadLong(this Stream stream, long pos)
        {
            // Seek the position
            stream.Position = pos;

            // Read 4 bytes
            byte[] fourBytes = new byte[4];
            stream.Read(fourBytes, 0, 4);

            // Generate the word
            return (uint)((((uint)fourBytes[3]) << 24) | (((uint)fourBytes[2]) << 16) | (((uint)fourBytes[1]) << 8) | ((uint)fourBytes[0]));
        }
    }
}

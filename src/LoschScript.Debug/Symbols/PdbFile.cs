using System;
using System.Collections.Generic;
using System.IO;

namespace RedFlag.Symbols
{
    /// <summary>
    /// Class for handling PDB files. We need to open and read these so we can change the signature so that Visual Studio will recognise
    /// a PDB file as belonging to a particular application.
    /// </summary>
    /// <remarks>
    /// Handles v7 PDB files, these should be the ones that we end up generating (the older v2 format seems to only apply to Visual Studio
    /// 6 and earlier, which predates .NET)
    /// </remarks>
    public sealed class PdbFile
    {
        #region File structure

        #region File header

        /// <summary>
        /// The header bytes of a PDB v7 file
        /// </summary>
        private static readonly char[] s_HeaderBytes = { 'M', 'i', 'c', 'r', 'o', 's', 'o', 'f', 't', ' ', 'C', '/', 'C', '+', '+', ' ', 'M', 'S', 'F', ' ', '7', '.', '0', '0', (char)13, (char)10, (char)0x1a, 'D', 'S', (char)0, (char)0, (char)0 };

        /// <summary>
        /// The number of bytes in the initial header. Contains 'Microsoft C/C++ MSF 7.00\r\n\x1ADS\0\0\0' by default
        /// </summary>
        private const int c_HeaderSize = 0x20;

        /// <summary>
        /// The dword within the header that refers to the size of a page (32-bits)
        /// </summary>
        private const int c_PageBytes = c_HeaderSize + 0;

        /// <summary>
        /// The dword within the header that contains the page number containing the allocation table
        /// </summary>
        private const int c_FlagPage = c_PageBytes + 4;

        /// <summary>
        /// The dword within the header that contains the number of pages within the file
        /// </summary>
        private const int c_FilePages = c_FlagPage + 4;

        /// <summary>
        /// The dword within the header that contains the size of the root stream in bytes
        /// </summary>
        private const int c_RootBytes = c_FilePages + 4;

        /// <summary>
        /// Reserved dword, not used for anything
        /// </summary>
        private const int c_HeaderReserved1 = c_RootBytes + 4;

        /// <summary>
        /// Array of dwords containing the page numbers of the root stream index
        /// </summary>
        private const int c_IndexPages = c_HeaderReserved1 + 4;

        #endregion

        // Root stream structure is a single dword giving the total number of streams. This is followed by the length of each stream, and
        // then by lists of the pages occupied by each stream in turn (you can calculate the number of pages by dividing the length by the
        // page size and rounding up)

        #endregion

        #region Member variables

        /// <summary>
        /// The random-access stream that this should read
        /// </summary>
        private readonly Stream m_PdbFile;

        /// <summary>
        /// The page size of this file
        /// </summary>
        private readonly int m_PageSize;

        /// <summary>
        /// The list of root pages
        /// </summary>
        private readonly List<int> m_RootPages = new List<int>();

        /// <summary>
        /// List of page sizes
        /// </summary>
        private readonly List<int> m_PageSizes = new List<int>();

        /// <summary>
        /// List of the pages mapped to each stream
        /// </summary>
        private readonly List<List<int>> m_StreamPages = new List<List<int>>();

        #endregion

        #region Initialisation

        public PdbFile(Stream pdbFile)
        {
            if (pdbFile == null) throw new ArgumentNullException("pdbFile");
            m_PdbFile = pdbFile;

            // Check that the header matches a PDB v7 file
            if (pdbFile.Length < c_IndexPages + 4) throw new InvalidOperationException("This doesn't appear to be a PDB file");

            byte[] headerBytes = new byte[c_HeaderSize];
            pdbFile.Position = 0;
            pdbFile.Read(headerBytes, 0, c_HeaderSize);

            for (int x = 0; x < c_HeaderSize; x++)
            {
                if (s_HeaderBytes[x] != headerBytes[x])
                {
                    throw new InvalidOperationException("This doesn't appear to be a PDB file");
                }
            }

            // Read the values from the header
            m_PageSize = (int)m_PdbFile.ReadLong(c_PageBytes);
            // int flagPage	= (int) m_PdbFile.ReadLong(c_FlagPage);
            int filePages = (int)m_PdbFile.ReadLong(c_FilePages);
            int rootBytes = (int)m_PdbFile.ReadLong(c_RootBytes);

            // Some sanity checking
            if (m_PdbFile.Length < filePages * m_PageSize)
            {
                throw new InvalidOperationException("This doesn't appear to be a PDB file");
            }

            // Work out how many root pages there must be
            int numRootPages = ((rootBytes - 1) / m_PageSize) + 1;

            // The c_IndexPages array contains the list of pages that contain the pages that make up the root stream (confusing, huh?)
            int numIndexPages = (((numRootPages * 4) - 1) / m_PageSize) + 1;
            List<int> indexPages = new List<int>();
            int pos = c_IndexPages;

            for (int x = 0; x < numIndexPages; x++)
            {
                indexPages.Add((int)m_PdbFile.ReadLong(pos));
                pos += 4;
            }

            // Read through the index page stream to get the list of pages that make up the root stream
            using (PdbStream indexStream = new PdbStream(m_PdbFile, m_PageSize, indexPages))
            {
                for (int x = 0; x < numRootPages; x++)
                {
                    m_RootPages.Add((int)indexStream.ReadLong(x * 4));
                }
            }

            // Find out the pages that correspond to the other streams
            using (PdbStream rootStream = new PdbStream(m_PdbFile, m_PageSize, m_RootPages))
            {
                // First word is the number of streams
                int numStreams = (int)rootStream.ReadLong(0);

                // Following words are the size of each page
                for (int x = 0; x < numStreams; x++)
                {
                    m_PageSizes.Add((int)rootStream.ReadLong(4 + x * 4));
                }

                // Following this is the list of streams mapped to each page
                pos = 4 + numStreams * 4;
                for (int x = 0; x < numStreams; x++)
                {
                    // Create the list of pages for this stream
                    List<int> streamList = new List<int>();

                    // The number of pages is calculated from the size for this page
                    if (m_PageSizes[x] > 0)
                    {
                        int numPagesForStream = ((m_PageSizes[x] - 1) / m_PageSize) + 1;
                        for (int y = 0; y < numPagesForStream; y++)
                        {
                            // Add the next page
                            int pageNum = (int)rootStream.ReadLong(pos);
                            streamList.Add(pageNum);
                            pos += 4;
                        }
                    }

                    // Store the pages for this stream
                    m_StreamPages.Add(streamList);
                }
            }
        }

        #endregion

        #region Low-level IO

        /// <summary>
        /// Returns the number of streams stored in this PDB file
        /// </summary>
        public int NumStreams
        {
            get
            {
                return m_StreamPages.Count;
            }
        }

        /// <summary>
        /// Given a stream number, returns the corresponding stream
        /// </summary>
        public Stream GetStream(int streamNo)
        {
            return new PdbStream(m_PdbFile, m_PageSize, m_StreamPages[streamNo]);
        }

        #endregion

        #region Higher level IO

        /// <summary>
        /// Reads or writes the signature for this file
        /// </summary>
        public PdbSignature Signature
        {
            get
            {
                // The signature is in stream 1
                using (Stream streamOne = GetStream(1))
                {
                    // The GUID is at byte 12
                    byte[] guid = new byte[16];
                    streamOne.Position = 12;
                    streamOne.Read(guid, 0, 16);

                    // The age is at byte 8
                    byte[] age = new byte[4];
                    streamOne.Position = 8;
                    streamOne.Read(age, 0, 4);

                    // Create the result
                    return new PdbSignature(new Guid(guid), (((int)age[0]) << 0) | (((int)age[1]) << 8) | (((int)age[2]) << 16) | (((int)age[3]) << 24));
                }
            }
            set
            {
                // Check for insanity
                if (value == null) throw new ArgumentNullException("value");
                if (value.Type != PdbType.PDB70) throw new ArgumentOutOfRangeException("value");

                // The signature is in stream 1
                using (Stream streamOne = GetStream(1))
                {
                    byte[] guidBytes = value.Guid.ToByteArray();
                    byte[] ageBytes = { (byte)(value.Age & 0xff), (byte)((value.Age >> 8) & 0xff), (byte)((value.Age >> 16) & 0xff), (byte)((value.Age >> 24) & 0xff) };

                    streamOne.Position = 8;
                    streamOne.Write(ageBytes, 0, 4);
                    streamOne.Write(guidBytes, 0, 16);
                }

                // The age is duplicated in stream 3
                using (Stream streamZero = GetStream(3))
                {
                    byte[] ageBytes = { (byte)(value.Age & 0xff), (byte)((value.Age >> 8) & 0xff), (byte)((value.Age >> 16) & 0xff), (byte)((value.Age >> 24) & 0xff) };

                    streamZero.Position = 8;
                    streamZero.Write(ageBytes, 0, 4);
                }
            }
        }

        #endregion
    }
}

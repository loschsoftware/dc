using System;

namespace RedFlag.Symbols
{
    /// <summary>
    /// Enumeration of the possible types of PDB file
    /// </summary>
    public enum PdbType
    {
        /// <summary>
        /// Unknown type of PDB file
        /// </summary>
        Unknown,

        /// <summary>
        /// PDB version 2.0 file (NB10 signature)
        /// </summary>
        PDB20,

        /// <summary>
        /// PDB version 7.0 file (RSDS signature)
        /// </summary>
        PDB70
    }
    /// <summary>
    /// Representation of a PDB signature
    /// </summary>
    public class PdbSignature
    {
        #region Member variables

        /// <summary>
        /// The type of PDB file that this signature is from
        /// </summary>
        private readonly PdbType m_Type;

        /// <summary>
        /// For PDB 7.0, this is the GUID that corresponds to the PDB file
        /// </summary>
        private readonly Guid m_Guid;

        /// <summary>
        /// For PDB 2.0, this is the 
        /// </summary>
        private readonly int m_Signature;

        /// <summary>
        /// The age of this PDB file
        /// </summary>
        private readonly int m_Age;

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialises a PDB signature by parsing a signature block from a PE file
        /// </summary>
        public PdbSignature(byte[] data)
        {
            // First few bytes should be 'NB10' or 'RSDS' depending on the PDB version used by this PE file. We don't support other versions
            // at the moment.
            if (data[0] == 'N' && data[1] == 'B' && data[2] == '1' && data[3] == '0')
            {
                // PDB 2.0 file
                unchecked
                {
                    int signature = (int)(((uint)data[8] << 0) | ((uint)data[9] << 8) | ((uint)data[10] << 16) | ((uint)data[11] << 24));
                    int age = (int)(((uint)data[12] << 0) | ((uint)data[13] << 8) | ((uint)data[14] << 16) | ((uint)data[15] << 24));

                    m_Type = PdbType.PDB20;
                    m_Signature = signature;
                    m_Age = age;
                    return;
                }
            }
            else if (data[0] == 'R' && data[1] == 'S' && data[2] == 'D' && data[3] == 'S')
            {
                // PDB 7.0 file
                unchecked
                {
                    byte[] guid = new byte[16];
                    for (int x = 0; x < 16; x++) guid[x] = data[x + 4];

                    Guid signature = new Guid(guid);
                    int age = (int)(((uint)data[20] << 0) | ((uint)data[21] << 8) | ((uint)data[22] << 16) | ((uint)data[23] << 24));

                    m_Type = PdbType.PDB70;
                    m_Guid = signature;
                    m_Age = age;
                    return;
                }
            }

            // Give up if the version is unsupported
            throw new InvalidOperationException("Unsupported debugger signature");
        }

        /// <summary>
        /// Initialises a v7 signature
        /// </summary>
        /// <param name="guid">The GUID of this signature</param>
        /// <param name="age">The age of this signature</param>
        public PdbSignature(Guid guid, int age)
        {
            m_Type = PdbType.PDB70;
            m_Guid = guid;
            m_Age = age;
        }

        /// <summary>
        /// Initialises a v2 signature
        /// </summary>
        /// <param name="signature">The signature dword</param>
        /// <param name="age">The age of this signature</param>
        public PdbSignature(int signature, int age)
        {
            m_Type = PdbType.PDB20;
            m_Signature = signature;
            m_Age = age;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The type of PDB file that this signature is from
        /// </summary>
        public PdbType Type
        {
            get { return m_Type; }
        }

        /// <summary>
        /// For PDB 7.0, this is the GUID that represents the signature of the PDB file
        /// </summary>
        public Guid Guid
        {
            get { return m_Guid; }
        }

        /// <summary>
        /// For PDB 2.0, this is the value that corresponds to the signature of the file
        /// </summary>
        public int Signature
        {
            get { return m_Signature; }
        }

        /// <summary>
        /// The age of this PDB file
        /// </summary>
        public int Age
        {
            get { return m_Age; }
        }

        #endregion

        #region Comparisons

        public bool Equals(PdbSignature obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.m_Type, m_Type) && obj.m_Guid.Equals(m_Guid) && obj.m_Signature == m_Signature && obj.m_Age == m_Age;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PdbSignature)) return false;
            return Equals((PdbSignature)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = m_Type.GetHashCode();
                result = (result * 397) ^ m_Guid.GetHashCode();
                result = (result * 397) ^ m_Signature;
                result = (result * 397) ^ m_Age;
                return result;
            }
        }

        #endregion

#if DEBUG

        public override string ToString()
        {
            switch (m_Type)
            {
                case PdbType.PDB20:
                    return string.Format("{0}: {1}", m_Age, m_Signature);

                case PdbType.PDB70:
                default:
                    return string.Format("{0}: {1}", m_Age, m_Guid);
            }
        }

#endif
    }
}


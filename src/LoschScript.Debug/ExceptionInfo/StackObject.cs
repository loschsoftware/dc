using System;
using System.Collections.Generic;
using System.Text;

namespace RedFlag
{
    [Serializable]
    public class StackObject : MethodArgument
    {
        private int m_ObjectDepth;
        private bool m_ComplexType = false;
        private int m_Size=0;
        private long m_Address = 0;
        public bool ComplexType
        {
            get
            {
                return m_ComplexType;
            }
            set
            {
                m_ComplexType = value;
            }
        }
        public int ObjectDepth
        {
            get
            {
                return m_ObjectDepth;
            }
            set
            {
                m_ObjectDepth = value;
            }
        }
        public int Size
        {
            get
            {
                return m_Size;
            }
            set
            {
                m_Size = value;
            }
        }
        public long Address {
            get
            {
                return m_Address;
            }
            set
            {
                m_Address = value;
            }
        }
        /// <summary>
        /// Dump the object type, name and value, indented by depth
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(80);
           /* for (int i = 0; i < m_ObjectDepth; i++)
            {
                sb.Append("    ");
            }*/
            sb.Append(this.Type);
            sb.Append(":");
            sb.Append(this.Name);
            sb.Append("=");
            sb.Append(this.Value);
            return sb.ToString();
        }
    }
}

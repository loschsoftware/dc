using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedFlag.ExceptionInfo
{
    public class HeapStatistic
    {
        private string m_Typename = String.Empty;
        private int m_TypeCount = 0;
        private int m_TypeSize=0;
        public HeapStatistic() { }
        public HeapStatistic(string ClassName, int ClassCount, int ClassSize)
        {
            m_Typename = ClassName;
            m_TypeCount = ClassCount;
            m_TypeSize = ClassSize;
        }
        public string TypeName
        {
            get
            {
                return m_Typename;
            }
            set
            {
                m_Typename = value;
            }
        }
        public int TypeSize
        {
            get
            {
                return m_TypeSize;
            }
            set
            {
                m_TypeSize = value;
            }
        }
        public int TypeCount
        {
            get
            {
                return m_TypeCount;
            }
            set
            {
                m_TypeCount = value;
            }
        }
    }
    public class HeapStatistics : List<HeapStatistic>
    {
        public HeapStatistic this[string s]
        {
            get
            {
                foreach (HeapStatistic stat in this)
                {
                    if (stat.TypeName == s) return stat;
                }
                return null;
            }
        }
    }
}

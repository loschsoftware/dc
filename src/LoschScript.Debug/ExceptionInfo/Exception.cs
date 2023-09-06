using System;
using System.Collections.Generic;
using System.Text;

namespace RedFlag
{
    [Serializable]
    public class Exception
    {
        private const int HRESULT_WIN32_ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
        private const int HRESULT_WIN32_ERROR_ACCESSDENIED = unchecked((int)0x80070005);
        private List<string> m_StackTrace=null;
        public List<Method> Methods = new List<Method>();
        private List<string> m_Strings = null;
        private string m_Name = String.Empty;
        private Guid m_Guid = Guid.NewGuid();
        private TimeSpan m_Time=new TimeSpan(0);
        private string m_Message = String.Empty;
        private int m_Hresult = 0;
        private bool m_FirstChance = true;
        private string m_MaybeFullPath = String.Empty;
        public bool Handled
        {
            get { return m_FirstChance; }
            set { m_FirstChance = value; }
        }
        [System.Xml.Serialization.XmlIgnore]
        public string MayBeFullPath
        {
            get { return m_MaybeFullPath; }
            set { m_MaybeFullPath = value; }
        }
        public int HResult
        {
            get
            {
                return m_Hresult;
            }
            set
            {
                m_Hresult = value;
                // Check for locked file
                if (m_Hresult == HRESULT_WIN32_ERROR_SHARING_VIOLATION || m_Hresult == HRESULT_WIN32_ERROR_ACCESSDENIED)
                {
                try{
                    //Filename should be in "MaybeFullPath" already
                    if (!String.IsNullOrEmpty(m_MaybeFullPath))
                    m_Message += "(processes:" + OpenHandles.GetProcessesUsingFile(m_MaybeFullPath) + ")";
                    
               }
                    catch (System.Exception e){
                    }
            }
            }
        }
        public string Message
        {
            get
            {
                return m_Message;
            }
            set
            {
                m_Message = value;
            }
        }
        public Guid GUID
        {

            get
            {
                return m_Guid;
            }
            set
            {
                m_Guid = value;
            }
        }
        [System.Xml.Serialization.XmlIgnore]
        public List<string> StackTrace
        {
            get
            {
                m_StackTrace = new List<string>();
                foreach (Method m in Methods)
                {
                    m_StackTrace.Add(m.Signature);
                }
                return m_StackTrace;
            }
        }
        [System.Xml.Serialization.XmlIgnore]
        public List<string> Strings
        {
            get
            {
                m_Strings = new List<string>();
                foreach (Method m in Methods)
                {
                    foreach (StackObject so in m.PrivateMembers)
                    {
                        if (so.Type == "System.String") m_Strings.Add(String.Format("{0}={1}",so.Name,so.Value));
                    }
                }
                return m_Strings;
            }
        }
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
                
            }
        }
        public TimeSpan Time
        {
            get
            {
                return m_Time;
            }
            set
            {
                m_Time = value;
            }
        }
        public long TimeOffset
        {
            get
            {
                return m_Time.Ticks;
            }
            set
            {
                m_Time = new TimeSpan(value);
            }
        }
    }
    public class Exceptions : List<Exception>
    {
        private List<Exception> m_Exceptions;
        public Exception this[Guid id]
        {
            get
            {
                foreach (Exception ex in m_Exceptions)
                {
                    if (id.Equals(ex.GUID)) return ex;
                }
                return null;
            }
        }
        public List<Exception> ListOfExceptions
        {
            get
            {
                return m_Exceptions;
            }
            set
            {
                m_Exceptions = value;
            }
        }
    }
}

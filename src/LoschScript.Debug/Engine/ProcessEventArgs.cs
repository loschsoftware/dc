using System;
using System.Collections.Generic;
using System.Text;

namespace RedFlag.Engine
{
    public class ProcessEventArgs : EventArgs
    {
        private string m_Message = String.Empty;
        private EngineStatus m_EngineStatus = EngineStatus.Idle;
        private int m_ProcessId = -1;
        public EngineStatus Status
        {
            get
            {
                return m_EngineStatus;
            }
            set
            {
                m_EngineStatus = value;
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
        public int ProcessId
        {
            get
            {
                return m_ProcessId;
            }
            set
            {
                m_ProcessId = value;
            }
        }
    }
}

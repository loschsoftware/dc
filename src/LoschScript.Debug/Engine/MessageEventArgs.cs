using System;
using System.Collections.Generic;
using System.Text;

namespace RedFlag.Engine
{
    public class MessageEventArgs : EventArgs
    {
       public MessageEventArgs(TraceMessage message)
       {
           TraceMessage = message;
       }
        public TraceMessage TraceMessage { get; set; }
    }
}

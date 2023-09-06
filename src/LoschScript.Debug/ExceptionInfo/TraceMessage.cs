using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RedFlag
{
    /// <summary>
    /// Information returned by Debug.Trace
    /// </summary>
    [Serializable]
    public class TraceMessage
    {
        public TraceMessage() { }
        public TraceMessage(string message, string name, string switchname)
        {
            Message = message;
            Name = name;
            SwitchName = switchname;
        }
        public string Message { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string SwitchName { get; set; }
    }
}

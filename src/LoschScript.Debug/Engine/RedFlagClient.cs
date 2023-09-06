using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace RedFlag
{
    public class ChangeSettingsEventArgs : EventArgs
    {
        public int StackDepth { get; set; }
        public int StackLength { get; set; }
        public string BreakSource { get; set; }
        public int BreakLine { get; set; }
        public bool ProcessArrays { get; set; }
        public string DefaultDotNetVersion { get; set; }
        public List<string> IgnoreExceptions { get; set; }
    }
}

namespace RedFlag.Engine
{
    /// <summary>
    /// A class that communicates via named pipes and "acts like" a debug engine
    /// </summary>
    class RedFlagClient : DebugEngine
    {
        private NamedPipeClientStream client;
        private NamedPipeClientStream clientControl;
        public string pipeName = String.Format("RedFlag_{0}", System.Diagnostics.Process.GetCurrentProcess().Id);
        private string m_SetBreakPoint = String.Empty;
        private ChangeSettingsEventArgs settings = new ChangeSettingsEventArgs();
        public override string SetBreakpoint
        {
            set
            {
                m_SetBreakPoint = value;
                // make a settings file and send to client
                if (!String.IsNullOrEmpty(this.SetBreakpoint))
                {
                    string[] bpInfo = this.SetBreakpoint.TrimEnd(')').Split('(');
                    int breakLine = 0;
                    settings.BreakSource = bpInfo[0];
                    Int32.TryParse(bpInfo[1], out breakLine);
                    settings.BreakLine = breakLine;
                }
                settings.IgnoreExceptions = base.ExceptionsToIgnore;
                settings.ProcessArrays = base.GetArrays;
                settings.StackDepth = base.MaxObjectDepth;
                settings.StackLength = base.MaxStackDepth;
                settings.DefaultDotNetVersion = this.DefaultNetVersion;
                settings.IgnoreExceptions = base.ExceptionsToIgnore;
                settings.ProcessArrays = base.GetArrays;
                settings.StackDepth = base.MaxObjectDepth;
                settings.StackLength = base.MaxStackDepth;
               
                SendSettingsThroughPipe(settings,clientControl);
            }
            get
            {
                return m_SetBreakPoint;
            }
        }
        public override void RunProcess()
        {
           this.client = new NamedPipeClientStream(".", pipeName, PipeDirection.In);
           this.clientControl = new NamedPipeClientStream(".", pipeName + "_Control", PipeDirection.Out);
           onStatusChange("Waiting for " + base.ProcessName + " to connect.", EngineStatus.Attaching, -1);
           client.Connect();
           clientControl.Connect();
           SendSettingsThroughPipe(settings,clientControl);
           
            string temp = String.Empty;
            XmlSerializer exceptionSerializer = new XmlSerializer(typeof(RedFlag.Exception));
            EngineEventType eventType = EngineEventType.None;
             while (client.IsConnected)
            {
                switch (eventType)
                {
                    case EngineEventType.NewException:
                        RedFlag.Exception ex;
                        var neString = new StreamString(client);
                        string xmlException = neString.ReadString().Replace("&#x0;","");
                        XmlSerializer xs = new XmlSerializer(typeof(RedFlag.Exception));
                        using (var reader = new StringReader(xmlException))
                        {
                            ex = (RedFlag.Exception)xs.Deserialize(reader);
                        }
                        base.onNewException(ex);
                        break;
                    case EngineEventType.NewMessage:
                        RedFlag.TraceMessage traceMessage;
                        var nmString = new StreamString(client);
                        string xmlRecv = nmString.ReadString();
                        XmlSerializer nmSer = new XmlSerializer(typeof(RedFlag.TraceMessage));
                        using (var reader = new StringReader(xmlRecv))
                        {
                            traceMessage = (RedFlag.TraceMessage)nmSer.Deserialize(reader);
                        }
                        base.onNewMessage(new MessageEventArgs(traceMessage));
                        break;
                    case EngineEventType.StatusChange:
                        RedFlag.Engine.ProcessEventArgs processEvent;
                        var peString = new StreamString(client);
                        string peXml = peString.ReadString();
                        XmlSerializer peSer = new XmlSerializer(typeof(RedFlag.Engine.ProcessEventArgs));
                        using (var reader = new StringReader(peXml))
                        {
                            processEvent = (RedFlag.Engine.ProcessEventArgs)peSer.Deserialize(reader);
                        }
                        if (processEvent.Status == RedFlag.Engine.EngineStatus.Attaching)
                        {
                            base.ProcessName = processEvent.Message.Substring(processEvent.Message.IndexOf(" ") + 1);
                            base.ProcessId = processEvent.ProcessId;
                        }
                        if (processEvent.Status == RedFlag.Engine.EngineStatus.Idle)
                            SendEventTypeThroughPipe(EngineEventType.Disconnect,clientControl);
                        if (processEvent.Status != EngineStatus.DumpCreated)
                        base.onStatusChange(processEvent.Message, processEvent.Status, processEvent.ProcessId);
                        break;
                    case EngineEventType.ModuleLoad:
                        RedFlag.Module mod;
                        var modString = new StreamString(client);
                        string modXml = modString.ReadString();
                        XmlSerializer modSer = new XmlSerializer(typeof(RedFlag.Module));
                        using (var reader = new StringReader(modXml))
                        {
                            mod = (RedFlag.Module)modSer.Deserialize(reader);
                        }
                        base.onModuleLoad(mod);
                        break;
                    case EngineEventType.Pong:
                        //Console.WriteLine("Server replied to Ping");
                        break;
                    case EngineEventType.StackDump:
                        var stackStream = new StreamString(client);
                        base.StackDump=stackStream.ReadString();
                        onStatusChange("Dumped stacks", EngineStatus.DumpCreated, base.ProcessId);
                        break;
                    default:
                        break;
                }
                
                eventType = GetStatusFromPipe(client);
            }

        // TODO: process exited
        }
        private EngineEventType GetStatusFromPipe(NamedPipeClientStream clientStm)
        {
            var statStream = new StreamString(clientStm);
            string statString = statStream.ReadString();
            EngineEventType type = EngineEventType.None;
            try
            {
                type = (EngineEventType)Enum.Parse(typeof(EngineEventType), statString);
            }
            catch {  /* What's he playing at? */}
            return type;
        }
        private void SendEventTypeThroughPipe(EngineEventType type,NamedPipeClientStream clientStm)
        {
           StreamString ss = new StreamString(clientStm);
            string typeString = type.ToString("G");
            ss.WriteString(typeString);
           // client.WaitForPipeDrain();
        }
        private void SendSettingsThroughPipe(ChangeSettingsEventArgs settings,NamedPipeClientStream clientStm)
        {
            if (client!=null && client.IsConnected)
            {
                SendEventTypeThroughPipe(EngineEventType.Settings,clientStm);
                XmlSerializer xsSettings = new XmlSerializer(typeof(RedFlag.ChangeSettingsEventArgs));
                using (StringWriter textWriter = new StringWriter())
                {
                    xsSettings.Serialize(textWriter, settings);
                    string s = textWriter.ToString();
                    StreamString ss = new StreamString(clientStm);
                    ss.WriteString(s);
                }
            }
        }
        /// <summary>
        /// Signal the debugging loop to exit and detach the debugger
        /// </summary>
        /// <param name="worker">The thread that is running this debugging session</param>
        public override void StopDebugging(System.Threading.Thread worker)
        {
            onStatusChange("Detaching from " + ProcessName, EngineStatus.Detaching, ProcessId);
            SendEventTypeThroughPipe(EngineEventType.StopRequest,clientControl);
        }
        public override void DumpStacks(System.Threading.Thread worker)
        {
            SendEventTypeThroughPipe(EngineEventType.StackDump,clientControl);
        }
    }
}

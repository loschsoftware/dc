using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace RedFlag.Engine
{
    public enum EngineEventType
    {
        None = 0,
        NewMessage,
        NewException,
        StatusChange,
        ModuleLoad,
        Settings,
        Ping,
        Pong,
        StopRequest,
        StackDump,
        Disconnect
    }
    public class RedFlagServer
    {
        private NamedPipeServerStream m_PipeServer;
        private string m_PipeName = String.Empty;
        private string m_ProgramName = String.Empty;
        private string m_ProgramArgs = String.Empty;
        private DebugEngine m_Engine;
        private Thread workerThread;
        private RegistryKey m_IFEOKey;
        private bool m_ProcessFinished = false;
        public RedFlagServer(string PipeName, string ProgramName, string ProgramArguments)
        {
            m_PipeName = PipeName; m_ProgramName = ProgramName; m_ProgramArgs = ProgramArguments;
        }
        public void Run()
        {
            m_PipeServer = new NamedPipeServerStream(m_PipeName, PipeDirection.Out);
            try
            {
                string keyName = "Software\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\" + System.IO.Path.GetFileName(m_ProgramName);
                m_IFEOKey = Registry.LocalMachine.OpenSubKey(keyName, true);
                m_IFEOKey.DeleteValue("Debugger", false);
            }
            catch { m_ProcessFinished = true; }

            m_PipeServer.WaitForConnection();
            if (m_ProcessFinished)
            {
               // SendEventTypeThroughPipe(EngineEventType.Disconnect);
                m_PipeServer.Disconnect();
                return;
            }

            // A client has connected!
            // Client must send engine settings before we agree to do anything!
            m_Engine = new DebugEngine();
            m_Engine.NewException += new NewExceptionHandler(engine_NewException);
            m_Engine.ProcessStatus += new ProcessStatusHandler(engine_ProcessStatus);
            m_Engine.NewMessage += new DebugMessageHandler(engine_NewMessage);
            m_Engine.ModuleLoad += new ModuleLoadHandler(engine_NewModule);
            m_Engine.ProcessName = m_ProgramName;
            m_Engine.ProcessArgs = " "+m_ProgramArgs;
            NamedPipeServerStream controlConnection = new NamedPipeServerStream(m_PipeName + "_Control", PipeDirection.In);
            controlConnection.WaitForConnection();
            EngineEventType eventType = GetStatusFromPipe(controlConnection);

            if (eventType == EngineEventType.Settings)
                ChangeSettings(controlConnection);
            else
            {
                m_Engine.MaxStackDepth = 4;
                m_Engine.MaxObjectDepth = 5;
                m_Engine.GetArrays = false;
                m_Engine.SetBreakpoint = String.Empty;
                m_Engine.ExceptionsToIgnore = new List<string>();
                m_Engine.DefaultNetVersion = "v4.0.30319";

            }
            try
            {
                System.Threading.ThreadStart ts = new System.Threading.ThreadStart(m_Engine.RunProcess);
                workerThread = new System.Threading.Thread(ts);
                workerThread.IsBackground = true; // mainly for SA so it terms
                workerThread.Name = "RedFlagServer";
                workerThread.Start();

            }
            catch (System.Exception exc)
            {
                SendEventTypeThroughPipe(EngineEventType.Disconnect);

            }
            while (!m_ProcessFinished)
            {
                EngineEventType type = GetStatusFromPipe(controlConnection);
                switch (type)
                {
                    case EngineEventType.StopRequest:
                        m_Engine.StopDebugging(workerThread);
                        // Engine will signal client when it stops so the loop exits
                        break;
                    case EngineEventType.Ping:
                        SendEventTypeThroughPipe(EngineEventType.Pong);
                        break;
                    case EngineEventType.Settings:
                        ChangeSettings(controlConnection);
                        break;
                    case EngineEventType.StackDump:
                        m_Engine.DumpStacks(workerThread);
                        // The engine will respond with a StacksDUmped status and then we pick it up.
                        break;
                    case EngineEventType.Disconnect:
                        /* seems a bit backwards - the server sends "aborting" or "idle" to the client, then
                         * the client sends a "Disconnect" command and then the server exits. Who's in charge here?? */
                        m_ProcessFinished = true;
                        break;
                    default:
                        break;
                }
            }
            controlConnection.Disconnect();
            m_PipeServer.Disconnect();
        }

        private void engine_NewModule(object o, ModuleLoadedEventArgs e)
        {
            SendEventTypeThroughPipe(EngineEventType.ModuleLoad);
            XmlSerializer xs = new XmlSerializer(typeof(RedFlag.Module));
            using (StringWriter textWriter = new StringWriter())
            {
                xs.Serialize(textWriter, e.Module);
                string s = textWriter.ToString();
                StreamString ss = new StreamString(m_PipeServer);
                ss.WriteString(s);
            }
           m_PipeServer.WaitForPipeDrain();
        }

        private void engine_NewMessage(object o, MessageEventArgs e)
        {
            SendEventTypeThroughPipe(EngineEventType.NewMessage);
            XmlSerializer xs = new XmlSerializer(typeof(RedFlag.TraceMessage));
            using (StringWriter textWriter = new StringWriter())
            {
                xs.Serialize(textWriter, e.TraceMessage);
                string s = textWriter.ToString();
                StreamString ss = new StreamString(m_PipeServer);
                ss.WriteString(s);
            }
            m_PipeServer.WaitForPipeDrain();
        }

        private void engine_ProcessStatus(object o, ProcessEventArgs e)
        {
            SendEventTypeThroughPipe(EngineEventType.StatusChange);
            XmlSerializer xs = new XmlSerializer(typeof(ProcessEventArgs));
            using (StringWriter textWriter = new StringWriter())
            {
                xs.Serialize(textWriter, e);
                string s = textWriter.ToString();
                StreamString ss = new StreamString(m_PipeServer);
                ss.WriteString(s);
            }
            if (e.Status == EngineStatus.DumpCreated)
            {
                SendEventTypeThroughPipe(EngineEventType.StackDump);
                StreamString ss = new StreamString(m_PipeServer);
                string dump = m_Engine.StackDump;
                ss.WriteString(dump);
                m_PipeServer.WaitForPipeDrain();
            }
            m_PipeServer.WaitForPipeDrain();
        }

        private void engine_NewException(object o, EventArgs e)
        {
            SendEventTypeThroughPipe(EngineEventType.NewException);
            //XmlTextWriter xw = new XmlTextWriter(m_PipeServer, Encoding.UTF8);
            XmlSerializer xs = new XmlSerializer(typeof(RedFlag.Exception));
            using (StringWriter textWriter = new StringWriter())
            {
                xs.Serialize(textWriter, o);
                string s = textWriter.ToString();
                StreamString ss = new StreamString(m_PipeServer);
                ss.WriteString(s);
            }
            m_PipeServer.WaitForPipeDrain();
        }
        private EngineEventType GetStatusFromPipe(NamedPipeServerStream stream)
        {
            EngineEventType type = EngineEventType.None;
            try
            {
                var statStream = new StreamString(stream);
                string statString = statStream.ReadString();
                type = (EngineEventType)Enum.Parse(typeof(EngineEventType), statString);
            }
            catch (ArgumentException){ }
            return type;
        }
        private void SendEventTypeThroughPipe(EngineEventType type)
        {
            StreamString ss = new StreamString(m_PipeServer);
            string typeString = type.ToString("G");
            ss.WriteString(typeString);
            m_PipeServer.WaitForPipeDrain();
        }
        private void ChangeSettings(NamedPipeServerStream serverStm)
        {
            var settingsStream = new StreamString(serverStm);
            string settingsString = settingsStream.ReadString();
            XmlSerializer xs = new XmlSerializer(typeof(ChangeSettingsEventArgs));
            ChangeSettingsEventArgs settings = new ChangeSettingsEventArgs();
            using (var reader = new StringReader(settingsString))
            {
                settings = (ChangeSettingsEventArgs)xs.Deserialize(reader);
            }
            if (m_Engine != null)
            {
                m_Engine.MaxStackDepth = settings.StackLength;
                m_Engine.MaxObjectDepth = settings.StackDepth;
                m_Engine.GetArrays = settings.ProcessArrays;
                if (!String.IsNullOrEmpty(settings.BreakSource) && settings.BreakLine > 0)
                    m_Engine.SetBreakpoint = String.Format("{0}({1})", settings.BreakSource, settings.BreakLine);
                m_Engine.ExceptionsToIgnore = settings.IgnoreExceptions;
                m_Engine.DefaultNetVersion = settings.DefaultDotNetVersion;
            }
        }
    }
}

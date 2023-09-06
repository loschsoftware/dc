using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Samples.Debugging.MdbgEngine;
using System.Runtime.InteropServices;
using RedFlag.Engine;
using System.Threading;
using RedFlag.Engine.AssemblyInspectorImplementation;

namespace RedFlag.Engine
{
    public enum EngineMode
    {
        None=0,
        Attach,
        Launch
    }
    public enum EngineStatus
    {
        Idle = 0,
        Attaching,
        Debugging,
        Detaching,
        Dumping,
        DumpCreated,
        HeapDumped,
        AssemblyLoaded,
        Aborting
    }
    public delegate void NewExceptionHandler(object o, EventArgs e);
    public delegate void ProcessStatusHandler(object o, ProcessEventArgs e);
    public delegate void DebugMessageHandler(object o, MessageEventArgs e);
    public delegate void ModuleLoadHandler(object o,ModuleLoadedEventArgs e);
    class DebugEngine
    {
        public RedFlag.ExceptionInfo.HeapStatistics HeapStats = new RedFlag.ExceptionInfo.HeapStatistics();
        /// <summary>
        /// If we are launching unmanaged, what version of net do we want?
        /// </summary>
        public string DefaultNetVersion 
        {
           get 
           {
               return m_DefaultDotNetVersion;
           }
            set 
            {
                m_DefaultDotNetVersion = value;
            }
        }
        /// <summary>
        /// This debugger is running in the WOW
        /// </summary>
        public int MaxStackDepth = 6; 
        /// <summary>
        /// The amount of recursion into dependent objects
        /// </summary>
        public int MaxObjectDepth = 4;
        /// <summary>
        /// Fires when a new module is loaded into the process
        /// </summary>
        public event ModuleLoadHandler ModuleLoad;
        /// <summary>
        /// Fires when a new exception is detected
        /// </summary>
        public event NewExceptionHandler NewException;
        /// <summary>
        /// Fires when a trace message is produced
        /// </summary>
        public event DebugMessageHandler NewMessage;
        /// <summary>
        /// Fires when the status of the debugger changes
        /// </summary>
        public event ProcessStatusHandler ProcessStatus;
        /// <summary>
        /// This debugger is running in the WOW
        /// </summary>
        private bool ThisProcessIs32Bit = false;
        /// <summary>
        /// The process we are debugging is running in the WOW
        /// </summary>
        private bool DebuggedProcessIs32Bit = false;
        /// <summary>
        /// The arguments to the program to be launched
        /// </summary>
        public string ProcessArgs = String.Empty;
        /// <summary>
        /// The program to be launched for debugging
        /// </summary>
        public string ProcessName = String.Empty;
        /// <summary>
        /// Get local array values (affects performance badly!)
        /// </summary>
        public bool GetArrays = false;
        public AppDomains AppDomains = new AppDomains();
        /// <summary>
        /// The ID of the process to attach to (overrides launch)
        /// </summary> 
        public int ProcessId=-1;
        /// <summary>
        /// The last stack dump requested by the user
        /// </summary>
        public string StackDump = String.Empty;
        /// <summary>
        /// The list of modules loaded into this process;
        /// </summary>
        public List<Module> Modules = new List<Module>();
        /// <summary>
        /// A list of exception types that we won't collect any information about
        /// </summary>
        public List<string> ExceptionsToIgnore
        {
            set
            {
                m_ExceptionsToIgnore = value;
            }
            get
            {
                return m_ExceptionsToIgnore;
            }
        }
        /// <summary>
        /// A sync event handle to use (currently only for releasing the debugger from pause)
        /// </summary>
        private AutoResetEvent m_DebuggerPause = new AutoResetEvent(false);
        private string m_BreakSourceFile=String.Empty;
        private int m_BreakSourceLine=0;
        private ManualResetEvent m_SignalDebugger = new ManualResetEvent(false);
        /// <summary>
        /// Set a breakpoint format=SourceFileName.cs(line number)
        /// If debugging, interrupt the debugger's thread for the changes to take effect.
        /// </summary>
        public virtual string SetBreakpoint 
        {
            get
            {
                return String.Format("{0}({1})",m_BreakSourceFile,m_BreakSourceLine);
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    string[] bpInfo = value.TrimEnd(')').Split('(');
                    int breakLine = 0;
                    m_BreakSourceFile = bpInfo[0];
                    Int32.TryParse(bpInfo[1], out breakLine);
                    m_BreakSourceLine = breakLine;
                }
            }
        }
        private MDbgEngine m_Debugger = new MDbgEngine();
        private bool m_cancel = false; // flag to stop the debugger on thread interrupt
        private DateTime m_StartTime = DateTime.Now;
        private EngineStatus m_Status = EngineStatus.Idle;
        private List<string> m_ExceptionsToIgnore;
        private string m_DefaultDotNetVersion=System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion;
        private enum ThreadInterruptFlags
        {
            None = 0,
            StackDump,
            HeapStats,
            NewBreakpoint,
            StopDebugger,
            PauseDebugger
        }
        private ThreadInterruptFlags m_InterruptFlags = ThreadInterruptFlags.None;
        public virtual void DumpStacks(System.Threading.Thread worker)
        {
            // I'm not proud of this, but interrput the debugger thread
            // and set m_StackDump to true, then the debugger thread will dump the stacks and resume
            m_InterruptFlags = m_InterruptFlags | ThreadInterruptFlags.StackDump;
            m_SignalDebugger.Set();
        }
        public void GetHeapStats()
        {
            m_InterruptFlags = m_InterruptFlags | ThreadInterruptFlags.HeapStats;
            m_SignalDebugger.Set();
        }
        /// <summary>
        /// Method that will stop the running process so we can inspect something manually
        /// </summary>
        /// <param name="worker"></param>
        public void PauseDebugger(System.Threading.Thread worker)
        {
            m_InterruptFlags = m_InterruptFlags | ThreadInterruptFlags.PauseDebugger;
            m_DebuggerPause.Reset();
            //worker.Interrupt();
            m_SignalDebugger.Set();
        }
        /// <summary>
        /// If paused, set the debugger running again
        /// </summary>
        /// <param name="worker"></param>
        public void RestartDebugger(System.Threading.Thread worker)
        {
             m_DebuggerPause.Set();
        }
        /// <summary>
        /// Interrupt the debugger when we want to change the settings
        /// </summary>
        /// <param name="worker"></param>
        public void ChangeDebuggerSettings(System.Threading.Thread worker)
        {
            m_InterruptFlags = m_InterruptFlags | ThreadInterruptFlags.NewBreakpoint;
           // worker.Interrupt();
            m_SignalDebugger.Set();
        }
        /// <summary>
        /// Start the specified process name and gather exceptions, stack traces, and stack objects
        /// </summary>
        public virtual void RunProcess()
        {
            #region Set up debugging engine
            m_Debugger.Options.CreateProcessWithNewConsole = true;
            m_Debugger.Options.StopOnException = true;
            m_Debugger.Options.StopOnAssemblyLoad = true;
            m_Debugger.Options.StopOnLogMessage = true;
            MDbgProcess proc = null;
            SysWowInfo.IsWow64Process(System.Diagnostics.Process.GetCurrentProcess().Handle, out ThisProcessIs32Bit);
            string bitError32="it is a 32-bit application. Please use RedFlag-x86.exe";
            string bitError64="it is a 64-bit application. Please use RedFlag.exe";
            if (ProcessId < 0)
            {
                if (!System.IO.File.Exists(ProcessName))
                {
                    onStatusChange(String.Format(
                        "Cannot launch {0}: file does not exist", ProcessName), EngineStatus.Aborting, -1);
                    m_cancel = true;
                }
                else
                {
                    try
                    {
                        proc = m_Debugger.CreateProcess(ProcessName, ProcessArgs, DebugModeFlag.Debug, DebugEngineUtils.GetAssemblyRuntimeVersion(ProcessName, DefaultNetVersion));
                    }
                    catch (COMException exc)
                    {
                        string message = exc.Message;
                        if (exc.ErrorCode == unchecked((int)0x80131C30) && ThisProcessIs32Bit) message = bitError64;
                        if (exc.ErrorCode == unchecked((int)0x80131C30) && !ThisProcessIs32Bit) message = bitError32;
                        onStatusChange("Cannot debug " + ProcessName + ": " + message, EngineStatus.Aborting, ProcessId);
                        m_cancel = true;
                    }
                    if (!m_cancel)
                    {
                        ProcessId = proc.CorProcess.Id;
                        onStatusChange("Launching " + ProcessName, EngineStatus.Attaching, ProcessId);
                    }
                    
                }
            }
            if (ProcessId > -1 && !m_cancel) // We have a process Id but it could be from a process that can't be debugged
            {
                if (m_Status != EngineStatus.Attaching) // Status will be "attaching" if the launch was successful
                {
                    try
                    {
                        onStatusChange("Attaching to " + ProcessName, EngineStatus.Attaching, ProcessId);
                        proc = m_Debugger.Attach(ProcessId);
                    }
                    catch (COMException cex)
                    {
                        string message = cex.Message;
                        if (cex.ErrorCode == unchecked((int)0x80131C30) && ThisProcessIs32Bit) message = bitError64;
                        if (cex.ErrorCode == unchecked((int)0x80131C30) && !ThisProcessIs32Bit) message = bitError32;
                        onStatusChange("Cannot debug " + ProcessName + ": " + message, EngineStatus.Aborting, ProcessId);
                        m_cancel = true;
                    }
                }
            }
            if (!m_cancel) // We have a debugged process - set the environment for it
            {
                SysWowInfo.IsWow64Process(System.Diagnostics.Process.GetProcessById(ProcessId).Handle, out DebuggedProcessIs32Bit);
                // Once process is running, we can set a breakpoint
                if (!String.IsNullOrEmpty(m_BreakSourceFile) && m_BreakSourceLine > 0) m_Debugger.Processes.Active.Breakpoints.CreateBreakpoint(
                      m_BreakSourceFile, m_BreakSourceLine
                 );
                string shortProcessName = DebugEngineUtils.FileNameIsValid(ProcessName) ? System.IO.Path.GetFileName(ProcessName) : ProcessName;
                if (ProcessStatus != null) onStatusChange("Debugging " + shortProcessName, EngineStatus.Debugging, ProcessId);
                proc.CorProcess.OnCreateAppDomain += new Microsoft.Samples.Debugging.CorDebug.CorAppDomainEventHandler(CorProcess_OnCreateAppDomain);
                proc.CorProcess.OnAppDomainExit += new Microsoft.Samples.Debugging.CorDebug.CorAppDomainEventHandler(CorProcess_OnAppDomainExit);
                proc.CorProcess.OnModuleLoad += new Microsoft.Samples.Debugging.CorDebug.CorModuleEventHandler(CorProcess_OnModuleLoad);
                proc.EnableUserEntryBreakpoint = false;
             }
            #endregion
            while (!m_cancel && proc.IsAlive)
            {
                // Let the debuggee run and wait until it hits a debug event or m_SignalDebugger is set.
                try
                {
                    m_SignalDebugger.SafeWaitHandle = proc.Go().SafeWaitHandle;
                    m_SignalDebugger.WaitOne();
                }
                catch (InvalidOperationException)
                {
                    //Increment stop count to prevent this again.
                    proc.AsyncStop();
                }
                #region Check if user requested stop
                // If the handle was not set by MDbgEngine, handle the reason for the set.
                    
                    switch (m_InterruptFlags)
                    {
                        case ThreadInterruptFlags.HeapStats:
                            proc.AsyncStop().WaitOne();
                            HeapStats = DebugEngineUtils.HeapStats(proc,this.MaxObjectDepth,this.MaxObjectDepth,this.GetArrays);
                            onStatusChange("Heap dump created", EngineStatus.HeapDumped, ProcessId);
                            m_InterruptFlags = m_InterruptFlags ^ ThreadInterruptFlags.HeapStats;
                        break;
                        case ThreadInterruptFlags.StackDump:
                            Dictionary<int, int[]> threads = DebugEngineUtils.GetThreadStatus(ProcessId);
                            proc.AsyncStop().WaitOne();
                            StackDump = DebugEngineUtils.DumpStacks(proc, ProcessId, threads);
                            onStatusChange("Stack dump created", EngineStatus.DumpCreated, ProcessId);
                            m_InterruptFlags = m_InterruptFlags ^ ThreadInterruptFlags.StackDump;
                            break;
                        case ThreadInterruptFlags.NewBreakpoint:
                            proc.AsyncStop().WaitOne();
                            m_Debugger.Processes.Active.Breakpoints.DeleteAll();
                            if (!String.IsNullOrEmpty(m_BreakSourceFile) && m_BreakSourceLine>0)
                            m_Debugger.Processes.Active.Breakpoints.CreateBreakpoint(m_BreakSourceFile, m_BreakSourceLine);
                            m_InterruptFlags = m_InterruptFlags ^ ThreadInterruptFlags.NewBreakpoint;
                            break;
                        case ThreadInterruptFlags.StopDebugger:
                            proc.AsyncStop().WaitOne();
                            m_Debugger.Processes.Active.Breakpoints.DeleteAll();
                            m_InterruptFlags = m_InterruptFlags ^ ThreadInterruptFlags.StopDebugger;
                            break;
                        case ThreadInterruptFlags.PauseDebugger:
                            proc.AsyncStop().WaitOne();
                            // wait for set event
                            m_DebuggerPause.WaitOne();
                            m_InterruptFlags = m_InterruptFlags ^ ThreadInterruptFlags.PauseDebugger;
                            break;
                        default:
                            //m_cancel = true;
                            break;
                    }
                #endregion
                    m_SignalDebugger.Reset(); // we should be done now.
                if (m_cancel) break; // exit the loop if the main thread wants to stop debugging
                object procStopReason = proc.StopReason;
                // Process is now stopped. proc.StopReason tells us why we stopped.
                // The process is also safe for inspection.
                LogMessageStopReason msg = procStopReason as LogMessageStopReason;
                if (msg != null)
                {
                    TraceMessage traceMessage = new TraceMessage(msg.Message, msg.Name, msg.SwitchName);
                    MessageEventArgs e=new MessageEventArgs(traceMessage);
                    onNewMessage(e);
                }
                #region Stop because new assembly loaded
                AssemblyLoadedStopReason srAssemblyLoad = procStopReason as AssemblyLoadedStopReason;
                if (srAssemblyLoad != null) //a new assembly has been loaded
                {
                    // The reason for not simply firing a module loaded event from DebugEngine
                    // is simply that the same module can load over and over again...
                    // So what we do is check the module list of the MDbgProcess against our list
                    // Some modules do not even have a filename - so explains all of this fiddlefaddle
                   
                    foreach (MDbgModule module in proc.Modules)
                        {
                           string assemblyFilespec=module.CorModule.Name;
                           if (Modules.Find(delegate(Module mod)
                           {
                               return mod.FileName == assemblyFilespec;
                           }) == null &&
                               Modules.Find(delegate(Module mod)
                           {
                               return mod.Name == "0x"+module.CorModule.BaseAddress.ToString("X");
                           }) == null)
                              {
                                  string assemblyFullName = String.Empty;
                                  if (DebugEngineUtils.FileNameIsValid(assemblyFilespec))
                                  {
                                      try
                                      {
                                          assemblyFullName = Module.GetFullNameFromAssemblyFile(assemblyFilespec);
                                      }
                                      catch
                                      {
                                          assemblyFullName = System.IO.Path.GetFileNameWithoutExtension(assemblyFilespec);
                                          System.Diagnostics.FileVersionInfo fi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyFilespec);
                                          assemblyFullName += String.Format(", Version={0}", fi.FileVersion);
                                      }
                                  }
                                  else assemblyFullName = "0x"+module.CorModule.BaseAddress.ToString("X");
                                  Module mod = new Module(assemblyFullName, assemblyFilespec, module.SymbolFilename);
                                  Modules.Add(mod);
                                  if (ModuleLoad != null) ModuleLoad(this, new ModuleLoadedEventArgs(mod));
                              }
                            }
                        }
                #endregion
                ExceptionThrownStopReason srException = procStopReason as ExceptionThrownStopReason;
                BreakpointHitStopReason srBreakpoint = procStopReason as BreakpointHitStopReason;
                 // This is an exception or breakpoint
                 if (srException != null|| srBreakpoint!=null)
                 {
                     MDbgThread t = proc.Threads.Active;
                     MDbgValue ex = t.CurrentException;
                     MDbgFrame f = null;
                     RedFlag.Exception rfException = new RedFlag.Exception();   // Breakpoint or exception will still be an exception either way
                        if (ex != null) 
                        {
                            MDbgValue vExceptionMsg = null;
                            MDbgValue vExceptionHresult = null;
                            MDbgValue vExceptionFile = null;
                            try
                            {
                                if (m_ExceptionsToIgnore.Find(delegate(string exType) { return exType.Equals(ex.TypeName, StringComparison.InvariantCultureIgnoreCase); }) != null) continue;
                                vExceptionMsg = ex.GetField("_message"); // normally this has a value
                                // Exception Href - usually SystemException, but some ex types have meaningful Href like IOException
                                vExceptionHresult = ex.GetField("_HResult"); // normally this has a value
                                // for System.IO.IOException, we can get the offending filename
                                if (ex.TypeName.StartsWith("System.IO", StringComparison.OrdinalIgnoreCase)) vExceptionFile = ex.GetField("_maybeFullPath"); // only with System.IO.IOException
                            }
                            catch (MDbgValueException) { }
                            if (vExceptionMsg!=null && !vExceptionMsg.IsNull) rfException.Message = vExceptionMsg.GetStringValue(false);
                            if (vExceptionFile!=null && !vExceptionFile.IsNull) rfException.MayBeFullPath = vExceptionFile.GetStringValue(false);
                            if (vExceptionHresult!=null && !vExceptionHresult.IsNull)
                            {
                                byte[] Data = new byte[4];
                                long read = proc.CorProcess.ReadMemory(vExceptionHresult.CorValue.Address, Data);
                                Int32 hres = BitConverter.ToInt32(Data, 0);
                                rfException.HResult = hres;
                            }
                            // Type of exception, ie System.IO.IOException
                            rfException.Name = ex.TypeName;
                            // Need to check the StopReason to actually work our whether this is a first or second-chancer
                            if (srException!=null)
                            rfException.Handled = srException.EventType ==
                                Microsoft.Samples.Debugging.CorDebug.NativeApi.CorDebugExceptionCallbackType.DEBUG_EXCEPTION_UNHANDLED
                                ? false : true;
                        }
                        if (srBreakpoint!=null)
                        {
                            rfException.Name = "RedFlag.BreakpointReached";
                            if (srBreakpoint.Breakpoint.GetType().Name=="UserEntryBreakpoint")
                                rfException.Message = "User entry breakpoint";
                            else
                             rfException.Message = "A breakpoint has been hit at " + SetBreakpoint;
                        }
                        rfException.Time = DateTime.Now.Subtract(m_StartTime);
                        // can walk stack?
                        if (t.HaveCurrentFrame) f = t.CurrentFrame;
                        f = t.CurrentFrame; 
                        for (int i = 0; i < MaxStackDepth; i++)
                        {
                            if (f != null) // frame can be null if number of frames < stack length
                            {
                                RedFlag.Method rfMethod = new RedFlag.Method(); 
                                // add the args
                                List<MethodArgument> rfArgs = new List<MethodArgument>();
                                // add private members
                                List<StackObject> sobjs = new List<StackObject>();
                                MDbgValue[] localVars = null;
                                try
                                {
                                    localVars = f.Function.GetActiveLocalVars(f);
                                }
                                catch (COMException)
                                {
                                    continue; /* it's possible to get "no current frame" during shutdown */
                                }
                                if (localVars!=null)
                                {
                                foreach (MDbgValue v in localVars)
                                            DebugEngineUtils.GetAllMembers(v, MaxObjectDepth, MaxObjectDepth, GetArrays, ref sobjs);
                                }
                               
                                rfMethod.PrivateMembers = sobjs;
                                rfMethod.Name = f.Function.FullName;
                                if (f.SourcePosition != null)
                                    {
                                        rfMethod.SourceFile = f.SourcePosition.Path;
                                        rfMethod.SourceLine = f.SourcePosition.Line;
                                    }
                                    if (f.CorFrame.GetArgumentCount() > -1)
                                    {
                                        foreach (MDbgValue val in f.Function.GetArguments(f))
                                        {
                                            MethodArgument arg = new MethodArgument();
                                            arg.Name = val.Name;
                                            arg.Type = val.TypeName;
                                            arg.Value = val.GetStringValue(0, true);
                                            rfArgs.Add(arg);
                                        }

                                    }
                                rfMethod.Arguments = rfArgs;
                                // TODO... Not implemented in COR
                               // System.Reflection.MethodInfo mi = f.Function.MethodInfo;
                               // rfMethod.ReturnType=mi.ReturnType.FullName;
                                rfException.Methods.Add(rfMethod);
                            }
                            if (f!=null) f = f.NextUp;
                        }
                        onNewException(rfException);
                }
            }
            if (m_cancel && proc != null && !(proc.StopReason is MDbgInitialContinueNotCalledStopReason))
             {
                 // If we have requested a detach, stop the process
                 proc.CorProcess.Stop(20000);
                 proc.CorProcess.Detach();
             }
            if (ProcessStatus != null) onStatusChange("Idle...",EngineStatus.Idle,ProcessId);
        }
        void CorProcess_OnModuleLoad(object sender, Microsoft.Samples.Debugging.CorDebug.CorModuleEventArgs e)
        {
            e.Module.JITCompilerFlags = Microsoft.Samples.Debugging.CorDebug.CorDebugJITCompilerFlags.CORDEBUG_JIT_DISABLE_OPTIMIZATION;
        }
        void CorProcess_OnAppDomainExit(object sender, Microsoft.Samples.Debugging.CorDebug.CorAppDomainEventArgs e)
        {
            this.AppDomains.Remove(new AppDomain(e.AppDomain.Name));
        }

        void CorProcess_OnCreateAppDomain(object sender, Microsoft.Samples.Debugging.CorDebug.CorAppDomainEventArgs e)
        {
            this.AppDomains.Add(new AppDomain(e.AppDomain.Name));
        }

        /// <summary>
        /// Signal the debugging loop to exit and detach the debugger
        /// </summary>
        /// <param name="worker">The thread that is running this debugging session</param>
        public virtual void StopDebugging(System.Threading.Thread worker)
        {
            if (ProcessStatus != null) onStatusChange("Detaching from " + ProcessName, EngineStatus.Detaching,ProcessId);
            m_InterruptFlags = m_InterruptFlags | ThreadInterruptFlags.StopDebugger;
            m_cancel = true; // Setting this will break the debugging loop after setting the WaitHandle
            //worker.Interrupt();
            m_SignalDebugger.Set();
            DateTime tmr = DateTime.Now;
            while (m_Status != EngineStatus.Idle)
            {
                Thread.Sleep(1000);
                if (DateTime.Now.Subtract(tmr) > TimeSpan.FromSeconds(10)) break;
            }
        }
        protected void onModuleLoad(RedFlag.Module module)
        {
            if (ModuleLoad!=null)
                ModuleLoad(this,new ModuleLoadedEventArgs(module));
        }
        /// <summary>
        /// Pass the detected exception to the main thread
        /// </summary>
        /// <param name="exc"></param>
        protected void onNewException(RedFlag.Exception exc)
        {
            EventArgs args=new EventArgs();
            if (NewException!=null) NewException(exc, args);
        }
        /// <summary>
        /// Notify the main thread about what the debugger is doing
        /// </summary>
        /// <param name="message">Message to pass along</param>
        /// <param name="stat">The current value for the EngineStatus</param>
        protected void onStatusChange(string message,EngineStatus stat,int pid)
        {
            this.m_Status = stat;
            ProcessEventArgs arg = new ProcessEventArgs();
            arg.Message = message;
            arg.Status = stat;
            arg.ProcessId = pid;
            ProcessStatus(this, arg);
        }
        protected void onNewMessage(MessageEventArgs e)
        {
            if (NewMessage != null)
            {
                NewMessage(this, e);
            }
        }
        // Skip past fake attach events. 
        static void DrainAttach(MDbgEngine debugger, MDbgProcess proc)
        {
            bool fOldStatus = debugger.Options.StopOnNewThread;
            debugger.Options.StopOnNewThread = false; // skip while waiting for AttachComplete

            proc.Go().WaitOne();

            debugger.Options.StopOnNewThread = true; // needed for attach= true; // needed for attach

            // Drain the rest of the thread create events.
            while (proc.CorProcess.HasQueuedCallbacks(null))
            {
                proc.Go().WaitOne();
            }

            debugger.Options.StopOnNewThread = fOldStatus;
        }


    }
    
}

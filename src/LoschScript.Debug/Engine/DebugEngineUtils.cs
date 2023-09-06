using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Microsoft.Samples.Debugging.MdbgEngine;
using System.Reflection;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using System.Text.RegularExpressions;

namespace RedFlag.Engine
{

    class DebugEngineUtils
    {
        /// <summary>
        /// Count up the types of object from all threads
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="ProcessId"></param>
        /// <returns></returns>
        public static ExceptionInfo.HeapStatistics HeapStats(MDbgProcess proc, int Depth, int MaxDepth, bool GetArrays)
        {
           ExceptionInfo.HeapStatistics statistics=new ExceptionInfo.HeapStatistics();
            MDbgThreadCollection tc = proc.Threads;
            int iDepth = Depth;
            int iMaxDepth = MaxDepth;
            foreach (MDbgThread t in tc)
            {
                
                foreach (MDbgFrame f in t.Frames)
                {
                    List<StackObject> oList=new List<StackObject>();
                    if (f!=null && f.CorFrame.GetLocalVariablesCount() > 0)
                    {
                        foreach (MDbgValue v in f.Function.GetActiveLocalVars(f))
                        {
                            try
                            {
                                GetAllMembers(v, iDepth, iMaxDepth, GetArrays, ref oList);
                            }
                            catch { }
                        }
                        DedupCollection(oList);
                        foreach (StackObject sO in oList)
                        {
                            if (statistics[sO.Type]==null)
                                statistics.Add(new ExceptionInfo.HeapStatistic(sO.Type,1,sO.Size));
                            else 
                            {
                                 ExceptionInfo.HeapStatistic stat=statistics[sO.Type];
                                 stat.TypeCount++;
                                 stat.TypeSize+=sO.Size;
                            }
                        }
                    }
                }
            }
            return statistics;
        }
        /// <summary>
        /// .NET 2 way of .NET 3.5 select Distinct!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<T> DedupCollection<T> ( IEnumerable<T> input ) { 
        HashSet<T> passedValues = new HashSet<T>(); 
 
        //relatively simple dupe check alg used as example 
        foreach( T item in input) 
            if( passedValues.Contains(item) ) 
            continue; 
            else {
            passedValues.Add(item); 
            yield return item; 
                } 
        } 
        /// <summary>
        /// Make a string describing a stack dump
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="ProcessId"></param>
        /// <param name="m_Threads"></param>
        /// <returns></returns>
        public static string DumpStacks(MDbgProcess proc, int ProcessId,Dictionary<int,int[]>m_Threads)
        {
            StringBuilder sb = new StringBuilder();
            MDbgThreadCollection tc = proc.Threads;
            sb.Append(String.Format("Attached to pid:{0}\r\n\r\n", ProcessId));
            foreach (MDbgThread t in tc)
            {
                sb.Append(String.Format("Thread {0} [Status={1} UserTime={2}", t.Id, ((System.Diagnostics.ThreadState)m_Threads[t.Id][0]).ToString(),m_Threads[t.Id][2]));
                if (m_Threads[t.Id][0] == 5)
                    sb.Append(" Wait Reason=" + ((System.Diagnostics.ThreadWaitReason)m_Threads[t.Id][1]).ToString());
                sb.Append("]\r\n");
                sb.Append(String.Format("Callstack for Thread {0}\r\n", t.Id.ToString()));

                foreach (MDbgFrame f in t.Frames)
                {
                    
                    sb.Append(" ");
                    try
                    {
                        sb.Append(f.Function.FullName);
                    }
                    catch (COMException)
                    {
                        // avoid "code is currently unavailable"
                        sb.Append("<No code for this function>");
                        continue;
                    }
                    sb.Append("(");
                    if (f.CorFrame.GetArgumentCount() > -1)
                    {
                        MDbgValue[] arguments = f.Function.GetArguments(f);
                        for (int i = 0; i < arguments.Length; i++)
                        {
                            MethodArgument arg = new MethodArgument();
                            sb.Append(arguments[i].TypeName);
                            if (i<arguments.Length-1) sb.Append(",");
                        }

                    }
                    sb.Append(")");
                    if (f.SourcePosition != null)
                    {
                        sb.Append(" "+f.SourcePosition.Path);
                        sb.Append(":" + f.SourcePosition.Line);
                    }
                    sb.Append("\r\n");
                   // sb.Append("  " + f + "\r\n");
                }
                sb.Append("\r\n");
            }
            return sb.ToString();

        }
        /// <summary>
        /// Get the status os threads for a process
        /// </summary>
        /// <param name="ProcessId"></param>
        /// <returns>key=threadID, value=status,waitreason</returns>
        public static Dictionary<int, int[]> GetThreadStatus(int ProcessId)
        {
            Dictionary<int, int[]> threads = new Dictionary<int, int[]>();
            foreach (System.Diagnostics.ProcessThread thread in System.Diagnostics.Process.GetProcessById(ProcessId).Threads)
            {
                int[] status = new int[3] { (int)thread.ThreadState, 0, (int) thread.UserProcessorTime.TotalMilliseconds };
                if (status[0] == 5) status[1] = (int)thread.WaitReason;
                threads.Add(thread.Id, status);

            }
            return threads;
        }
        /// <summary>
        /// Add the current stack object, and all dependent objects, recursively to a list
        /// </summary>
        /// <param name="val">The current memory object</param>
        /// <param name="MaxDepth">The maximum depth of dependent objects</param>
        /// <param name="StackObjects">Refers to a list created outside of this method</param>
        public static void GetAllMembers(MDbgValue val, int Depth, int MaxDepth, bool GetArrays, ref List<StackObject> StackObjects)
        {
            StackObject sobj = null;
            sobj = new StackObject();
            sobj.Name = val.Name;
            sobj.Type = val.TypeName;
            sobj.Value = val.GetStringValue(false);
            if (val.CorValue != null)
            {
                sobj.Size = val.CorValue.Size;
                sobj.Address = val.CorValue.Address;
            }
            sobj.ObjectDepth = MaxDepth - Depth;
            StackObjects.Add(sobj);

            if (val.IsComplexType)
            {
                sobj.ComplexType = true;
                Depth--;
                // if val is a type it will fail with a MethodNotImplementedException?
                MDbgValue[] subvals = null;
                try
                {
                    subvals = val.GetFields();
                }
                catch (NotImplementedException)
                {

                }
                if (subvals != null)
                {
                    foreach (MDbgValue v2 in subvals)
                    {

                        if (Depth > 0)
                            GetAllMembers(v2, Depth, MaxDepth, GetArrays, ref StackObjects);
                    }
                }

            }
            if (val.IsArrayType && GetArrays) //This SERIOUSLY slows things down!!!
            {
                Depth--;
                foreach (MDbgValue v2 in val.GetArrayItems())
                {
                    if (Depth > 0)
                        GetAllMembers(v2, Depth, MaxDepth, GetArrays, ref StackObjects);
                }
            }
            // Release "val"
            //Marshal.FinalReleaseComObject(val.CorValue.ComObject);
            val = null;
        }
        /// <summary>
        /// If the file is an assembly, gets the runtime version. If not,returns the default runtime version
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static string GetAssemblyRuntimeVersion(string FilePath, string Defaultversion)
        {
            string dotNetVersion = Defaultversion;
            if (String.IsNullOrEmpty(FilePath)) return Defaultversion;
            StringBuilder sb=new StringBuilder(20);
            uint capreq = 0;
            uint capreq2 = 20;
            if (System.Environment.Version.Major > 3)
            {
                Guid CLSID_MetaHost = new Guid("9280188D-0E8E-4867-B30C-7FA83884E8DE");
                Guid IID_MetaHost = new Guid("D332DB9E-B9B3-4125-8207-A14884F53216");
                ICLRMetaHost metahost = (ICLRMetaHost)ClrCreateInterface(CLSID_MetaHost, IID_MetaHost);
                try
                {
                    metahost.GetVersionFromFile(FilePath, sb, ref capreq2);
                }
                catch (BadImageFormatException) {/*unmanaged code*/ }

                if (!String.IsNullOrEmpty(sb.ToString())) dotNetVersion = sb.ToString();
                Marshal.FinalReleaseComObject(metahost);
            }
            else
            {
                try
                {
                    GetFileVersion(FilePath, sb, capreq2, ref capreq2);
                }
                catch (BadImageFormatException) {/*unmanaged code*/ }
                if (!String.IsNullOrEmpty(sb.ToString())) dotNetVersion = sb.ToString();
            }
            return dotNetVersion;
        }
        public static string GetWindowsVersion()
        {
            string winVer = "Unknown Windows Version";
            try
            {
                winVer = System.Environment.OSVersion.ToString();
            }
            catch { }
            return winVer;
        }
        public static Int64 ProcessMemoryUsage()
        {
            Int64 totalMemory = 0;
            totalMemory = System.Environment.WorkingSet;
            return totalMemory;
        }
        public static bool IAm64Bit()
        {
            return IntPtr.Size == 8 ? true : false;
        }
        public static string MyCommandLine()
        {
            string argString = "could not get commandline";
            try
            {
                string wmiQuery = string.Format("select CommandLine from Win32_Process where ProcessId='{0}'", Process.GetCurrentProcess().Id);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject instance in collection)
                {
                    argString = (string)instance["CommandLine"];

                }
            }
            catch {/*don't care */ }
            return argString;
        }
        public static bool FileNameIsValid(string FileName)
        {
            string s = new string(System.IO.Path.GetInvalidPathChars(), 0, System.IO.Path.GetInvalidPathChars().Length);
            Regex r = new Regex("[" + Regex.Escape(s) + "]");
            if (r.IsMatch(FileName)) return false;
            if (!System.IO.File.Exists(FileName)) return false;
            return true;
        }
        private const string Ole32LibraryName = "ole32.dll";
        private const string ShimLibraryName = "mscoree.dll";
        [DllImport(ShimLibraryName, PreserveSig = false, EntryPoint = "CLRCreateInstance")]
        [return: MarshalAs(UnmanagedType.Interface)]
        public static extern object ClrCreateInterface(
                [MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
                [MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        [DllImport(ShimLibraryName, PreserveSig = false, EntryPoint = "GetFileVersion")]
        public static extern void GetFileVersion([In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzBuffer, [In] uint buflen, [In, Out] ref uint pcchBuffer);
    }
}

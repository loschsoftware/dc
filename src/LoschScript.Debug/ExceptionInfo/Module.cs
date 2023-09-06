using RedFlag.Engine.AssemblyInspectorImplementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedFlag
{
    [Serializable]
    public class Module
    {
        private string m_ModuleName = String.Empty;
        private string m_FileName=String.Empty;
        private string m_SymbolPath = String.Empty;
        public Module() { }
        public Module(string name,string path,string symbols)
        {
            m_ModuleName = name;
            m_FileName = path;
            m_SymbolPath = symbols;
        }
        public string Name
        {
            get
            {
                return m_ModuleName;
            }
            set
            {
                m_ModuleName = value;
            }
        }
        public string FileName
        {
            get
            {
                return m_FileName;
            }
            set
            {
                m_FileName = value;
            }
        }
        public string SymbolFile
        {
            get
            {
                return m_SymbolPath;
            }
            set
            {
                m_SymbolPath = value;
            }
        }
        public override string ToString()
        {
            return m_ModuleName;
        }
        public static string GetFullNameFromAssemblyFile(string FileName)
        {
            string fullName = String.Empty;
            if (!RedFlag.Engine.DebugEngineUtils.FileNameIsValid(FileName)) return fullName;
            
                byte[] assemblyBuffer = System.IO.File.ReadAllBytes(FileName);
                System.AppDomain tempAppDomain = System.AppDomain.CreateDomain("InspectorAppDomain");
                object anObject = tempAppDomain.CreateInstanceAndUnwrap(System.Reflection.Assembly.GetExecutingAssembly().FullName,
                    "RedFlag.Engine.AssemblyInspectorImplementation.AssemblyInspector");
                AssemblyInspector assemblyInspector = anObject as AssemblyInspector;
                Dictionary<string, string> attrs = assemblyInspector.GetAssemblyInfo(assemblyBuffer);
                fullName = attrs["FullName"];
            /* This needs an unload but that causes w3wp to flake out because of the ThreadAbortExceptions
             * that filter down from RedFlag to w3wp when it is hosting the worker process */
             //    System.AppDomain.Unload(tempAppDomain);
            return fullName;
        }
        /// <summary>
        /// TODO: This doesn't work because the address is not relative to the process so we get protected memory violation
        /// </summary>
        /// <param name="StartAddress"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public static string GetFullNameFromAssemblyAddress(long StartAddress, long Length)
        {
            string fullName = String.Empty;
            byte[] assemblyBuffer = new byte[Length];
            //Process.CorProcess.ReadMemory(StartAddress, assemblyBuffer);
            for (int i=0; i<Length-1;i++)
            {
                assemblyBuffer[i] = System.Runtime.InteropServices.Marshal.ReadByte(((IntPtr)(StartAddress + i)), i);
            }
            System.AppDomain tempAppDomain = System.AppDomain.CreateDomain("InspectorAppDomain");
            object anObject = tempAppDomain.CreateInstanceAndUnwrap(System.Reflection.Assembly.GetExecutingAssembly().FullName,
                "RedFlag.Engine.AssemblyInspectorImplementation.AssemblyInspector");
            AssemblyInspector assemblyInspector = anObject as AssemblyInspector;
            try
            {
                Dictionary<string, string> attrs = assemblyInspector.GetAssemblyInfo(assemblyBuffer);
                fullName = attrs["FullName"];
            }
            catch { }
            
            System.AppDomain.Unload(tempAppDomain);
            return fullName;
        }
    }
}

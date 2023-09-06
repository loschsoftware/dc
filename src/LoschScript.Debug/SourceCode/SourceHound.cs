using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using RedFlag.Engine.AssemblyInspectorImplementation;

namespace RedFlag.SourceCode
{
    /// <summary>
    /// Class that gets the source code - if there is none try Reflector
    /// </summary>
    public class SourceHound
    {
        private string m_Reflectorpath = String.Empty;
        public bool ReflectorInstalled = false;
        public SourceHound()
        {
            // get the path to Reflector
            RegistryKey key = null;
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(@"Applications\Reflector.exe\shell\open\command");
                if (key == null) key = Registry.ClassesRoot.OpenSubKey(@"code\shell\open\command"); // dnr 8.x
            }
            catch { }
                if (key != null)
                {
                    string registryPath = (string)key.GetValue("");
                    Regex r = new Regex(@"[a-zA-Z]{1}:\\.*Reflector.exe");
                    Match m=r.Match(registryPath);
                    m_Reflectorpath = m.Value;
                    ReflectorInstalled = m.Success;
                }
            

        }
        public void OpenSourceFile(Method TargetMethod, List<Module> LoadedModules)
        {
            if (ReflectorInstalled && (
                String.IsNullOrEmpty(TargetMethod.SourceFile) ||
                !File.Exists(TargetMethod.SourceFile)
                ))
            {
                // Find the module that the source should be in
                string moduleFile = GetModuleContainingMethod(TargetMethod.Name, LoadedModules);
                if (String.IsNullOrEmpty(moduleFile)) throw new ArgumentNullException("Target module could not be found.\r\nThe module may have a dependency that could not be loaded");
                try
                {
                    string methodNameSpace = TargetMethod.Name.Substring(0, TargetMethod.Name.LastIndexOf('.'));
                    StringBuilder argBuilder=new StringBuilder(50);
                    argBuilder.Append("(");
                    for (int i=0;i<TargetMethod.Arguments.Count;i++)
                    {
                        string arg=TargetMethod.Arguments[i].Type;
                        //have to remove "System" from base types
                        if (arg.StartsWith("System.") && arg.Split('.').Length==2)
                            argBuilder.Append(arg.Substring(arg.LastIndexOf('.')+1));
                        else argBuilder.Append(arg);
                        if (i < TargetMethod.Arguments.Count - 1) argBuilder.Append(",");
                    }
                    argBuilder.Append(")");
                    // TODO: we can be much more accurate if we know the return type and add to the end (:retType)
                    string methodName = TargetMethod.Name.Substring(methodNameSpace.Length+1)+argBuilder.ToString();

                    OpenInReflector(moduleFile, methodNameSpace,methodName);
                }
                catch (System.Exception ex)
                {
                    throw new InvalidOperationException(ex.Message);
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(TargetMethod.SourceFile)
                    && File.Exists(TargetMethod.SourceFile))
                OpenInNotepad(TargetMethod.SourceFile);
            }
        }
        public void OpenInNotepad(string SourceFile)
        {
            string notePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "notepad.exe");
            ExecuteNonShell(notePath, SourceFile);
        }
        public void OpenInReflector(string ModuleFileName, string MethodNameSpace, string MethodName)
        {
            string reflectorArgs = String.Format(
           "/share /select:\"code://locatedAssembly:{0}/{1}/{2}\"",
            ModuleFileName, MethodNameSpace,MethodName);
            ExecuteNonShell(m_Reflectorpath, reflectorArgs);
        }
        private void ExecuteNonShell(string Command, string Arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo(Command, Arguments);
            psi.UseShellExecute = false;
            Process.Start(psi);
        }
        private string GetModuleContainingMethod(string MethodName, List<Module> ModuleList)
        {
            String moduleFileName = String.Empty;
            System.AppDomain tempDomain=System.AppDomain.CreateDomain("ReflectionOnly");
            string methodTypeName=MethodName.Substring(0,MethodName.LastIndexOf('.'));
            string methodMethodName=MethodName.Substring(MethodName.LastIndexOf(".")+1);
            object anObject = tempDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                           "RedFlag.Engine.AssemblyInspectorImplementation.AssemblyInspector");
            AssemblyInspector assemblyInspector = anObject as AssemblyInspector;
           
            foreach (Module mod in ModuleList)
            {
                if (File.Exists(mod.FileName))
                {
                    // load module and reflect for method
                    byte[] assemblyBuffer = System.IO.File.ReadAllBytes(mod.FileName);
                    if(assemblyInspector.IsMethodInAssembly(assemblyBuffer, methodTypeName, methodMethodName))
                    {
                        moduleFileName = mod.FileName;
                        break;
                    }
                }
            }
            System.AppDomain.Unload(tempDomain);
            return moduleFileName;
        }
    }
}

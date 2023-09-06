using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;

namespace RedFlag.Engine.AssemblyInspectorImplementation
{

    public class AssemblyInspector : MarshalByRefObject, IAssemblyInspector
    {
        #region IAssemblyInspector Members
        public Dictionary<string,string> GetAssemblyInfo(byte[] assemblyBuffer)
        {
            Dictionary<string,string> result = new Dictionary<string,string>();
            Assembly assembly = System.AppDomain.CurrentDomain.Load(assemblyBuffer);
            
            System.Reflection.Module manifest = assembly.ManifestModule;
            result.Add("ScopeName",manifest.ScopeName);
            result.Add("FullName", manifest.Assembly.FullName);

            return result;
        }
        public Type GetTypeHandle(byte[] assemblyBuffer,string typeName)
        {
            Assembly assembly = System.AppDomain.CurrentDomain.Load(assemblyBuffer);
            Type t = assembly.ManifestModule.GetType(typeName);
            return t;
        }
        public bool IsMethodInAssembly(byte[] assemblyBuffer, string TypeName, string MethodName)
        {
            Assembly assembly = System.AppDomain.CurrentDomain.Load(assemblyBuffer);
            try
            {
                Type t = assembly.ManifestModule.GetType(TypeName, false, true);
                if (t == null) return false;
                MethodInfo mi = t.GetMethod(MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null) return true;
            }
            catch (AmbiguousMatchException)
            {
                return true;
            }
            catch (System.IO.FileNotFoundException) // this is a good thing - it means we hit the mark but there is a dependency
            {
                return true;
            }
            catch { return false; }
            return false;
        }
        #endregion
    }
    public interface IAssemblyInspector
    {
        Dictionary<string,string> GetAssemblyInfo(byte[] assembly);
        Type GetTypeHandle(byte[] assemblyBuffer, string typeName);
        bool IsMethodInAssembly(byte[] assemblyBuffer, string TypeName, string methodName);
    }
}

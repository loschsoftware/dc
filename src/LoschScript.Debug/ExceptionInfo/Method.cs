using System;
using System.Collections.Generic;
using System.Text;

namespace RedFlag
{
    [Serializable]
    public class Method
    {
        private List<MethodArgument> m_Arguments = new List<MethodArgument>();
        private string m_MethodName = String.Empty;
        private string m_MethodSignature = String.Empty;
        private List<StackObject> m_PrivateMembers = new List<StackObject>();
        private string m_ReturnType=String.Empty;
        private int m_SourceLine = 0;
        private string m_SourcePath = String.Empty;
        /// <summary>
        /// Create a method object from a method signature
        /// </summary>
        /// <param name="MethodSignature">Format MUST be method(type,type...)</param>
        public Method(string MethodSignature)
        {
           // method format should be name(type name, type name...)
            // Note there is no sanity checking as the Engine should be making the sig in this format
            m_MethodSignature = MethodSignature;
            m_MethodName=m_MethodSignature.Substring(0,m_MethodSignature.IndexOf('('));
            string methodArgStr = m_MethodSignature.Substring(m_MethodName.Length+1).Trim(')');
            string[] methodArgArray = methodArgStr.Split(',');
            foreach (string arg in methodArgArray)
            {
                MethodArgument methodArgument = new MethodArgument();
                methodArgument.Type = arg;
                m_Arguments.Add(methodArgument);
            }
        }
        public Method() { }
        /// <summary>
        /// Only the method name, not the full signature
        /// </summary>
        public string Name
        {
            get
            {
                return m_MethodName;
            }
            set
            {
                m_MethodName = value;
            }
        }
        /// <summary>
        /// The type of object this method returns
        /// </summary>
        public string ReturnType
        {
            get
            {
                return m_ReturnType;
            }
            set
            {
                m_ReturnType = value;
            }
        }
        /// <summary>
        /// The method signature (method name and parameter types)
        /// </summary>
        public string Signature
        {
            get
            {
                string argString = String.Empty;
                foreach (MethodArgument arg in m_Arguments)
                {
                    argString += arg.Type + ",";
                }
                if (m_SourceLine>0)
                    return String.Format("{0}({1}) {2} line: {3}", m_MethodName, argString.TrimEnd(','),m_SourcePath,m_SourceLine);
                else
                return String.Format("{0}({1})", m_MethodName, argString.TrimEnd(','));
            }
        }
        /// <summary>
        /// The arguments to the method (arguments include name, type, and value)
        /// </summary>
        public List<MethodArgument> Arguments
        {
            get
            {
                return m_Arguments;
            }
            set
            {
                m_Arguments = value;

            }
        }
        /// <summary>
        /// The variables that are private to the method
        /// </summary>
        public List<StackObject> PrivateMembers
        {
            get
            {
                return m_PrivateMembers;
            }
            set
            {
                m_PrivateMembers = value;

            }
        }
        /// <summary>
        /// The values of the arguments passed to the method
        /// </summary>
        public string[] Parameters
        {
            get
            {
                int counter = 0;
                string[] argVals = new string[m_Arguments.Count];
                foreach (MethodArgument arg in m_Arguments)
                {
                    argVals[counter] = arg.Value;
                    counter++;
                }
                return argVals;
            }

        }
        /// <summary>
        /// The line of source code
        /// </summary>
        public int SourceLine
        {
            get
            {
                return m_SourceLine;
            }
            set
            {
                m_SourceLine = value;
            }
        }
        /// <summary>
        /// The code file
        /// </summary>
        public string SourceFile
        {
            get
            {
                return m_SourcePath;
            }
            set
            {
                m_SourcePath = value;
            }
        }
    }
    [Serializable]
    public class MethodArgument
    {
        private string m_Name;
        private string m_Type;
        private string m_Value;
        /// <summary>
        /// The variable name of the argument
        /// </summary>
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }
        /// <summary>
        /// The type of variable (System.Int32, etc)
        /// </summary>
        public string Type
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
            }
        }
        /// <summary>
        /// The value that the argument was set to
        /// </summary>
        public string Value
        {
            get
            {
               return m_Value;
            }
            set
            {
                m_Value = value;
            }
        }
        /// <summary>
        /// Dump the argument name and value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(80);
            sb.Append(this.Type);
            sb.Append(":");
            sb.Append(this.Name);
            sb.Append("=");
            sb.Append(this.Value);
            return sb.ToString();
        }

    }
}

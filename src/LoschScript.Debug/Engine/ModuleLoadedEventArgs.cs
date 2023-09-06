using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedFlag.Engine
{
    public class ModuleLoadedEventArgs : EventArgs
    {
        RedFlag.Module m_Module = null;
        public ModuleLoadedEventArgs(RedFlag.Module module)
        {
            m_Module = module;
        }
        public RedFlag.Module Module
        {
            get
            {
                return m_Module;
            }
            set
            {
                m_Module = value;
            }
        }
    }
}

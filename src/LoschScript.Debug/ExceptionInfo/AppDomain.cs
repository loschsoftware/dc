using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace RedFlag
{
    [Serializable]
    public class AppDomain
    {
        public AppDomain() { }
        public AppDomain(string AppDomainName)
        {
            this.Name = AppDomainName;
        }
        [XmlAttribute]
        public string Name { get; set; }
    }
    [Serializable]
    public class AppDomains : List<RedFlag.AppDomain>
    {
        public AppDomain this[string AppDomainName]
        {
            get
            {
                foreach (AppDomain d in this)
                {
                    if (d.Name == AppDomainName) return d;
                }
                return null;
            }
            set { }
        }
    }
}

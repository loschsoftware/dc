using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;

namespace RedFlag
{
    class HostConfig
    {
        /// <summary>
        /// Disable the Win64 filesystem redirection. This will fail on Win32!
        /// This should be used on a seperate thread, since it's a per-thread
        /// call, and can break other things being loaded at the same time.
        /// </summary>
        /// <param name="oldValue"></param>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr oldValue);

        /// <summary>
        /// Restore the Win64 filesystem redirection. This will fail on Win32!
        /// </summary>
        /// <param name="oldValue"></param>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void Wow64RevertWow64FsRedirection(IntPtr oldValue);
        public static int CreateAlternateApplicationHostConfig(string destination, ref Uri url, bool profileOnNewPort, int newPort,string fileName)
        {
            XmlDocument xd = new XmlDocument();
            xd.Load(fileName);
            return CreateAlternateApplicationHostConfig(destination, ref url, profileOnNewPort,newPort,xd);
        }
        public static int CreateAlternateApplicationHostConfig(string destination, ref Uri url, bool profileOnNewPort, int newPort)
        {
            XmlDocument appHostConfig = GetApplicationHostConfig();
            return CreateAlternateApplicationHostConfig(destination, ref url, profileOnNewPort, newPort, appHostConfig);
        }
        /// <summary>
        /// Takes an IIS7-style applicationHost.config file and alters the appropriate bits
        /// such that we can run the desired site in our own application pool
        /// </summary>
        /// <param name="destination">Path of the newly created destination file</param>
        /// <param name="url">URL of the website we're going to profile - modifies this to include the Cunning New Port</param>
        /// <returns>The port number of a newly created port to send the user to</returns>
        public static int CreateAlternateApplicationHostConfig(string destination, ref Uri url, bool profileOnNewPort, int newPort, XmlDocument appHostConfig)
        {
            int port = newPort;
            String appPool = "RedFlagAppPool";
            /// Stuff that needs to be modified in here:
            /// 
            /// system.applicationHost
            ///     applicationPools
            ///         add name=...
            ///         remove all other app pools
            ///     sites
            ///         applicationDefaults
            ///             applicationPool => RG Pool
            ///         for all sites to profile:
            ///             serverAutoStart => true
            ///             for all applictions:
            ///                 applicationPool =>RG pool
            ///             binding
            ///                 bindingInformation => new port (optionally)
            ///         for all sites not to profile:
            ///             DELETE
            ///                 

            XmlNode root = appHostConfig.DocumentElement;
            // document the original config items we found
            StringBuilder docComments = new StringBuilder();
            docComments.Append("Original configuration found in appHost:\r\n");
            docComments.Append("Original Url: " + url+"\r\n");

            XmlNode systemAppHost = root["system.applicationHost"];
            XmlElement applicationPools = systemAppHost["applicationPools"];
            XmlElement sites = systemAppHost["sites"];
            // Get the id of the site to profile
            int siteToProfile = GetSiteForURL(url, appHostConfig);
            docComments.Append("Site ID: " + siteToProfile.ToString() +"\r\n");
                if (siteToProfile == 0)
            {
                throw new System.Exception("Couldn't determine the IIS Site associated with URL '" + url + "'. \n\n" +
                                       "Please check that the URL is serviced by the instance of IIS " +
                                       "running on this machine");
            }
            // Figure out which app pool the site we're profiling was in
            string apName = GetApplicationPool(url,siteToProfile,appHostConfig);
            docComments.Append(String.Format("App Pool Name: {0}\r\n" , apName));

            string originalAPName;
            if (apName != null)
                originalAPName = apName;
            else
                originalAPName = string.Empty;
            // Remove all application pools, except the one we need  
            foreach (XmlNode childNode in applicationPools.SelectNodes("add"))
            {
                if (!(childNode.Attributes["name"] != null && childNode.Attributes["name"].Value.Equals(apName, StringComparison.CurrentCultureIgnoreCase)))
                        applicationPools.RemoveChild(childNode);              
            }
   
           
           XmlNode rgPoolNode = applicationPools.SelectSingleNode("//add[@name='" + apName + "']");
           if (rgPoolNode != null) docComments.Append(rgPoolNode.InnerText + "\r\n");

           if (apName == "DefaultAppPool")
           {
               //rgPoolNode could be null -- sometimes DefaultAppPool doesn't exist
               if (rgPoolNode == null)
               {
                   rgPoolNode = appHostConfig.CreateNode(XmlNodeType.Element, "add", null);
                   XmlAttribute attrAppPoolName = appHostConfig.CreateAttribute("name");
                   attrAppPoolName.Value = apName;
                   rgPoolNode.Attributes.Append(attrAppPoolName);
                   applicationPools.AppendChild(rgPoolNode);
               }
               XmlNode appPoolDefaultsNode = applicationPools.SelectSingleNode("applicationPoolDefaults");
               if (appPoolDefaultsNode != null) docComments.Append("App Pool Defaults: "+appPoolDefaultsNode.InnerXml+"\r\n");
               //if all we're left with is default app pool, need to add the defaults to it
               if (appPoolDefaultsNode.Attributes !=null)
               {
                   for (int i = 0; i < appPoolDefaultsNode.Attributes.Count;i++)
                   {
                       try
                       {
                           if (rgPoolNode.Attributes[appPoolDefaultsNode.Attributes[i].Name]==null)
                           rgPoolNode.Attributes.Append(appPoolDefaultsNode.Attributes[i]);
                       }
                       catch (System.Exception) { }//don't care
                   }

               }
               foreach (XmlNode appDefault in appPoolDefaultsNode)
               {
                   if (rgPoolNode[appDefault.Name]==null)
                   rgPoolNode.AppendChild(appDefault);
        
               }
               //check default app pool has everything it needs
               if (rgPoolNode.Attributes["autoStart"] == null)
               {
                   XmlAttribute asPoolAttr = appHostConfig.CreateAttribute("autoStart");
                   asPoolAttr.Value = "true";
                   rgPoolNode.Attributes.Append(asPoolAttr);
               }
               if (rgPoolNode.Attributes["managedPipelineMode"] == null)
               {
                   XmlAttribute asPipeAttr = appHostConfig.CreateAttribute("managedPipelineMode");
                   asPipeAttr.Value = "Integrated";
                   rgPoolNode.Attributes.Append(asPipeAttr);
               }
           }
           //change the pool name to our test one
           if (rgPoolNode!=null) rgPoolNode.Attributes["name"].Value = appPool;
           else 
           {
               XmlAttribute newPoolName=appHostConfig.CreateAttribute("name");
               newPoolName.Value=appPool;
               applicationPools["add"].Attributes.Append(newPoolName);
           }
           
            // Change the default app pool
            sites["applicationDefaults"].Attributes["applicationPool"].Value = appPool;
            // If the site's default application lists an app pool name, replace that
            XmlAttribute defaultSiteApp=sites.SelectSingleNode("site[@id='"+siteToProfile+"']/application[@path='/']").Attributes["applicationPool"];
            if (defaultSiteApp != null) defaultSiteApp.Value = appPool;
            //Modify applications in the target site to use the same app pool
            XmlNodeList apNodes = sites.SelectNodes("//site[@id='" + siteToProfile + "']/application");
            List<XmlNode> siteAppsToDrop = new List<XmlNode>();
            foreach (XmlNode apNode in apNodes)
            {
                if (apNode.Attributes["path"] != null && apNode.Attributes["path"].Value.Equals("/" + url.AbsolutePath.Split('/')[1], StringComparison.CurrentCultureIgnoreCase))
                {
                    docComments.Append("Application configuration:" + apNode.InnerXml);

                    if (apNode.Attributes["applicationPool"] != null) apNode.Attributes["applicationPool"].Value = appPool;
                    else // add the app pool attribute if it doesn't already exist
                    {
                        XmlAttribute apToAdd = appHostConfig.CreateAttribute("applicationPool");
                        apToAdd.Value = appPool;
                        apNode.Attributes.Append(apToAdd);
                    }
                }
                else
                {
                    // We must preserve the default application or the site won't run
                    if (apNode.Attributes["path"] != null && !apNode.Attributes["path"].Value.Equals("/"))
                        siteAppsToDrop.Add(apNode);
                }
            }
            
            // Change binding info and application pool for all sites
            int workingPort = port;
            List<XmlElement> sitesToKill = new List<XmlElement>();
            foreach (XmlElement site in sites.GetElementsByTagName("site"))
            {
                // If this site isn't going to be profiled, add it to the kill list
                int siteId = Convert.ToInt32(site.Attributes["id"].Value);
                if (siteId != siteToProfile)
                {
                    sitesToKill.Add(site);
                }

                // Set auto-start to true
                site.SetAttribute("serverAutoStart", "true");
                // MS have struck again and introduced an app default node here too!!!!1!!
                if (site["applicationDefaults"] != null && site["applicationDefaults"].Attributes["applicationPool"]!=null)
                    site["applicationDefaults"].Attributes["applicationPool"].Value = appPool;
                // Only do the following if we're profiling on a new port - if we're profiling
                // in place, there's no need, since we want to keep all the bindings the same
                if (profileOnNewPort)
                {
                    // Set binding info to new port, assuming the site we're currently on is the
                    // same as that we're interested in - otherwise, destroy it (so we don't end up
                    // starting lots of unnecessary sites).
                    XmlElement bindingsList = site["bindings"];
                    List<XmlNode> bindingsToKill = new List<XmlNode>();
                    // SHAREPOINT -- must eliminate any sites with no bindings
                    if (bindingsList == null) continue;
                    foreach (XmlNode bindingNode in bindingsList.ChildNodes)
                    {
                        if (bindingNode.Attributes["protocol"] != null /*<clear/>*/ && bindingNode.Attributes["protocol"].Value.Equals(url.Scheme, StringComparison.CurrentCultureIgnoreCase))
                        {
                            XmlAttribute bindingInfo = bindingNode.Attributes["bindingInformation"];
                            string[] bindings = bindingInfo.Value.ToString().Split(':');
                            int bindingPort = 80;
                            if (bindings.Length>1) bindingPort=Convert.ToInt32(bindings[1]);
                            if (bindingPort != url.Port)
                            {
                                // This site is running on a different port to the URL we were passed,
                                // and should therefore be DESTROYED!
                                bindingsToKill.Add(bindingNode); // (Can't kill it right now as it modifies the collection we're iterating over)
                            }

                            bindingInfo.Value = String.Format("{0}:{1}:{2}", bindings[0], workingPort, bindings[2]);
                        }
                        else bindingsToKill.Add(bindingNode);
                }
                    // Actually kill the bindings we wanted to earlier
                    foreach (XmlNode binding in bindingsToKill)
                    {
                        bindingsList.RemoveChild(binding);
                    }
                }
            }

            // Kill any sites we wanted to kill earlier
            foreach (XmlElement site in sitesToKill)
            {
                sites.RemoveChild(site);
            }
            //Delete applications from the target site
            XmlNode siteNode = sites.SelectSingleNode("//site[@id='" + siteToProfile + "']");
            foreach (XmlNode app in siteAppsToDrop)
            {
                siteNode.RemoveChild(app);
            }
            //Console.WriteLine("Launching site \"{0}\" app pool \"{1}\" on new app pool \"{2}\"", siteToProfile, apName, appPool);
            
            XmlNode commentNode = appHostConfig.CreateNode(XmlNodeType.Comment, "Original configuration", null);
            commentNode.Value = docComments.ToString();
            appHostConfig.AppendChild(commentNode);
            appHostConfig.Save(destination);

            // Modify the URL to include the port - iif we're running on a new port
            if (profileOnNewPort)
            {
                UriBuilder uriBuilder = new UriBuilder(url);
                uriBuilder.Port = port;
                url = uriBuilder.Uri;
               
            }

            return profileOnNewPort ? port : url.Port;
        }
        internal static XmlDocument GetApplicationHostConfig()
        {

            string apphostConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "inetsrv\\config\\applicationHost.config");
            XmlDocument appHostConfig = new XmlDocument();

            IntPtr temp = IntPtr.Zero;
            try
            {
                Wow64DisableWow64FsRedirection(ref temp);
            }
            catch
            {
            }

            if (!File.Exists(apphostConfigPath))
                throw new System.Exception("Couldn't locate applicationHost.config - ensure IIS is installed and you are running as Administrator");

            appHostConfig.Load(apphostConfigPath);

            try
            {
                Wow64RevertWow64FsRedirection(temp);
            }
            catch
            {
            }

            return appHostConfig;
        }
        internal static string GetApplicationPool(Uri url,int siteId,XmlDocument configFile)
        {
            string poolName = "DefaultAppPool"; //Probably need to check ApplicationPoolDefaults in future
            XmlNode root = configFile.DocumentElement;
            XmlNode site = root.SelectSingleNode("//configuration/system.applicationHost/sites/site[@id='"+siteId+"']");
                    // Check for application matching our vdir name
                    XmlNodeList applications = site.SelectNodes("application");
                    string vDirName = url.AbsolutePath.Split('/')[1];
                    foreach (XmlNode application in applications)
                    {
                        //XmlNode application = applications[indexer].SelectSingleNode("application");
                        if (application.Attributes["path"].Value.Trim('/').Equals(vDirName, StringComparison.InvariantCultureIgnoreCase))
                          if (application.Attributes["applicationPool"]!=null) poolName = application.Attributes["applicationPool"].Value;
                        
                    }
                     
             //and return the pool name
            return poolName;
        }
        internal static int GetSiteForURL(Uri url,XmlDocument configFile)
        {
            int siteId = 0;
            XmlNode root = configFile.DocumentElement;
            XmlNodeList siteCollection = root.SelectNodes("//configuration/system.applicationHost/sites/site");

            foreach (XmlNode site in siteCollection)
            {
                bool hostMatch = false;
                bool portMatch = false;
                bool ipMatch = false;
                string[] siteBindings = new string[3];
                XmlNodeList bindingNodes = site.SelectNodes("bindings/binding[@protocol='"+url.Scheme.ToLower()+"']");
                foreach (XmlNode bindingNode in bindingNodes)
                {
                    //will be in the format IP:port:host
                    string bindings = bindingNode.Attributes["bindingInformation"].Value;
                    siteBindings = bindings.Split(':');
                    //check host
                    if (siteBindings.Length > 2)
                    {
                        if (siteBindings[2] == string.Empty || siteBindings[2].Equals(url.Host.Split('.')[0], StringComparison.CurrentCultureIgnoreCase)) hostMatch = true;
                        if (siteBindings[1] == url.Port.ToString() || (url.Port.ToString() == "" && siteBindings[1] == "80")) portMatch = true;
                        if (siteBindings[0] == "*" || siteBindings[0] == string.Empty) ipMatch = true;
                        else
                        {
                            //check host is IP
                            bool isDottedDecimal = false;
                            int networkValue = 0;
                            string[] dottedNotation = url.Host.Split('.');
                            foreach (string component in dottedNotation)
                            {
                                isDottedDecimal = int.TryParse(component, out networkValue);
                            }
                            if (isDottedDecimal)
                            {
                                if (siteBindings[0] == url.Host) ipMatch = true;
                                else ipMatch = false;
                            }
                        }
                    }
                    else
                    {
                        //fargin inconsistent IIS will put the port first.
                        if (siteBindings[0] == url.Port.ToString() || (url.Port.ToString() == "" && siteBindings[1] == "80")) portMatch = true;
                        if (siteBindings[1] == "*" || siteBindings[0] == string.Empty) ipMatch = true;
                    }
                }
                // now check the vals to see if this is the site that matches
                if (ipMatch && portMatch && hostMatch)
                {
                    siteId = Convert.ToInt32(site.Attributes["id"].Value);
                    // This still may not be the BEST match -- lets try to
                    // match the host header before breaking the loop
                    if (siteBindings[2]!=String.Empty && hostMatch==true)
                    break;
                }
            }
               return siteId;
        }
    }
}

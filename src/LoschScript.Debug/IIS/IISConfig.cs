using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace RedFlag.IIS
{
    class IISConfig
    {
        private static string GetIISAdminFolder()
        {  
            string iisAdminPath = String.Empty;
            //Get the imagepath of inetsrv
            try{
              
            RegistryKey iisAdminKey=Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\IISADMIN",false);
            iisAdminPath=iisAdminKey.GetValue("ImagePath").ToString();
            iisAdminKey.Close();
            }
            catch // IISADMIN seems to have gone walkies in Windows 7 at some point.
            {
                iisAdminPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "inetsrv\\w3wp.exe");
            }
            return System.IO.Path.GetDirectoryName(System.Environment.ExpandEnvironmentVariables(iisAdminPath));
        }
        public static string GetWorkerProcessExe()
        {
           string w3wpImagePath = System.IO.Path.Combine(GetIISAdminFolder(), "w3wp.exe");
           if (!System.IO.File.Exists(w3wpImagePath))
               throw new NotSupportedException("Cannot debug IIS version 5 and lower (w3wp.exe not found)");
           else return w3wpImagePath;
        }
        private static int Getw3wpVersion(string PathToW3wp)
        {
            System.Diagnostics.FileVersionInfo fi=System.Diagnostics.FileVersionInfo.GetVersionInfo(PathToW3wp);
            return fi.FileMajorPart;
        }
       private static string GetAppHostConfigPath()
        {
            string appHostPath = System.IO.Path.Combine(GetIISAdminFolder(), "config\\applicationHost.config");
            if (!System.IO.File.Exists(appHostPath))
                throw new NotSupportedException("applicationHost.config not found");
            else return appHostPath;
        }
       public static string GetW3wpArgs(Uri ApplicationUrl,int TcpPort)
       {
           if (ApplicationUrl.Scheme == Uri.UriSchemeHttps)
               throw new NotSupportedException("Cannot debug HTTPS, only HTTP. Use \"Attach Process\" instead.");
           //Do we need to create a host config?
           int w3wpVersion=Getw3wpVersion(GetWorkerProcessExe());

           if (w3wpVersion > 6)
           {
               string tempConfigFile = System.IO.Path.GetTempFileName();
               HostConfig.CreateAlternateApplicationHostConfig(tempConfigFile, ref ApplicationUrl, true, TcpPort);
               return String.Format(" -h \"{0}\"", tempConfigFile);
           }
           else
           {
               string newUrl = ApplicationUrl.Scheme + "://" + ApplicationUrl.Host + ":" + TcpPort + ApplicationUrl.PathAndQuery;
               return String.Format(" -debug -d {0} -ap RedFlagAppPool", newUrl);
           }
       }
    }
}

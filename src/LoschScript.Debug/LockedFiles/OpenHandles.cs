using System;
using System.Collections.Generic;
using System.Text;
/*using System.Management;*/
using System.Diagnostics;
using RedFlag.FileHandles;
using System.Runtime;
using System.Runtime.InteropServices;

namespace RedFlag
{
    class OpenHandles
    {
        static int m_nLastProcId = 0;
        static IntPtr m_ipProcessHwnd = IntPtr.Zero;
        // According to sysinternals, we cannot get the name of some objects
        // for instance named pipes, synchronizing objects. You need a kernel-level driver for this.
        const int ACCESS_MASK_PIPE_SIGNATURE = 0x0012019f;
        //const int ACCESS_MASK_SYSTEM_SECURITY = 0x01000000;

        static public string GetProcessesUsingFile(string FileName)
        {
            // first, get a list of PIDs
            List<string> processNames = new List<string>();
            string processOut = "";
            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
               // m_ipProcessHwnd = Win32API.OpenProcess(Win32API.ProcessAccessFlags.DupHandle, false, p.Id);
               // if (m_ipProcessHwnd == null) continue;
                    IntPtr srcProcess = Win32API.OpenProcess(Win32API.ProcessAccessFlags.DupHandle, false, p.Id);
                    if (srcProcess == IntPtr.Zero) continue;
                    List<Win32API.SYSTEM_HANDLE_INFORMATION> handles = CustomAPI.GetHandles(p);
                    for (int nDex = 0; nDex < handles.Count; nDex++)
                    {
                        string strTemp = "";
                       
                        strTemp = GetFileDetails(handles[nDex],srcProcess).Name;
                        if (!String.IsNullOrEmpty(strTemp))
                        {
                            if (strTemp.Equals(FileName.Trim('\"'),StringComparison.OrdinalIgnoreCase))
                            {
                                processNames.Add(p.ProcessName);
                            }
                        }
                    }
            }
            foreach  (string s in processNames)
            {
                processOut += s+",";
            }
            return processOut.TrimEnd(',');
        }
        public static FileDetails GetFileDetails(Win32API.SYSTEM_HANDLE_INFORMATION sYSTEM_HANDLE_INFORMATION, IntPtr srcProcessHandle)
        {
            
            FileDetails fd = new FileDetails();
            fd.Name = String.Empty;
            fd.ObjectTypeName = String.Empty;
            IntPtr ipHandle = IntPtr.Zero;
            Win32API.OBJECT_BASIC_INFORMATION objBasic = new Win32API.OBJECT_BASIC_INFORMATION();
            IntPtr ipBasic = IntPtr.Zero;
            Win32API.OBJECT_TYPE_INFORMATION objObjectType = new Win32API.OBJECT_TYPE_INFORMATION();
            IntPtr ipObjectType = IntPtr.Zero;
            //Win32API.OBJECT_NAME_INFORMATION objObjectName = new Win32API.OBJECT_NAME_INFORMATION();
            IntPtr ipObjectName = IntPtr.Zero;
            string strObjectTypeName = String.Empty;
            string strObjectName = String.Empty;
            int nLength = 0;
            int nReturn = 0;
            IntPtr ipTemp = IntPtr.Zero;
            if (sYSTEM_HANDLE_INFORMATION.Handle == 0) return fd;
            if (!Win32API.DuplicateHandle(srcProcessHandle, sYSTEM_HANDLE_INFORMATION.Handle, Win32API.GetCurrentProcess(), out ipHandle, 0, false, Win32API.DUPLICATE_SAME_ACCESS))
            {
                int errorcode = Marshal.GetLastWin32Error(); // Code 50 is not supported
               if (errorcode==50 || errorcode==6) return fd; // Code 6 is invalid handle
            }
            ipBasic = Marshal.AllocHGlobal(Marshal.SizeOf(objBasic));
            if (Win32API.NtQueryObject(ipHandle, (int)Win32API.ObjectInformationClass.ObjectBasicInformation, ipBasic, Marshal.SizeOf(objBasic), ref nLength) != 0)
            {
                int errorcode = Marshal.GetLastWin32Error();
                if (errorcode==50 || errorcode == 6) return fd;
            }
            objBasic = (Win32API.OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(ipBasic, objBasic.GetType());
            Marshal.FreeHGlobal(ipBasic);

            ipObjectType = Marshal.AllocHGlobal(objBasic.TypeInformationLength);
            nLength = objBasic.TypeInformationLength;
            while ((uint)(nReturn = Win32API.NtQueryObject(ipHandle, (int)Win32API.ObjectInformationClass.ObjectTypeInformation, ipObjectType, nLength, ref nLength)) == Win32API.STATUS_INFO_LENGTH_MISMATCH)
            {
                Marshal.FreeHGlobal(ipObjectType);
                ipObjectType = Marshal.AllocHGlobal(nLength);
            }
            if (nReturn == 0)
            {
                objObjectType = (Win32API.OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(ipObjectType, objObjectType.GetType());
                if (Is64Bits())
                {
                    ipTemp = new IntPtr(Convert.ToInt64(objObjectType.Name.Buffer.ToString(), 10) >> 32);
                }
                else
                {
                    ipTemp = objObjectType.Name.Buffer;
                }

                if (ipTemp != IntPtr.Zero) strObjectTypeName = Marshal.PtrToStringUni(ipTemp, objObjectType.Name.Length >> 1);
                Marshal.FreeHGlobal(ipObjectType);
            }
            fd.ObjectTypeName = strObjectTypeName;
            nLength = objBasic.NameInformationLength;
            // Now we should have an object type - get the object name            
            GetObjectNameInfoCaller caller = new GetObjectNameInfoCaller(GetObjectNameInfo);
            IAsyncResult result = caller.BeginInvoke(ipHandle, null, null);
            // Do this asynchronously because some objects will cause a hang
            if (result.AsyncWaitHandle.WaitOne(1000, true))
            {
                //either we have a name or it's hung...
                strObjectName = caller.EndInvoke(result);
                if (strObjectName != null && strObjectTypeName == "File") fd.Name = GetRegularFileNameFromDevice(strObjectName);
                else fd.Name = strObjectName;
            }
            else fd.Name = String.Empty;
            
            Win32API.CloseHandle(ipHandle);
            return fd;
        }
        public delegate string GetObjectNameInfoCaller(IntPtr handle);
        private static string GetObjectNameInfo(IntPtr ipHandle)
        {
            string strObjectName = String.Empty;
            int nReturn = 0;
            int nLength = 0;
            IntPtr ipObjectName = IntPtr.Zero;
            IntPtr ipTemp = IntPtr.Zero;
            Win32API.OBJECT_NAME_INFORMATION objObjectName = new Win32API.OBJECT_NAME_INFORMATION();
            nReturn = Win32API.NtQueryObject(ipHandle, (int)Win32API.ObjectInformationClass.ObjectNameInformation, IntPtr.Zero, nLength, ref nLength);
            if (nLength > 0)
            {
                ipObjectName = Marshal.AllocHGlobal(nLength);
                nReturn = Win32API.NtQueryObject(ipHandle, (int)Win32API.ObjectInformationClass.ObjectNameInformation, ipObjectName, nLength, ref nLength);
                if (nReturn == 0)
                {
                    objObjectName = (Win32API.OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(ipObjectName, objObjectName.GetType());
                    if (Is64Bits())
                    {
                        ipTemp = new IntPtr(Convert.ToInt64(objObjectName.Name.Buffer.ToString(), 10) >> 32);
                    }
                    else
                    {
                        ipTemp = objObjectName.Name.Buffer;
                    }

                    if (Is64Bits())
                    {
                        strObjectName = Marshal.PtrToStringUni(new IntPtr(ipTemp.ToInt64()));
                    }
                    else
                    {
                        strObjectName = Marshal.PtrToStringUni(new IntPtr(ipTemp.ToInt32()));
                    }

                }
                else
                {
                    strObjectName = "Error! " + nReturn.ToString("X") + " see NTSTATUS.h";
                }
            }
            Marshal.FreeHGlobal(ipObjectName);
            return strObjectName;
        }
        /// <summary>
        /// Translate a device name to a path on disk
        /// </summary>
        /// <param name="strRawName"></param>
        /// <returns></returns>
        public static string GetRegularFileNameFromDevice(string strRawName)
        {
            string strFileName = strRawName;
            foreach (string strDrivePath in Environment.GetLogicalDrives())
            {
                StringBuilder sbTargetPath = new StringBuilder(Win32API.MAX_PATH);
                if (Win32API.QueryDosDevice(strDrivePath.Substring(0, 2), sbTargetPath, Win32API.MAX_PATH) == 0)
                {
                    return strRawName;
                }
                string strTargetPath = sbTargetPath.ToString();
                if (strFileName.StartsWith(strTargetPath))
                {
                    strFileName = strFileName.Replace(strTargetPath, strDrivePath.Substring(0, 2));
                    break;
                }
            }
            return strFileName;
        }

        static bool Is64Bits()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8 ? true : false;
        }
        private static void OpenProcessForHandle(int p)
        {
            if (p != m_nLastProcId)
            {
                Win32API.CloseHandle(m_ipProcessHwnd);
                m_ipProcessHwnd = Win32API.OpenProcess(Win32API.ProcessAccessFlags.DupHandle, false, p);
                m_nLastProcId = p;
            }
        }

    }
}

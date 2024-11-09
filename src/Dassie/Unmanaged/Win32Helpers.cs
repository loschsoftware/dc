using System;
using System.Runtime.InteropServices;

namespace Dassie.Unmanaged;

internal static class Win32Helpers
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr BeginUpdateResource(string pFileName, [MarshalAs(UnmanagedType.Bool)] bool bDeleteExistingResources);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, IntPtr lpData, uint cbData);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

    public static unsafe bool SetIcon(string assemblyPath, string iconPath)
    {
        byte[] data = File.ReadAllBytes(iconPath);

        IntPtr hUpdate = BeginUpdateResource(assemblyPath, false);

        bool b = false;

        fixed (byte* pData = data)
        {
            b = UpdateResource(hUpdate, (IntPtr)14, (IntPtr)1, 0, (IntPtr)pData, (uint)data.Length);
        }

        EndUpdateResource(hUpdate, false);

        return b;
    }
}
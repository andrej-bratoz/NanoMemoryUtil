using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WarInTheNorthTrainer.Code;

namespace WarInTheNorthTrainer
{
    public class WinAPI
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
       public  static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            Int32 nSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesWritten);


        [DllImport("kernel32.dll")]
        public static extern int VirtualQueryEx(IntPtr hProcess, uint lpAddress, 
            out MEMORY_BASIC_INFORMATION lpBuffer, 
            uint dwLength);


        [DllImport("ntdll.dll")]
        public static extern int NtWriteVirtualMemory(int handle,int address, byte[] buffer, int bufferLen, out int written);

        [DllImport("ntdll.dll")]
        public static extern int NtReadVirtualMemory(int handle, int address, byte[] buffer, int bufferLen, out int written);


        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        [DllImport("kernel32", SetLastError = true)]
        public static extern int ReadProcessMemory(
            IntPtr hProcess,
            int lpBase,
            ref int lpBuffer,
            int nSize,
            int lpNumberOfBytesRead
        );

        public static void WriteMem(Process p, int address, long v)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)p.Id);
            var val = new byte[] { (byte)v };
            int wtf = 0;
            WriteProcessMemory(hProc, new IntPtr(address), val, (UInt32)val.LongLength, out wtf);
            CloseHandle(hProc);
        }

        public static byte[] ReadMemory(IntPtr handle, int address, int size)
        {
            //Prepare buffer and pointer
            byte[] dataBuffer = new byte[size];
            IntPtr bytesRead = IntPtr.Zero;
            //Read
            ReadProcessMemory(handle, (IntPtr)address, dataBuffer, dataBuffer.Length, out bytesRead);
            //Error handling - Small'n'dirty: Return null if smth went wrong
            if (bytesRead.ToInt32() != size)
                return null;
            return dataBuffer;
        }

        public static int ReadInt(IntPtr handle, int address, int defaultValue)
        {
            //Get our data
            byte[] data = ReadMemory(handle, address,4);
            if (data == null)
                return defaultValue; //Alternatively, you can throw an exception if you like
            return BitConverter.ToInt32(data,0);
        }

    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VMOperation = 0x00000008,
        VMRead = 0x00000010,
        VMWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        Synchronize = 0x00100000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public UIntPtr BaseAddress;
        public UIntPtr AllocationBase;
        public uint AllocationProtect;
        public UIntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION64
    {
        public ulong BaseAddress;
        public ulong AllocationBase;
        public int AllocationProtect;
        public int __alignment1;
        public ulong RegionSize;
        public int State;
        public int Protect;
        public int Type;
        public int __alignment2;
    }

    public enum AllocationProtect : uint
    {
        PAGE_EXECUTE = 0x00000010,
        PAGE_EXECUTE_READ = 0x00000020,
        PAGE_EXECUTE_READWRITE = 0x00000040,
        PAGE_EXECUTE_WRITECOPY = 0x00000080,
        PAGE_NOACCESS = 0x00000001,
        PAGE_READONLY = 0x00000002,
        PAGE_READWRITE = 0x00000004,
        PAGE_WRITECOPY = 0x00000008,
        PAGE_GUARD = 0x00000100,
        PAGE_NOCACHE = 0x00000200,
        PAGE_WRITECOMBINE = 0x00000400
    }

    public struct SYSTEM_INFO
    {
        public ushort processorArchitecture;
        ushort reserved;
        public uint pageSize;
        public IntPtr minimumApplicationAddress;  // minimum address
        public IntPtr maximumApplicationAddress;  // maximum address
        public IntPtr activeProcessorMask;
        public uint numberOfProcessors;
        public uint processorType;
        public uint allocationGranularity;
        public ushort processorLevel;
        public ushort processorRevision;
    }
}

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace UtilityLib
{
    // http://msdn.microsoft.com/en-us/library/windows/desktop/ms680519(v=vs.85).aspx
    [Flags]
    public enum MINIDUMP_TYPE
    {
        MiniDumpNormal = 0x00000000,
        MiniDumpWithDataSegs = 0x00000001,
        MiniDumpWithFullMemory = 0x00000002,
        MiniDumpWithHandleData = 0x00000004,
        MiniDumpFilterMemory = 0x00000008,
        MiniDumpScanMemory = 0x00000010,
        MiniDumpWithUnloadedModules = 0x00000020,
        MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
        MiniDumpFilterModulePaths = 0x00000080,
        MiniDumpWithProcessThreadData = 0x00000100,
        MiniDumpWithPrivateReadWriteMemory = 0x00000200,
        MiniDumpWithoutOptionalData = 0x00000400,
        MiniDumpWithFullMemoryInfo = 0x00000800,
        MiniDumpWithThreadInfo = 0x00001000,
        MiniDumpWithCodeSegs = 0x00002000,
        MiniDumpWithoutAuxiliaryState = 0x00004000,
        MiniDumpWithFullAuxiliaryState = 0x00008000,
        MiniDumpWithPrivateWriteCopyMemory = 0x00010000,
        MiniDumpIgnoreInaccessibleMemory = 0x00020000,
        MiniDumpWithTokenInformation = 0x00040000,
        MiniDumpWithModuleHeaders = 0x00080000,
        MiniDumpFilterTriage = 0x00100000,
        MiniDumpValidTypeFlags = 0x001fffff
    }

    // http://msdn.microsoft.com/en-us/library/windows/desktop/ms680366(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MINIDUMP_EXCEPTION_INFORMATION
    {
        public uint ThreadId;
        public IntPtr ExceptionPointers;
        public int ClientPointers;
    }

    public class MiniDump
    {
        private static MethodBase? _last = null;

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("DbgHelp.dll")]
        private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint ProcessId,
            IntPtr hFile, int DumpType, ref MINIDUMP_EXCEPTION_INFORMATION ExceptionParam,
            IntPtr UserStreamParam, IntPtr CallbackParam);

        public static void CreateMiniDump()
        {
            // 동일한 곳에서 연속으로 찍지 말기
            var _stack = new StackFrame(1, true).GetMethod();
            if (_last == _stack)
            {
                return;
            }

            _last = _stack;

            var proc = Process.GetCurrentProcess();
            var fileName = String.Format("minidump_{0}_{1}.dmp", proc.ProcessName, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));

            var info = new MINIDUMP_EXCEPTION_INFORMATION()
            {
                ThreadId = GetCurrentThreadId(),
                ExceptionPointers = Marshal.GetExceptionPointers(),
                ClientPointers = 1
            };

            //#if DEBUG
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                // MINIDUMP_TYPE.MiniDumpWithFullMemory gives more info, but dump file size will be very large
                MiniDumpWriteDump(proc.Handle, (uint)proc.Id, fs.SafeFileHandle.DangerousGetHandle(),
                    (int)MINIDUMP_TYPE.MiniDumpWithFullMemory, ref info, IntPtr.Zero, IntPtr.Zero);
            }
            /*#else
                        using (var fs = new FileStream(fileName, FileMode.Create)) {
                            // MINIDUMP_TYPE.MiniDumpWithFullMemory gives more info, but dump file size will be very large
                            MiniDumpWriteDump(proc.Handle, (uint)proc.Id, fs.SafeFileHandle.DangerousGetHandle(),
                                (int)MINIDUMP_TYPE.MiniDumpWithDataSegs, ref info, IntPtr.Zero, IntPtr.Zero);
                        }
            #endif*/
        }
    }
}

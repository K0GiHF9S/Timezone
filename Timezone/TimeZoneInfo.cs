using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Timezone
{
    [StructLayout(LayoutKind.Sequential)]
    struct SYSTEMTIME
    {
        [MarshalAs(UnmanagedType.U2)]
        public Int16 wYear;
        [MarshalAs(UnmanagedType.U2)]
        public Int16 wMonth;
        [MarshalAs(UnmanagedType.U2)]
        public Int16 wDayOfWeek;
        [MarshalAs(UnmanagedType.U2)]
        public Int16 wDay;
        [MarshalAs(UnmanagedType.U2)]
        public Int16 wHour;
        [MarshalAs(UnmanagedType.U2)]
        public Int16 wMinute;
        [MarshalAs(UnmanagedType.U2)]
        public Int16 wSecond;
        [MarshalAs(UnmanagedType.U2)]
        public Int16 wMilliseconds;
        public SYSTEMTIME(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length != 0x10)
            {
                throw new ArgumentException();
            }
            int offset = 0;
            this.wYear = BitConverter.ToInt16(byteArray, offset);
            offset += 0x02;
            this.wMonth = BitConverter.ToInt16(byteArray, offset);
            offset += 0x02;
            this.wDayOfWeek = BitConverter.ToInt16(byteArray, offset);
            offset += 0x02;
            this.wDay = BitConverter.ToInt16(byteArray, offset);
            offset += 0x02;
            this.wHour = BitConverter.ToInt16(byteArray, offset);
            offset += 0x02;
            this.wMinute = BitConverter.ToInt16(byteArray, offset);
            offset += 0x02;
            this.wSecond = BitConverter.ToInt16(byteArray, offset);
            offset += 0x02;
            this.wMilliseconds = BitConverter.ToInt16(byteArray, offset);
            offset += 0x02;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct TIME_ZONE_INFORMATION
    {
        [MarshalAs(UnmanagedType.I4)]
        public Int32 Bias;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string StandardName;
        public SYSTEMTIME StandardDate;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 StandardBias;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DaylightName;
        public SYSTEMTIME DaylightDate;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 DaylightBias;

        public TIME_ZONE_INFORMATION(byte[] byteArray, string StandardName, string DaylightName)
        {
            if (byteArray == null || byteArray.Length != 0x2c)
            {
                throw new ArgumentException();
            }
            int offset = 0;
            this.Bias = BitConverter.ToInt32(byteArray, offset);
            offset += 0x04;
            this.StandardBias = BitConverter.ToInt32(byteArray, offset);
            offset += 0x04;
            this.DaylightBias = BitConverter.ToInt32(byteArray, offset);
            offset += 0x04;
            this.StandardDate = new SYSTEMTIME((new List<byte>(byteArray).GetRange(offset, 0x10)).ToArray());
            offset += 0x10;
            this.DaylightDate = new SYSTEMTIME((new List<byte>(byteArray).GetRange(offset, 0x10)).ToArray());

            this.StandardName = StandardName;
            this.DaylightName = DaylightName;
        }
    }

    class TimeZoneInfo
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 PrivilegeCount;
            [MarshalAs(UnmanagedType.I8)]
            public Int64 Luid;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 Attributes;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetTimeZoneInformation([In] ref TIME_ZONE_INFORMATION lpTimeZoneInformation);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll")]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll")]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
            [In] ref TOKEN_PRIVILEGES NewState, Int32 BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out Int64 lpLuid);

        public const int TOKEN_QUERY = 0x08;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        public const int SE_PRIVILEGE_ENABLED = 0x02;
        public const string SE_TIME_ZONE_PRIVILEGE = "SeTimeZonePrivilege";

        public static int SetTimeZoneInformation(TIME_ZONE_INFORMATION tzi)
        {
            EnablePrivileges();
            var result = SetTimeZoneInformation(ref tzi);

            var lastError = Marshal.GetLastWin32Error();

            DisablePrivileges();

            return lastError;
        }

        private static void EnablePrivileges()
        {
            var result = OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr tokenHandle);
            var tp = new TOKEN_PRIVILEGES
            {
                Attributes = SE_PRIVILEGE_ENABLED,
                PrivilegeCount = 1
            };
            result = LookupPrivilegeValue(null, SE_TIME_ZONE_PRIVILEGE, out tp.Luid);
            result = AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            result = CloseHandle(tokenHandle);
        }

        private static void DisablePrivileges()
        {
            OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr tokenHandle);
            var tp = new TOKEN_PRIVILEGES
            {
                // Attributes = NONE;
                PrivilegeCount = 1
            };
            LookupPrivilegeValue(null, SE_TIME_ZONE_PRIVILEGE, out tp.Luid);
            AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            CloseHandle(tokenHandle);
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Timezone
{
    static class TimeZoneInfo
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
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
        private struct TIME_ZONE_INFORMATION
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

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 PrivilegeCount;

            public LUID Luid;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 Attributes;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint LowPart;
            [MarshalAs(UnmanagedType.U4)]
            public uint HighPart;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetTimeZoneInformation([In] ref TIME_ZONE_INFORMATION lpTimeZoneInformation);

        [DllImport("kernel32.dll")]
        private static extern Int32 GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(Int32 hObject);

        [DllImport("advapi32.dll")]
        private static extern bool OpenProcessToken(Int32 ProcessHandle, UInt32 DesiredAccess, ref Int32 TokenHandle);

        [DllImport("advapi32.dll")]
        private static extern bool AdjustTokenPrivileges(Int32 TokenHandle, bool DisableAllPrivileges,
            [In] ref TOKEN_PRIVILEGES NewState, Int32 BufferLength, Int32 PreviousState, Int32 ReturnLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        private const UInt32 TOKEN_QUERY = 0x08;
        private const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x20;
        private const UInt32 SE_PRIVILEGE_ENABLED = 0x02;
        private const string SE_TIME_ZONE_PRIVILEGE = "SeTimeZonePrivilege";
        
        private static readonly Dictionary<string, TIME_ZONE_INFORMATION> timeZoneDictionary;
        private static readonly Dictionary<string, string> standardNameTimeZoneIdDictionary;

        static TimeZoneInfo()
        {
            var keyName = @"Software\Microsoft\Windows NT\CurrentVersion\Time Zones";
            using (var registryKey = Registry.LocalMachine.OpenSubKey(keyName))
            {
                var subKeyNameList = new List<string>(registryKey.GetSubKeyNames());
                subKeyNameList.Sort();
                
                timeZoneDictionary = new Dictionary<string, TIME_ZONE_INFORMATION>();
                standardNameTimeZoneIdDictionary = new Dictionary<string, string>();
                foreach (var subKeyName in subKeyNameList)
                {
                    using (var subKey = registryKey.OpenSubKey(subKeyName))
                    {
                        var standardName = subKey.GetValue("Std").ToString();
                        var daylightName = subKey.GetValue("Dlt").ToString();
                        var tzi = (byte[])subKey.GetValue("TZI");
                        var timeZoneInformation = new TIME_ZONE_INFORMATION(tzi, standardName, daylightName);
                        timeZoneDictionary.Add(subKeyName, timeZoneInformation);
                        standardNameTimeZoneIdDictionary.Add(standardName, subKeyName);
                    }
                }
            }
        }

        public static List<string> GetTimeZoneIdList()
        {
            return new List<string>(timeZoneDictionary.Keys);
        }

        public static string GetCurrentTimeZoneId()
        {
            return standardNameTimeZoneIdDictionary[TimeZone.CurrentTimeZone.StandardName];
        }

        public static int SetTimeZoneInformation(string timeZoneId)
        {
            var result =
                timeZoneDictionary.TryGetValue(timeZoneId, out TIME_ZONE_INFORMATION selectedTimeZone);
            if (!result)
            {
                return -1;
            }

            EnablePrivileges();
            result = SetTimeZoneInformation(ref selectedTimeZone);

            var lastError = Marshal.GetLastWin32Error();

            DisablePrivileges();

            System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();

            return lastError;
        }

        private static void EnablePrivileges()
        {
            Int32 tokenHandle = 0;
            OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref tokenHandle);
            var tp = new TOKEN_PRIVILEGES
            {
                Attributes = SE_PRIVILEGE_ENABLED,
                PrivilegeCount = 1
            };
            LookupPrivilegeValue(null, SE_TIME_ZONE_PRIVILEGE, out tp.Luid);
            AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, 0, 0);
            CloseHandle(tokenHandle);
        }

        private static void DisablePrivileges()
        {
            Int32 tokenHandle = 0;
            OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref tokenHandle);
            var tp = new TOKEN_PRIVILEGES
            {
                // Attributes = NONE;
                PrivilegeCount = 1
            };
            LookupPrivilegeValue(null, SE_TIME_ZONE_PRIVILEGE, out tp.Luid);
            AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, 0, 0);
            CloseHandle(tokenHandle);
        }
    }
}

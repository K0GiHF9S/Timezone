using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Timezone
{
    public partial class TimeZoneSetting : Form
    {
        private Dictionary<string, TIME_ZONE_INFORMATION> timeZoneList;
        private TimeZone currentTimeZone = TimeZone.CurrentTimeZone;

        public TimeZoneSetting()
        {
            InitializeComponent();
        }

        private void TimeZoneSetting_Load(object sender, EventArgs e)
        {
            var keyName = @"Software\Microsoft\Windows NT\CurrentVersion\Time Zones";
            using (var registryKey = Registry.LocalMachine.OpenSubKey(keyName))
            {
                var subKeyNameList = new List<string>(registryKey.GetSubKeyNames());
                subKeyNameList.Sort();

                this.timeZoneComboBox.Items.Clear();
                timeZoneList = new Dictionary<string, TIME_ZONE_INFORMATION>();
                var currentStandardName = currentTimeZone.StandardName;
                foreach (var subKeyName in subKeyNameList)
                {
                    this.timeZoneComboBox.Items.Add(subKeyName);

                    using (var subKey = registryKey.OpenSubKey(subKeyName))
                    {
                        var standardName = subKey.GetValue("Std").ToString();
                        var daylightName = subKey.GetValue("Dlt").ToString();
                        var tzi = (byte[])subKey.GetValue("TZI");
                        var timeZoneInformation = new TIME_ZONE_INFORMATION(tzi, standardName, daylightName);
                        timeZoneList.Add(subKeyName, timeZoneInformation);
                        if (currentStandardName == standardName)
                        {
                            this.timeZoneComboBox.SelectedIndex = timeZoneList.Count - 1;
                        }
                    }
                }
            }
            
            var dateTimeOffset = DateTimeOffset.Now;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            TIME_ZONE_INFORMATION selectedTimeZone;
            var selectedString = this.timeZoneComboBox.SelectedItem.ToString();
            var result =
                timeZoneList.TryGetValue(selectedString, out selectedTimeZone);
            if (result && currentTimeZone.StandardName != selectedTimeZone.StandardName)
            {
                TimeZoneInfo.SetTimeZoneInformation(timeZoneList[selectedString]);
                currentTimeZone = TimeZone.CurrentTimeZone;
            }
        }
    }
}

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
        private string currentTimeZoneId;

        public TimeZoneSetting()
        {
            InitializeComponent();
        }

        private void TimeZoneSetting_Load(object sender, EventArgs e)
        {
            var timeZoneIdList = TimeZoneInfo.GetTimeZoneIdList();
            this.timeZoneComboBox.Items.Clear();
            foreach (var timeZoneId in timeZoneIdList)
            {
                this.timeZoneComboBox.Items.Add(timeZoneId);
            }
            
            currentTimeZoneId = TimeZoneInfo.GetCurrentTimeZoneId();
            this.timeZoneComboBox.SelectedIndex = timeZoneIdList.IndexOf(currentTimeZoneId);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            var selectedString = this.timeZoneComboBox.SelectedItem.ToString();
            if (selectedString != null && selectedString != currentTimeZoneId)
            {
                if (TimeZoneInfo.SetTimeZoneInformation(selectedString) == 0)
                {
                    currentTimeZoneId = selectedString;
                }
            }
        }
    }
}

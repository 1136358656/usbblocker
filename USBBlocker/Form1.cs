using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Threading;
using System.Security.Permissions;

namespace USBBlocker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public enum DeviceEvent : int
        {
            DBT_CONFIGCHANGECANCELED = 0x0019,
            DBT_CONFIGCHANGED = 0x0018,
            DBT_CUSTOMEVENT = 0x8006,
            DBT_DEVICEARRIVAL = 0x8000,//USB Insert DEvice Statu
            DBT_DEVICEQUERYREMOVE = 0x8001,
            DBT_DEVICEQUERYREMOVEFAILED = 0x8002,
            DBT_DEVICEREMOVEPENDING = 0x8003,//USB Revoing.
            DBT_DEVICEREMOVECOMPLETE = 0x8004,//USB Remove Completed
            DBT_DEVICETYPESPECIFIC = 0x8005,
            DBT_DEVNODES_CHANGED = 0x0007,//Device List _Changed
            DBT_QUERYCHANGECONFIG = 0x0017,
            DBT_USERDEFINED = 0xFFFF
        }
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            DeviceEvent lEvent;

            lEvent = (DeviceEvent)m.WParam.ToInt32();
            switch (lEvent)
            {
                case DeviceEvent.DBT_DEVICEARRIVAL://[Insert]
                    this.CheckDeviceStatus_Lable.BackColor = Color.Green;
                    this.CheckDeviceStatus_Lable.Text = "----Connection Device!----";
                    MessageBox.Show("Just Insert At Moment !", "Insert");
                    break;
                case DeviceEvent.DBT_DEVICEREMOVECOMPLETE://[REmove]
                    this.CheckDeviceStatus_Lable.BackColor = Color.Red;
                    this.CheckDeviceStatus_Lable.Text = "------No Connection------";
                    MessageBox.Show("Remove Complete At Moment!", "Remove");
                    break;
                case DeviceEvent.DBT_DEVNODES_CHANGED://[Device List Have Changed]
                    MessageBox.Show("Device List have been Changed!");
                    break;
                default:
                    break;
            }
        }
    }
}

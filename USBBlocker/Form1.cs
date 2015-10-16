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
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using UsbEject;
using System.Text.RegularExpressions;



namespace USBBlocker
{
    
    public partial class Form1 : Form
    {
        

        public Form1()
        {
            InitializeComponent();
        }
        public Hashtable DiskFile = new Hashtable();
        public  ArrayList UsbDisk = new ArrayList();
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private delegate void SetTextCallback(string text);
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam); 
        public ArrayList GetRemovableDisk()
        {
            ArrayList result = new ArrayList();
            DriveInfo[] Drives = DriveInfo.GetDrives();
            foreach (DriveInfo Drive in Drives)
            {
                if (Drive.DriveType.ToString() == "Removable")
                {
                    result.Add(Drive.Name.ToString());
                }
               
            }
            return result;
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) ;        
            {
                Capture = false;            
                SendMessage(Handle, 0x00A1, 2, 0);         
            }
        }
        public string winDir = System.Environment.GetEnvironmentVariable("windir");
        private void Form1_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(listBox1, "双击列表中的盘符可以弹出对应U盘");
            this.Visible = false;
            this.Opacity = 0.7;
            Win32.AnimateWindow(this.Handle, 400, Win32.AW_VER_POSITIVE);
            ArrayList Removable1 = this.GetRemovableDisk();
            foreach (string x in Removable1)
            {
                this.dir = x;
                this.HandleUsbDevice(x, 0);
                Thread t1 = new Thread(new ThreadStart(GetFile));
                t1.Start();

            }
            foreach (string n in this.UsbDisk)
            {
                listBox1.Items.Clear();
                listBox1.Items.Add(n);
            }
            
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
                    //MessageBox.Show("Just Insert At Moment !", "Insert");
                    ArrayList Removable1 = this.GetRemovableDisk();
                    this.DiskFile.Clear();
                    foreach (string x in Removable1)
                    {
                        this.dir = x;
                        this.HandleUsbDevice(x,0);
                        //MessageBox.Show(x);
                        Thread t1 = new Thread(new ThreadStart(GetFile));
                        t1.Start();

                    }
                    foreach (string n in this.UsbDisk)
                    {
                        listBox1.Items.Clear();
                        listBox1.Items.Add(n);
                    }
                    break;
                case DeviceEvent.DBT_DEVICEREMOVECOMPLETE://[REmove]
                    this.CheckDeviceStatus_Lable.BackColor = Color.Red;
                    this.CheckDeviceStatus_Lable.Text = "------No Connection------";
                    ArrayList Removable2 = this.GetRemovableDisk();
                    this.UsbDisk.Clear();
                    foreach (string x in Removable2)
                    {
                        this.dir = x;
                        this.HandleUsbDevice(x,1);
                        //MessageBox.Show(x);
                    }
                    ArrayList RemovedDisk = new ArrayList();
                    foreach (DictionaryEntry i in this.DiskFile)
                    {
                        if (this.UsbDisk.Contains(i.Key)==false)
                        {
                            RemovedDisk.Add(i.Key);
                        }
                    }
                    foreach (string i in RemovedDisk)
                    {
                        this.DiskFile.Remove(i);
                    }

                    listBox1.Items.Clear();
                    foreach (string n in this.UsbDisk)
                    {
                        
                        listBox1.Items.Add(n);
                    }
                    //MessageBox.Show("Remove Complete At Moment!", "Remove");
                    break;
                case DeviceEvent.DBT_DEVNODES_CHANGED://[Device List Have Changed]
                    //MessageBox.Show("Device List have been Changed!");
                    break;
                default:
                    break;
            }
        }

        public   static   ArrayList   ListFiles(FileSystemInfo   info) 
　　{ 
            ArrayList File = new ArrayList();
            ArrayList temp = null;
　　　　if(!info.Exists)   return null;

　　　　DirectoryInfo   dir   =   info   as   DirectoryInfo; 
　　　　//不是目录 
　　　　if(dir   ==   null)   return null;

　　　　FileSystemInfo   []   files   =   dir.GetFileSystemInfos(); 
　　　　for(int   i   =   0;   i   <   files.Length;   i++) 
　　　　{ 
　　　　　　FileInfo   file   =   files[i]   as   FileInfo; 
　　　　　　//是文件 
　　　　　　if(file   !=   null){
          File.Add(file.FullName);
          //对于子目录，进行递归调用 
      }else
      {
          temp = ListFiles(files[i]);
          foreach (string x in temp)
          {
              File.Add(x);
          }
      }
          
                 
      
　　　　　　　　

　　　　}
            return File;
 
　　}
        public string dir="";
        public  void GetFile()
        {
            ArrayList y = ListFiles(new DirectoryInfo(this.dir));
            if (this.DiskFile.ContainsKey(this.dir) == true) { this.DiskFile.Remove(this.dir); }
            this.DiskFile.Add(this.dir,y);
        }
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text = text + "\r\n";
            }
        }
        public int Eject(string LogicalDisk)
        {
            VolumeDeviceClass volumeDeviceClass = new VolumeDeviceClass();
            foreach (Volume device in volumeDeviceClass.Devices)
            {
                // is this volume on USB disks?
                if (!device.IsUsb)
                    continue;

                // is this volume a logical disk?
                if ((device.LogicalDrive == null) || (device.LogicalDrive.Length == 0))
                    continue;
                if (device.LogicalDrive.ToString() + "\\" == LogicalDisk)
                {
                    device.Eject(true); // allow Windows to display any relevant UI
                    MessageBox.Show(device.LogicalDrive.ToString());
                }
                else
                {
                    continue;
                }
                
            }

            return 0;

        }
        public ArrayList HandleUsbDevice(string drive,int method=0)
        {
            if (method == 0)
            {
                //添加
                if (this.UsbDisk.Contains(drive) == true)
                {
                    return this.UsbDisk;
                }
                this.UsbDisk.Add(drive);
                return this.UsbDisk;
            }
            else
            {
                if (method == 1)
                {
                    if (this.UsbDisk.Contains(drive) == false)
                    {
                        return this.UsbDisk;
                    }
                    this.UsbDisk.Remove(drive);
                    return this.UsbDisk;
                }
            }

            return this.UsbDisk;
        }
        private void listBox1_DrawItem(object sender,
    System.Windows.Forms.DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            // Define the default color of the brush as black.
            Brush myBrush = Brushes.Black;

            // Determine the color of the brush to draw each item based 
            // on the index of the item to draw.
           

            // Draw the current item text based on the current Font 
            // and the custom brush settings.
            if (e.Index < 0)
            {
                return;
            }
            e.Graphics.DrawString(listBox1.Items[e.Index].ToString(),
                e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            //MessageBox.Show(listBox1.SelectedItem.ToString());
            if (listBox1.SelectedItem==null) { return; }
            Eject(listBox1.SelectedItem.ToString());
        }

        private void label1_Click(object sender, EventArgs e)
        {
            //Win32.AnimateWindow(this.Handle, 400, Win32.AW_BLEND);
            Environment.Exit(0);
        }
        public ArrayList FileFilter(ArrayList Filelist)
        {
            string pat = @"*.exe$";
            ArrayList result = new ArrayList();
            Regex mc = new Regex(pat, RegexOptions.IgnoreCase);
            foreach (string file in Filelist)
            {
                if (mc.IsMatch(file))
                {
                    result.Add(file);
                }
            }
            return result;
        }
}
       

        
    }
    public class Win32
    {
        public const Int32 AW_HOR_POSITIVE = 0x00000001; // 从左到右打开窗口
        public const Int32 AW_HOR_NEGATIVE = 0x00000002; // 从右到左打开窗口
        public const Int32 AW_VER_POSITIVE = 0x00000004; // 从上到下打开窗口
        public const Int32 AW_VER_NEGATIVE = 0x00000008; // 从下到上打开窗口
        public const Int32 AW_CENTER = 0x00000010; //若使用了AW_HIDE标志，则使窗口向内重叠；若未使用AW_HIDE标志，则使窗口向外扩展。
        public const Int32 AW_HIDE = 0x00010000; //隐藏窗口，缺省则显示窗口。
        public const Int32 AW_ACTIVATE = 0x00020000; //激活窗口。在使用了AW_HIDE标志后不要使用这个标志。
        public const Int32 AW_SLIDE = 0x00040000; //使用滑动类型。缺省则为滚动动画类型。当使用AW_CENTER标志时，这个标志就被忽略。
        public const Int32 AW_BLEND = 0x00080000; //使用淡出效果。只有当hWnd为顶层窗口的时候才可以使用此标志。
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool AnimateWindow(
          IntPtr hwnd, // handle to window 
          int dwTime, // duration of animation 
          int dwFlags // animation type 
          );
    }


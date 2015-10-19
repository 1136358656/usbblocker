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
        public Hashtable RiskFile = new Hashtable();
        public Hashtable BlockedFile = new Hashtable();
        public  ArrayList UsbDisk = new ArrayList();
        public DiskStatus[] DiskStatus= new DiskStatus[10];
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
            this.DiskFile.Clear();
            foreach (string x in Removable1)
            {
                //System.Console.WriteLine(x);
                this.dir = x;
                this.AddDiskStatus(x, false);
                System.Console.WriteLine(this.DiskStatus[0]);
                this.HandleUsbDevice(x, 0);
                //MessageBox.Show(x);
                Thread t1 = new Thread(new ThreadStart(GetFile));
                t1.Start();

            }
            foreach (string n in this.UsbDisk)
            {
                listBox1.Items.Clear();
                listBox1.Items.Add(n);
            }
            
            foreach (DiskStatus x in this.DiskStatus)
            {
                if (x != null)
                {
                    //MessageBox.Show(x.Disk + "\r\n" + x.Completed.ToString());
                    
                }

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
                        //System.Console.WriteLine(x);
                        this.dir = x;
                        this.AddDiskStatus(x, false);
                        System.Console.WriteLine(this.DiskStatus[0]);
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
                    foreach(DiskStatus x in this.DiskStatus)
                    {
                        if (x != null)
                        {
                            //MessageBox.Show(x.Disk + "\r\n" + x.Completed.ToString());
                        }
                        
                    }
                    //MessageBox.Show(this.DiskStatus[11].ToString());
                    break;
                case DeviceEvent.DBT_DEVICEREMOVECOMPLETE://[REmove]
                    this.CheckDeviceStatus_Lable.BackColor = Color.Red;
                    this.CheckDeviceStatus_Lable.Text = "------No Connection------";
                    ArrayList Removable2 = this.GetRemovableDisk();
                    this.UsbDisk.Clear();
                    foreach (string x in Removable2)
                    {
                        this.dir = x;
                        this.HandleUsbDevice(x,0);
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
                    foreach (DiskStatus x in this.DiskStatus)
                    {
                        if (x != null)
                        {
                            MessageBox.Show(x.Disk + "\r\n"+x.Completed.ToString());
                        }

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
        public static FileSystemInfo[] GetFileInfo(DirectoryInfo dir)
        {
            try
            {
                FileSystemInfo[] files = dir.GetFileSystemInfos();
                return files;
            }
            catch
            {
                return null;
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

            FileSystemInfo[] files = GetFileInfo(dir);
            if (files == null)
            {
                return null;
            }
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i] as FileInfo;
                    //是文件 
                    if (file != null)
                    {
                        File.Add(file.FullName);
                        //对于子目录，进行递归调用 
                    }
                    else
                    {
                        temp = ListFiles(files[i]);
                    if (temp != null) {
                        foreach (string x in temp)
                        {
                            File.Add(x);
                            //MessageBox.Show(x);
                            //System.Console.WriteLine(x);
                        }
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
            Global_Filter();
            foreach(string disk in this.UsbDisk)
            {
                //System.Console.WriteLine(disk);
                this.BlockRiskFile(disk);
            }
            this.UpdateDiskStatus(this.dir, true);
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
            //MessageBox.Show("ejected");
            VolumeDeviceClass volumeDeviceClass = new VolumeDeviceClass();
            foreach (Volume device in volumeDeviceClass.Devices)
            {
                // is this volume on USB disks?
                if (!device.IsUsb)
                    continue;
                //MessageBox.Show("ejected");
                // is this volume a logical disk?
                if ((device.LogicalDrive == null) || (device.LogicalDrive.Length == 0))
                    continue;
                if (device.LogicalDrive.ToString() + "\\" == LogicalDisk)
                {
                    //MessageBox.Show("ejected");
                    //MessageBox.Show( device.Eject(true));
                    string str = device.Eject(true);
                    //MessageBox.Show(str);
                    System.Console.WriteLine(str);
                    this.DelDiskStatus(LogicalDisk);
                    // allow Windows to display any relevant UI
                    //MessageBox.Show(device.LogicalDrive.ToString());
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
            if (this.GetDiskStatus(listBox1.SelectedItem.ToString()) == 1){
                //MessageBox.Show(this.GetDiskStatus(listBox1.SelectedItem.ToString()).ToString());
                UnblockRiskFile(listBox1.SelectedItem.ToString());
                Eject(listBox1.SelectedItem.ToString());
            }
            else
            {
                if (this.GetDiskStatus(listBox1.SelectedItem.ToString()) == 1)
                {
                    MessageBox.Show("设备正在使用，请稍后再试");
                }
            }
            
        }

        private void label1_Click(object sender, EventArgs e)
        {
            //Win32.AnimateWindow(this.Handle, 400, Win32.AW_BLEND);
            Environment.Exit(0);
        }
        public ArrayList FileFilter(ArrayList Filelist)
        {
            string pat1 = @".exe$";
            ArrayList result = new ArrayList();
            Regex mc1 = new Regex(pat1, RegexOptions.IgnoreCase);
            foreach (string file in Filelist)
            {
                //System.Console.WriteLine(file);
                //System.Console.WriteLine(mc1.IsMatch(file));
                if (mc1.IsMatch(file))
                {
                    result.Add(file);
                }
            }
            string pat2 = @".js$";
            Regex mc2 = new Regex(pat2, RegexOptions.IgnoreCase);
            foreach (string file in Filelist)
            {
                if (mc2.IsMatch(file))
                {
                    result.Add(file);
                }
            }
            string pat3 = @".vbs$";
            Regex mc3 = new Regex(pat3, RegexOptions.IgnoreCase);
            foreach (string file in Filelist)
            {
                if (mc3.IsMatch(file))
                {
                    result.Add(file);
                }
            }
            return result;
        }
        public  void Global_Filter()
        {
            ArrayList ExecFile = new ArrayList();
            foreach(string disk in this.UsbDisk)
            {
                System.Console.WriteLine(disk);
                ExecFile =this.FileFilter((ArrayList)this.DiskFile[disk]);
                if (RiskFile.Contains(disk))
                {
                    RiskFile.Remove(disk);
                }
                RiskFile.Add(disk, ExecFile);
                foreach(string xx in ExecFile)
                {
                    System.Console.WriteLine(xx);
                }
            }
        }
        public void BlockRiskFile(string Disk)
        {
            //Global_Filter();
            FileStream file;
            ArrayList SingleDiskBlockedFile = new ArrayList();
            foreach (string Riskfile in (ArrayList)RiskFile[Disk])
            {
                //SingleDiskBlockedFile 
                file = new FileStream(Riskfile, FileMode.Append);
                file.Lock(1, 0);
                SingleDiskBlockedFile.Add(file);
                file = null;
            }
            if (this.BlockedFile.Contains(Disk))
            {
                this.BlockedFile.Remove(Disk);
            }
            this.BlockedFile.Add(Disk, SingleDiskBlockedFile);
            SingleDiskBlockedFile = null;
        }
        public void UnblockRiskFile(string Disk)
        {
            ArrayList List = (ArrayList)this.BlockedFile[Disk];
            foreach(FileStream stream in List)
            {
                stream.Dispose();
            }
        }
        public  void AddDiskStatus(string Disk,Boolean Completed)
        {
            //System.Console.WriteLine("success");
            for (int i = 0; i < 10; i++)
            {
                System.Console.WriteLine("success");
                if (this.DiskStatus[i] == null)
                {
                    this.DiskStatus[i] = new DiskStatus();
                    this.DiskStatus[i].Disk = Disk;
                    this.DiskStatus[i].Completed = Completed;
                    //System.Console.WriteLine("success");
                    return;
                }
                else
                {
                    if (this.DiskStatus[i].Disk == "")
                    {
                        this.DiskStatus[i].Disk = Disk;
                        this.DiskStatus[i].Completed = Completed;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }          
        }
        public void UpdateDiskStatus(string Disk,Boolean Completed)
        {
            for (int i = 0; i < 10; i++)
            {
                if (this.DiskStatus[i] != null)
                {
                    //this.DiskStatus[i] = new DiskStatus();
                    this.DiskStatus[i].Disk = Disk;
                    this.DiskStatus[i].Completed = Completed;
                    continue;
                }
                
            }
        }
        public void DelDiskStatus(string Disk)
        {
            for (int i = 0; i < 10; i++)
            {
                if (this.DiskStatus[i] != null)
                {
                    if (this.DiskStatus[i].Disk == Disk)
                    {
                        this.DiskStatus[i] = null;
                    }
                    continue;
                }
                else
                {
                    continue;
                }
            }
        }
        public int GetDiskStatus(string Disk)
        {
            for (int i = 0; i < 10; i++)
            {
                if (this.DiskStatus[i].Disk == Disk)
                {
                    if (this.DiskStatus[i].Completed == true)
                    {
                        return 1;
                    }
                    else
                    {
                        if (this.DiskStatus[i].Completed == false)
                        {
                            return 0;
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            return -1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global_Filter();
            FileStream file = new FileStream("J:\\bin\\Debug\\USBBlocker.exe", FileMode.Append);
            file.Lock(1, 0);
            FileStream x = file;
            file = null;
            //StreamWriter log = new StreamWriter(file);
            x.Close();
            //x.Unlock(1, 0);
            //x.Dispose();
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
public class DiskStatus
{
    public string Disk;
    public Boolean Completed;
}


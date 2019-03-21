using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RadarBidClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SmartRobot robot;

        // IE进程句柄
        private int ieHwdn;

        public MainWindow()
        {
            InitializeComponent();

            robot = new SmartRobot();

            robot.SetPath("D:\\work\\bid\\大漠插件\\雷达字库");
            robot.SetDict(0, "dict1.txt");

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.x1.Text + "," + this.y1.Text;
            int ret = robot.MoveTo(Int32.Parse(this.x1.Text), Int32.Parse(this.y1.Text));
            MessageBox.Show(msg + ", " + ret);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.x1.Text + "," + this.y1.Text;
            var tarText = this.targetText.Text;
            msg += ". " + tarText;
            int ret = robot.MoveTo(Int32.Parse(this.x1.Text), Int32.Parse(this.y1.Text));
            ret = robot.LeftClick();
            ret = robot.KeyPressString(tarText);

            MessageBox.Show(msg + ", " + ret);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            int x1 = Int32.Parse(this.x1.Text);
            int y1 = Int32.Parse(this.y1.Text);

            int h1 = Int32.Parse(this.h1.Text);
            int w1 = Int32.Parse(this.w1.Text);

            int x2 = x1 + w1;
            int y2 = y1 + h1;

            //int ret = robot.CaptureJpg(x1, y1, x2, y2, "test.jpg", 50);
            int ret = robot.Capture(x1, y1, x2, y2, "test2.bmp");

            // int ret = robot.CaptureJpg(0, 0, 2000, 2000, "test.jpg", 50);

            MessageBox.Show(" capture jpg result is " + ret + ", " + x1 + ", " + y1 + ", " + x2 + ", " + y2);
            
        }

        private void h1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void w1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            string msg = robot.GetMachineCode();
            MessageBox.Show(" code is " + msg);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //int hwdn = robot.FindWindow("Microsoft Edge", "");
            // 
            string className = this.processClassName.Text;
            string title = this.processTitle.Text;
            int hwdn = robot.FindWindow(className, title);
            int oHwdn = hwdn;
            string msg = "" + hwdn + ", IsUserAdministrator = " + IsUserAdministrator();
            msg += ", className [" + className + "], title [" + title + "]";

            /**
             * WINServer 2003 - IE className = IEFrame
             * WIN10 - IE = IEFrame
             * WIN10 - Edge className = AppplicationFrameWindow
             */

            //if (hwdn > 0 && IsUserAdministrator())
            //{
            //    msg = msg + ", " + robot.GetWindowClass(hwdn);
            //    msg = msg + ", " + robot.GetWindowTitle(hwdn);
            //}

            killIEProcess();

            //if (hwdn == 0)
            //{
                this.OpenWithStartInfo("http://119.3.64.205:8888/login.htm");
            ieHwdn = robot.FindWindow(className, title);
            //}

            //WindowsPrincipal myPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            //bool checkRet = myPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            //if (hwdn > 0)
            //{
            //    if (checkRet == false)
            //    {

            //        //show messagebox - displaying a messange to the user that rights are missing
            //        MessageBox.Show("You need to run the application using the \"run as administrator\" option", "administrator right required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    }
            //    else
            //    {
            //        //msg = msg + ", " + robot.GetWindowClass(hwdn);
            //        //msg = msg + ", " + robot.GetWindowTitle(hwdn);
            //        MessageBox.Show("You are good to go - application running in elevated mode", "Good job", MessageBoxButton.OK, MessageBoxImage.Information);
            //    }


            //}
            MessageBox.Show(" msg is " + msg + ", ieHwdn = " + ieHwdn);// + ", checkRet is " + checkRet);
        }

        // Uses the ProcessStartInfo class to start new processes,
        // both in a minimized mode.
        void OpenWithStartInfo(string url)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("iexplore.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;

            // Process.Start(startInfo);

            startInfo.Arguments = url;

            Process.Start(startInfo);
        }

        private void killIEProcess()
        {
            // Get all IEXPLORE processes.

            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("IEXPLORE");
            foreach (System.Diagnostics.Process proc in procs)
            {

                // Look for Google title.

                //if (proc.MainWindowTitle.IndexOf("Google") > -1)

                proc.Kill(); // Close it down.

            }
        }

        //private static void KillProcessAndChildren(int pid)
        //{
        //    // Cannot close 'system idle process'.
        //    if (pid == 0)
        //    {
        //        return;
        //    }
        //    ManagementObjectSearcher searcher = new ManagementObjectSearcher
        //            ("Select * From Win32_Process Where ParentProcessID=" + pid);
        //    ManagementObjectCollection moc = searcher.Get();
        //    foreach (ManagementObject mo in moc)
        //    {
        //        KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
        //    }
        //    try
        //    {
        //        Process proc = Process.GetProcessById(pid);
        //        proc.Kill();
        //    }
        //    catch (ArgumentException)
        //    {
        //        // Process already exited.
        //    }
        //}


        public bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        private void textBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {

        }

        private void processClassName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            int intX = 0;
            int intY = 0;
            // int ret = robot.FindStrFast(0, 0, 2000, 2000, "确定", "2e6e9e-2e6e9e", 0.8, out intX, out intY);

            //RGB单色差色识别
            //string ret2 = robot.OcrEx(200, 100, 2000, 2000, "2e6e9e-2e6e9e", 0.8);
            string ret2 = robot.OcrEx(0, 0, 2000, 2000, "2e6e9e-2e6e9e", 0.8);
            MessageBox.Show(" ocr s2 is " + ret2);

            if (ret2 != null && ret2.Length > 0)
            {
                string[] arr = ret2.Split('|');
                int len = arr[0].Length;
                // MessageBox.Show(" ocr arr is " + arr.ToArray().ToString());

                for (int i=0; i<len; i++)
                {
                    string a = arr[0].ToCharArray()[i].ToString();
                    if (a == "确" || a == "认")
                    {
                        string[] xy = arr[i+1].Split(',');

                        try
                        {
                            // TODO: 目前必须在全屏下才能成功正确找到 确定按钮
                            int x = Int32.Parse(xy[0]);// + 200;
                            int y = Int32.Parse(xy[1])// + 100;

                            //object x1 = x;
                            //object y1 = y;

                            //if (ieHwdn == 0)
                            //{
                            //    ieHwdn = robot.FindWindow("IEFrame", "客车");
                            //}

                            MessageBox.Show(" 1 ocr arr is " + x + ", " + y + ", ieHwdn = " + ieHwdn);
                            

                            //if (ieHwdn > 0)
                            //{
                            //    robot.ScreenToClient(ieHwdn, ref x1, ref y1);
                            //    MessageBox.Show(" 2 ocr arr is " + x + ", " + y + ", x1 = " + x1 + ", y1 = " + y1);
                            //}
                            
                            robot.MoveTo(x, y);
                            // 
                            robot.LeftClick();
                            robot.LeftClick();
                        } catch (Exception e2)
                        {
                            Console.WriteLine("ok get it" + e2);
                        }
                        break;
                        // Console.WriteLine("ok get it");
                    }
                }

                
            }

            // MessageBox.Show(" msg is intX = " + intX + ", intY = " + intY + ", ret = " + ret2);
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            string ret = robot.OcrInFile(0, 0, 2000, 2000, "test2.bmp", "2e6e9e-2e6e9e", 0.8);

            MessageBox.Show(" ocr ret is " + ret);

           

        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            robot.SetPath("D:\\work\\bid\\大漠插件\\雷达字库");
            robot.SetDict(0, "dict1.txt");
        }
    }
}

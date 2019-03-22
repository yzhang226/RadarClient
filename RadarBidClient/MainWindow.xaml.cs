using log4net;
using log4net.Config;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
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

        private static readonly ILog logger = LogManager.GetLogger(typeof(MainWindow));


        private SmartRobot robot;

        // IE进程句柄
        private int ieHwdn;

        public MainWindow()
        {
            InitializeComponent();

            robot = new SmartRobot();

            robot.SetPath("D:\\work\\bid\\大漠插件\\雷达字库");
            robot.SetDict(0, "dict2.txt");

            // Set up a simple configuration that logs on the console.
            //BasicConfigurator.Configure();

            logger.InfoFormat("launch ... {0}", DateTime.Now);



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
            // MessageBox.Show(msg + ", " + ret);
            logger.InfoFormat("msg is {0}", ret);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.x1.Text + "," + this.y1.Text;
            var tarText = this.targetText.Text;
            msg += ". " + tarText;
            int ret = robot.MoveTo(Int32.Parse(this.x1.Text), Int32.Parse(this.y1.Text));
            ret = robot.LeftClick();
            ret = robot.KeyPressString(tarText);

            //MessageBox.Show(msg + ", " + ret);
            logger.InfoFormat("msg is {0}", ret);
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


            //MessageBox.Show(" capture jpg result is " + ret + ", " + x1 + ", " + y1 + ", " + x2 + ", " + y2);

            logger.InfoFormat(" capture jpg result is " + ret + ", " + x1 + ", " + y1 + ", " + x2 + ", " + y2);

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
            logger.InfoFormat(" code is " + msg);
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
            // MessageBox.Show(" msg is " + msg + ", ieHwdn = " + ieHwdn);// + ", checkRet is " + checkRet);

            logger.InfoFormat(" msg is " + msg + ", ieHwdn = " + ieHwdn);// + ", checkRet is " + checkRet);


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
            // 点击 - 可能的  确认 按钮
            long t1 = currentTs();
            var p1 = this.searchConfirmXYInScreen("2e6e9e-2e6e9e", new List<string> { "确", "认" });
            if (p1.x > 0 && p1.y > 0)
            {
                logger.InfoFormat("找到 - 确认按钮 - {0}, {1}", p1.x, p1.y);
                robot.MoveTo(p1.x, p1.y);
                robot.LeftClick();
                robot.LeftClick();
                logger.InfoFormat("点击了 - 确认按钮 - {0}, {1}, {2}", p1.x, p1.y, currentTs() - t1);
            }

            // 点击 - 可能的 我同意拍卖须知 按钮
            t1 = currentTs();
            var p2 = this.searchConfirmXYInScreen("2e6e9e-2e6e9e", new List<string> { "确", "认" });
            if (p2.x > 0 && p2.y > 0)
            {
                logger.InfoFormat("找到 - 我同意拍卖须知按钮 - {0}, {1}", p2.x, p2.y);
                robot.MoveTo(p2.x, p2.y);
                robot.LeftClick();
                robot.LeftClick();
                logger.InfoFormat("点击了 - 我同意拍卖须知按钮 - {0}, {1}, {2}", p2.x, p2.y, currentTs() - t1);
            }

            // 输入 - 投标号输入框
            t1 = currentTs();
            var p3 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "投", "标", "号" });
            if (p3.x > 0 && p3.y > 0)
            {
                logger.InfoFormat("找到 - 投标号输入框 - {0}, {1}", p3.x, p3.y);
                robot.MoveTo(p3.x + 100, p3.y);
                robot.LeftClick();
                //robot.LeftClick();
                robot.KeyPressString("22222222");
                logger.InfoFormat("输入了 - 投标号输入框 - {0}, {1}, {2}", p3.x, p3.y, currentTs() - t1);
            }

            // 输入 - 密码输入框
            t1 = currentTs();
            var p4 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "密", "码" });
            if (p4.x > 0 && p4.y > 0)
            {
                logger.InfoFormat("找到 - 密码输入框 - {0}, {1}", p4.x, p4.y);
                robot.MoveTo(p4.x + 100, p4.y );
                robot.LeftClick();
                //robot.LeftClick();
                robot.KeyPressString("2222");
                logger.InfoFormat("输入了 - 密码输入框 - {0}, {1}, {2}", p4.x, p4.y, currentTs() - t1);
            }

            // 输入 - 图像校验码输入框
            t1 = currentTs();
            var p5 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "图", "像", "校", "验" });
            if (p5.x > 0 && p5.y > 0)
            {
                logger.InfoFormat("找到 - 图像校验码输入框 - {0}, {1}", p5.x, p5.y);
                robot.MoveTo(p5.x + 150, p5.y);
                robot.LeftClick();
                //robot.LeftClick();
                robot.KeyPressString("301726");
                logger.InfoFormat("输入了 - 图像校验码输入框 - {0}, {1}, {2}", p5.x, p5.y, currentTs() - t1);
            }

            // 点击 - 可能的 参加投标竞买 按钮
            t1 = currentTs();
            var p6 = this.searchConfirmXYInScreen("ffffff-3898e2", new List<string> { "参", "加" });
            if (p6.x > 0 && p6.y > 0)
            {
                logger.InfoFormat("找到 - 参加投标竞买 按钮 - {0}, {1}", p6.x, p6.y);
                robot.MoveTo(p6.x + 10, p6.y + 2);
                robot.LeftClick();
                // robot.LeftClick();
                logger.InfoFormat("点击了 - 参加投标竞买 按钮 - {0}, {1}, {2}", p6.x, p6.y, currentTs() - t1);
            }

            // phase1
            // flash没有准备好
            Thread.Sleep(4 * 1000);

            // 输入 - 输入价格 输入框
            t1 = currentTs();
            var p11 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "输", "入", "价" });
            if (p11.x > 0 && p11.y > 0)
            {
                logger.InfoFormat("找到 - 输入价格 输入框 - {0}, {1}", p11.x, p11.y);
                robot.MoveTo(p11.x + 100, p11.y + 2);
                robot.LeftClick();
                robot.LeftClick();
                robot.KeyPressString("89000");
                logger.InfoFormat("输入了 - 输入价格 输入框 - {0}, {1}, {2}", p11.x, p11.y, currentTs() - t1);
            }

            // 输入 - 再次输入价格 输入框
            t1 = currentTs();
            var p12 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "再", "次" });
            if (p12.x > 0 && p12.y > 0)
            {
                logger.InfoFormat("找到 - 再次输入价格 输入框 - {0}, {1}", p12.x, p12.y);
                robot.MoveTo(p12.x + 170, p12.y + 2);
                robot.LeftClick();
                robot.LeftClick();
                robot.KeyPressString("89000");
                logger.InfoFormat("输入了 - 再次输入价格 输入框 - {0}, {1}, {2}", p12.x, p12.y, currentTs() - t1);

                // 点击出价
                t1 = currentTs();
                int ret = robot.MoveTo(p12.x + 50 + 200 + 50, p12.y + 2);
                robot.LeftClick();
                logger.InfoFormat("尝试点击 - 出价 按钮 - {0}, {1}, {2}", p12.x, p12.y, currentTs() - t1);

                // 输入验证


            }

        }

        private SimplePoint searchConfirmXYInScreen(string colorForamt, List<string> cons)
        {
            //string ret2 = robot.OcrEx(0, 0, 2000, 2000, colorForamt, 0.8);
            string ret2 = robot.OcrEx(500, 150, 1500, 900, colorForamt, 0.8);
            logger.InfoFormat(" OCR识别的内容是 {0}", ret2);

            SimplePoint point = new SimplePoint();

            if (ret2 == null || ret2.Length == 0)
            {
                return point;
            }

            string[] arr = ret2.Split('|');
            int len = arr[0].Length;

            for (int i = 0; i < len; i++)
            {
                string a = arr[0].ToCharArray()[i].ToString();
                if (cons.Contains(a)) 
                {
                    string[] xy = arr[i + 1].Split(',');
                    // TODO: 目前必须在全屏下才能成功正确找到 确定按钮
                    int x = Int32.Parse(xy[0]);// + 200;
                    int y = Int32.Parse(xy[1]);// + 100;
                    
                    point.x = x;
                    point.y = y;

                    break;
                }
            }

            return point;
        }

        private long currentTs()
        {
            long currentTicks = DateTime.Now.Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (currentTicks - dtFrom.Ticks) / 10000;
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            string ret = robot.OcrInFile(0, 0, 2000, 2000, "test2.bmp", "2e6e9e-2e6e9e", 0.8);

            logger.InfoFormat(" ocr ret is " + ret);

           

        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            robot.SetPath("D:\\work\\bid\\大漠插件\\雷达字库");
            robot.SetDict(0, "dict1.txt");
        }

        private void mockLogin_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

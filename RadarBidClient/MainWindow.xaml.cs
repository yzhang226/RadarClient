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

        private SimplePoint bench;

        public MainWindow()
        {
            InitializeComponent();

            robot = new SmartRobot();

            // robot.SetPath("D:\\work\\bid\\大漠插件\\雷达字库");
            robot.SetDict(0, "dict2003-02.txt");

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
            string className = this.processClassName.Text;
            string title = this.processTitle.Text;
            int hwdn = robot.FindWindow(className, title);
            int oHwdn = hwdn;
            string msg = "" + hwdn + ", IsUserAdministrator = " + IsUserAdministrator();
            msg += ", className [" + className + "], title [" + title + "]";

            killIEProcess();

            this.OpenWithStartInfo("http://119.3.64.205:8888/login.htm");
            ieHwdn = robot.FindWindow(className, title);

            logger.InfoFormat(" msg is " + msg + ", ieHwdn = " + ieHwdn);// + ", checkRet is " + checkRet);
        }

        // Uses the ProcessStartInfo class to start new processes,
        // both in a minimized mode.
        void OpenWithStartInfo(string url)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("iexplore.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            startInfo.Arguments = url;
            Process.Start(startInfo);
        }

        private void killIEProcess()
        {
            // Get all IEXPLORE processes.
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("IEXPLORE");
            foreach (System.Diagnostics.Process proc in procs)
            {
                proc.Kill(); // Close it down.
            }
        }

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

        private void mockStart_Click(object sender, RoutedEventArgs e)
        {
            //this.mockInWay1();

            bench = this.detectBenchPoint();

            // 首屏 确定 按钮
            var p11 = bench.AddDelta(741, 507);
            this.clickConfirmAtIndex(p11);

            // 首屏 同意 按钮
            var p12 = bench.AddDelta(730, 509);
            this.clickAgreeAtIndex(p11);

            // 登录页
            var p21 = bench.AddDelta(610, 200);
            var p22 = bench.AddDelta(610, 255);
            var p23 = bench.AddDelta(610, 314);

            var p24 = bench.AddDelta(630, 378);

            this.inputBidNumberAtLogin(p21, "22222222");
            this.inputPasswordAtLogin(p22, "2222");
            this.inputCaptchaAtLogin(p23, "301726");
            this.clickLoginAtLogin(p24);

            // 第一阶段
            Thread.Sleep(4 * 1000);
            var p31 = bench.AddDelta(690, 314);
            var p32 = bench.AddDelta(690, 373);

            var p33 = bench.AddDelta(800, 375);

            var p34 = bench.AddDelta(744, 416);
            var p35 = bench.AddDelta(552, 498);

            this.inputPriceAtPhase1(p31, 89000);
            this.inputPrice2AtPhase1(p32, 89000);

            this.clickBidButtonAtPhase1(p33);
            
            // TODO: 验证码 - 打码
            this.inputCaptchAtPhase1(p34, "5787");
            this.clickConfirmCaptchaAtPhase1(p35);

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            Thread.Sleep(2 * 1000);
            var p36 = bench.AddDelta(661, 478);
            this.clickConfirmBidOkAtPhase1(p36);

        }

        private void mockPhase2_Click(object sender, RoutedEventArgs e)
        {
            logger.InfoFormat("第二阶段 使用基准点 {0}", bench);

            // 第二阶段
            var p41 = bench.AddDelta(676, 417);
            var p42 = bench.AddDelta(800, 415);

            this.inputPriceAtPhase2(p41, 89000);
            this.clickBidButtonAtPhase2(p42);

            var p43 = bench.AddDelta(734, 416);
            var p44 = bench.AddDelta(553, 500);

            this.inputCaptchAtPhase2(p43, "0282");
            this.clickConfirmCaptchaAtPhase2(p44);

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            Thread.Sleep(2 * 1000);
            var p36 = bench.AddDelta(661, 478);
            this.clickConfirmBidOkAtPhase2(p36);

        }

        private SimplePoint getScreenResolution()
        {
            int screenWidth = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth);
            int screenHeight = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight);

            return new SimplePoint(screenWidth, screenHeight);
        }

        private SimplePoint detectBenchPoint()
        {
            long t1 = currentTs();
            // 尝试使用 相对位置 - 上海市个人非营业性客车额度投标拍卖
            // 个人非营业性客车
            // 您使用的是
            // var checkPoint = this.searchTextCoordXYInScreen("2e6e9e-2e6e9e", "用的是");

            var checkPoint = this.searchTextCoordXYInScreen("0074bf-101010|9c4800-101010|ffdf9c-101010|df9c48-101010|489cdf-101010|000000-101010|9cdfff-101010|00489c-101010", "用的是");
            // 
            logger.InfoFormat("检查点坐标是 - {0}. {1}", checkPoint.ToString(), currentTs() - t1);

            // 204, 287
            var bench = checkPoint.AddDelta(-206, -286);
            // 719, 364 - = 513，78 ， (510, 78)
            logger.InfoFormat("基准点坐标是 - {0}", bench.ToString());


            // 900 x 700
            int w = 900;
            int h = 700;

            if (bench.x <= 0 || bench.y <= 0)
            {
                var re = getScreenResolution();
                logger.InfoFormat("降级使用 - 屏幕分辨率 - {0}", re.ToString());

                bench.y = 79;
                if (re.y > h + 150)
                {
                    // 不会有 scroll
                    bench.x = (re.x - w) / 2;
                } else
                {
                    bench.x = (re.x - w - 15) / 2;
                }

                logger.InfoFormat("降级使用 - 新的基准点坐标是 - {0}", bench.ToString());
            }

            return bench;
        }

        // 首屏 确定 按钮
        private void clickConfirmAtIndex(SimplePoint p1)
        {
            long t1 = currentTs();
            if (p1.x > 0 && p1.y > 0)
            {
                logger.InfoFormat("找到 - 确认按钮 - {0}, {1}", p1.x, p1.y);
                robot.MoveTo(p1.x, p1.y);
                robot.LeftClick();
                logger.InfoFormat("点击了 - 确认按钮 - {0}, {1}, {2}", p1.x, p1.y, currentTs() - t1);
            }
        }

        // 首屏 同意 按钮
        private void clickAgreeAtIndex(SimplePoint p2)
        {
            long t1 = currentTs();
            if (p2.x > 0 && p2.y > 0)
            {
                logger.InfoFormat("找到 - 我同意拍卖须知按钮 - {0}, {1}", p2.x, p2.y);
                robot.MoveTo(p2.x, p2.y);
                robot.LeftClick();
                robot.LeftClick();
                logger.InfoFormat("点击了 - 我同意拍卖须知按钮 - {0}, {1}, {2}", p2.x, p2.y, currentTs() - t1);
            }
        }

        // 登录页  投标号 输入框
        private void inputBidNumberAtLogin(SimplePoint p3, string bidNumber)
        {
            long t1 = currentTs();
            if (p3.x > 0 && p3.y > 0)
            {
                logger.InfoFormat("找到 - 投标号输入框 - {0}, {1}", p3.x, p3.y);
                robot.MoveTo(p3.x, p3.y);
                robot.LeftClick();
                robot.KeyPressString(bidNumber);
                logger.InfoFormat("输入了 - 投标号输入框 - {0}, {1}, {2}", p3.x, p3.y, currentTs() - t1);
            }
        }

        // 登录页   密码 输入框
        private void inputPasswordAtLogin(SimplePoint p4, string password)
        {
            long t1 = currentTs();
            if (p4.x > 0 && p4.y > 0)
            {
                logger.InfoFormat("找到 - 密码输入框 - {0}, {1}", p4.x, p4.y);
                robot.MoveTo(p4.x, p4.y);
                robot.LeftClick();
                robot.KeyPressString(password);
                logger.InfoFormat("输入了 - 密码输入框 - {0}, {1}, {2}", p4.x, p4.y, currentTs() - t1);
            }
        }

        // 登录页   图像校验码 输入框
        private void inputCaptchaAtLogin(SimplePoint p5, string captcha)
        {
            long t1 = currentTs();
            if (p5.x > 0 && p5.y > 0)
            {
                logger.InfoFormat("找到 - 图像校验码 输入框 - {0}, {1}", p5.x, p5.y);
                robot.MoveTo(p5.x, p5.y);
                robot.LeftClick();
                robot.KeyPressString(captcha);
                logger.InfoFormat("输入了 - 图像校验码 输入框 - {0}, {1}, {2}", p5.x, p5.y, currentTs() - t1);
            }
        }

        // 登录页 参加投标竞买 按钮
        private void clickLoginAtLogin(SimplePoint p6)
        {
            long t1 = currentTs();
            if (p6.x > 0 && p6.y > 0)
            {
                logger.InfoFormat("找到 - 参加投标竞买 按钮 - {0}, {1}", p6.x, p6.y);
                robot.MoveTo(p6.x + 10, p6.y + 2);
                robot.LeftClick();
                logger.InfoFormat("点击了 - 参加投标竞买 按钮 - {0}, {1}, {2}", p6.x, p6.y, currentTs() - t1);
            }
        }

        // 第一阶段页 输入价格 输入框
        private void inputPriceAtPhase1(SimplePoint p11, int price)
        {
            long t1 = currentTs();
            if (p11.x > 0 && p11.y > 0)
            {
                logger.InfoFormat("找到 - 输入价格 输入框 - {0}, {1}", p11.x, p11.y);
                robot.MoveTo(p11.x, p11.y);
                robot.LeftClick();
                robot.KeyPressString(price.ToString());
                logger.InfoFormat("第一阶段 输入 - 输入价格 输入框 - {0}, {1}, {2}", p11.x, p11.y, currentTs() - t1);
            }
        }

        // 第一阶段页 再次输入价格 输入框
        private void inputPrice2AtPhase1(SimplePoint p12, int price)
        {
            long t1 = currentTs();
            if (p12.x > 0 && p12.y > 0)
            {
                logger.InfoFormat("找到 - 再次输入价格 输入框 - {0}, {1}", p12.x, p12.y);
                robot.MoveTo(p12.x, p12.y);
                robot.LeftClick();
                robot.KeyPressString(price.ToString());
                logger.InfoFormat("第一阶段 输入 - 再次输入价格 输入框 - {0}, {1}, {2}", p12.x, p12.y, currentTs() - t1);
            }
        }

        // 第一阶段页 出价 按钮
        private void clickBidButtonAtPhase1(SimplePoint p12)
        {
            long t1 = currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 出价 按钮 - {0}, {1}, {2}", p12.x, p12.y, currentTs() - t1);
        }

        // 第一阶段页 弹框 验证码 输入框
        private void inputCaptchAtPhase1(SimplePoint p13, string captcha)
        {
            long t1 = currentTs();
            if (p13.x > 0 && p13.y > 0)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
                robot.KeyPressString(captcha);
                logger.InfoFormat("第一阶段 尝试输入 - 验证码 输入框 - {0}, {1}, {2}", p13.x, p13.y, currentTs() - t1);
            }
        }

        // 第一阶段页 弹框 验证码 确认 按钮
        private void clickConfirmCaptchaAtPhase1(SimplePoint p12)
        {
            long t1 = currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 验证码 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, currentTs() - t1);
        }

        // 第一阶段页 弹框 出价结果 确认 按钮
        private void clickConfirmBidOkAtPhase1(SimplePoint p12)
        {
            long t1 = currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 出价结果 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, currentTs() - t1);
        }

        // 第二阶段页 自行输入价格 输入框
        private void inputPriceAtPhase2(SimplePoint p13, int price)
        {
            long t1 = currentTs();
            if (p13.x > 0 && p13.y > 0)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
                robot.KeyPressString(price.ToString());
                logger.InfoFormat("第二阶段 尝试输入 - 自行输入价格 输入框 - {0}, {1} {2}. {3}", p13.x, p13.y, price, currentTs() - t1);
            }
        }

        // 第二阶段页 出价 按钮 
        private void clickBidButtonAtPhase2(SimplePoint p13)
        {
            long t1 = currentTs();
            if (p13.x > 0 && p13.y > 0)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
                logger.InfoFormat("第二阶段 尝试点击 - 出价 按钮 - {0}, {1}. {2}", p13.x, p13.y, currentTs() - t1);
            }
        }

        // 第二阶段页 弹框 验证码 输入框
        private void inputCaptchAtPhase2(SimplePoint p13, string captcha)
        {
            long t1 = currentTs();
            if (p13.x > 0 && p13.y > 0)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
                robot.KeyPressString(captcha);
                logger.InfoFormat("第二阶段 尝试输入 - 验证码 输入框 - {0}, {1}, {2}. {3}", p13.x, p13.y, captcha, currentTs() - t1);
            }
        }

        // 第二阶段页 弹框 验证码 确认 按钮
        private void clickConfirmCaptchaAtPhase2(SimplePoint p12)
        {
            long t1 = currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第二阶段 尝试点击 - 验证码 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, currentTs() - t1);
        }

        // 第二阶段页 弹框 出价结果 确认 按钮
        private void clickConfirmBidOkAtPhase2(SimplePoint p12)
        {
            long t1 = currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第二阶段 尝试点击 - 出价结果 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, currentTs() - t1);
        }

        private void mockInWay1()
        {
            // 点击 - 可能的  确认 按钮
            long t1 = currentTs();
            var p1 = this.searchConfirmXYInScreen("2e6e9e-2e6e9e", new List<string> { "确", "认" });


            // 点击 - 可能的 我同意拍卖须知 按钮
            t1 = currentTs();
            var p2 = this.searchConfirmXYInScreen("2e6e9e-2e6e9e", new List<string> { "确", "认" });


            // 输入 - 投标号输入框
            t1 = currentTs();
            var p3 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "投", "标", "号" });


            // 输入 - 密码输入框
            t1 = currentTs();
            var p4 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "密", "码" });


            // 输入 - 图像校验码输入框
            t1 = currentTs();
            var p5 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "图", "像", "校", "验" });
            

            // 点击 - 可能的 参加投标竞买 按钮
            t1 = currentTs();
            var p6 = this.searchConfirmXYInScreen("ffffff-3898e2", new List<string> { "参", "加" });


            // phase1
            // flash没有准备好
            Thread.Sleep(4 * 1000);

            // 输入 - 输入价格 输入框
            t1 = currentTs();
            var p11 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "输", "入", "价" });


            // 输入 - 再次输入价格 输入框
            t1 = currentTs();
            var p12 = this.searchConfirmXYInScreen("686868-686868", new List<string> { "再", "次" });
            if (p12.x > 0 && p12.y > 0)
            {

                // 点击出价
                t1 = currentTs();
                int ret = robot.MoveTo(p12.x + 50 + 200 + 50, p12.y + 2);
                robot.LeftClick();
                logger.InfoFormat("尝试点击 - 出价 按钮 - {0}, {1}, {2}", p12.x, p12.y, currentTs() - t1);

                // 输入 - 第一阶段验证码
                t1 = currentTs();
                var p13 = this.searchConfirmXYInScreen("2e6e9e-2e6e9e", new List<string> { "请", "在", "右" });
                if (p13.x > 0 && p13.y > 0)
                {


   

                }





            }
        }

        private SimplePoint searchTextCoordXYInScreen(string colorForamt, string target)
        {
            string ret = robot.OcrEx(0, 0, 2000, 2000, colorForamt, 0.8);
            logger.InfoFormat(" 2000 OCR 识别的内容是 {0}", ret);

            SimplePoint point = new SimplePoint();

            if (ret == null || ret.Length == 0)
            {
                return point;
            }

            int idx = ret.IndexOf(target);
            if (idx < 0)
            {
                return point;
            }

            string[] arr = ret.Split('|');
            int len = arr[0].Length;

            string[] xy = arr[idx + 1].Split(',');
            // TODO: 目前必须在全屏下才能成功正确找到 确定按钮
            int x = Int32.Parse(xy[0]);
            int y = Int32.Parse(xy[1]);

            point.x = x;
            point.y = y;
            
            return point;
        }

        private SimplePoint searchConfirmXYInScreen(string colorForamt, List<string> cons)
        {
            //string ret2 = robot.OcrEx(0, 0, 2000, 2000, colorForamt, 0.8);
            // string ret2 = robot.OcrEx(500, 150, 1500, 900, colorForamt, 0.8);
            string ret2 = robot.OcrEx(250, 100, 1500, 900, colorForamt, 0.8);
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

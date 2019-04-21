using log4net;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace RadarBidClient
{
    public class BidderMocker
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BidderMocker));

        // TODO: 这里应该有更好的方式使用 单例 
        public static BidderMocker mocker = null;


        private SmartRobot robot;

        private SimplePoint bench;

        // IE进程句柄
        private int ieHwdn;


        private string processClassName = "IEFrame";

        private string processTitle = "客车";

        private bool debugModeOpen = true;
      


        // private string bidingWebsiteAddress = "http://119.3.64.205:8888/login.htm";

        private string bidingWebsiteAddress = "http://127.0.0.1:8888/bid.htm";

        private Dictionary<DateTime, PagePrice> timePriceMap = new Dictionary<DateTime, PagePrice>();
        private List<PagePrice> pagePrices = new List<PagePrice>();

        // 坐标 of 目前时间
        private SimplePoint coordOfCurrentTime { get; set; }

        // 坐标 of 价格区间
        private SimplePoint coordOfPriceSection { get; set; }

        public BidderMocker(SmartRobot robot)
        {
            this.robot = robot;

            BidderMocker.mocker = this;
        }

        public void ReopenNewBidWindow()
        {
            this.ReopenNewBidWindow(bidingWebsiteAddress);
        }

        public void ReopenNewBidWindow(string biddingAddress)
        {
            int prevHwdn = robot.FindWindow(processClassName, processTitle);
            int oHwdn = prevHwdn;

            killIEProcess();

            this.OpenWithStartInfo(biddingAddress);
            this.ieHwdn = robot.FindWindow(processClassName, processTitle);

            logger.InfoFormat("Previous IE hwdn#{0}, New IE Hwdn#{1}", prevHwdn, ieHwdn);
        }

        // Uses the ProcessStartInfo class to start new processes, both in a Maximized mode.
        private void OpenWithStartInfo(string url)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("iexplore.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            startInfo.Arguments = url;
            Process.Start(startInfo);
        }

        private void killIEProcess()
        {
            // Get all IEXPLORE processes.
            Process[] procs = Process.GetProcessesByName("IEXPLORE");
            foreach (Process proc in procs)
            {
                proc.Kill(); // Close it down.
            }
        }

        public void awaitPrice(int targetPrice, int targetMinute, int targetSecond)
        {
            int ret = 0;
            do
            {
                DateTime no = DateTime.Now;
                if (no.Minute < targetMinute)
                {

                }
                else if (no.Minute == targetMinute)
                {
                    if (no.Second <= targetSecond)
                    {
                        PagePrice pp = detectTimeOfPhase02(targetPrice);
                        if (pp.status == 0)
                        {
                            this.MockPhase022(targetPrice);
                        }
                    }
                    else if (no.Second > targetSecond)
                    {
                        // 直接提交
                        this.MockPhase022(targetPrice);
                    } 
                    else
                    {
                        break;
                    }
                    
                } else
                {
                    logger.InfoFormat("minute of time over");
                    break;
                }
                KK.Sleep(100);
            } while (ret < 0);
        }

        public void afterDetectPriceAndTime(int targetPrice, int targetMinute, int targetSecond)
        {
            if (pagePrices.Count == 0)
            {
                return;
            }

            PagePrice last = pagePrices.LastOrDefault();
            if (last == null)
            {
                return;
            }

            if (targetPrice == last.basePrice)
            {
                this.MockPhase022(targetPrice);
            }

        }

        public PagePrice detectPriceAndTimeInScreen()
        {
            long t1 = KK.currentTs();
            string uuid = KK.uuid();
            // PagePrice pp = new PagePrice();

            if (this.bench == null)
            {
                setBenchPoint();
            }

            // 找到坐标 of 目前时间
            //if (debugMode)
            //{
            //    robot.CaptureJpg(bench.x, bench.y, bench.x + 900, bench.y + 700, uuid + "-001.jpg", 80);
            //} 目前价时目前价目前时间
            if (this.coordOfCurrentTime == null)
            {
                this.findAndSetCoordOfCurrentTime();
                if (this.coordOfCurrentTime == null)
                {
                    logger.WarnFormat("this.coordOfCurrentTime is null");
                    return null;
                }
            }
            var p = this.coordOfCurrentTime;

            logger.DebugFormat("目前时间 - 坐标是 - {0}. {1}", p.ToString(), KK.currentTs() - t1);
            
            if (p.x <= 0 || p.y <= 0)
            {
                return null;
            }

            // 11:29:57
            int x1 = p.x + 55, y1 = p.y, x2 = p.x + 55 + 75, y2 = p.y + 18;
            if (debugModeOpen)
            {
                robot.CaptureJpg(x1, y1, x2, y2, "" + uuid + "-01.jpg", 80);
                KK.Sleep(900);
            }

            // ff0000-ff0000  ff0000-101010
            long s1 = KK.currentTs();
            string ret1 = robot.Ocr(x1, y1, x2, y2, "ff0000-000000", 0.8);
            logger.InfoFormat("目前时间 - OCR内容 {0}, {1}, {2}, {3}. elapsed {4}ms, {5}, {6}", x1, y1, x2, y2, KK.currentTs() - s1, ret1, uuid);

            if (ret1 == null || ret1.Length == 0)
            {
                return null;
            }

            DateTime no = DateTime.Now;

            char[] cs1 = ret1.ToCharArray();
            string[] arr1 = new string[3] { "", "", "" };
            int idx1 = 0;
            foreach (char c in cs1)
            {
                if (c >= '0' && c <= '9')
                {
                    int mod = idx1 / 2;
                    string st = arr1[mod] + c.ToString();
                    arr1[mod] = st;
                    idx1 += 1;
                }
            }

            logger.InfoFormat(" datetime arr is {0}, {1}, {2}", arr1[0], arr1[1], arr1[2]);

            DateTime dt = new DateTime(no.Year, no.Month, no.Day, int.Parse(arr1[0]), int.Parse(arr1[1]), int.Parse(arr1[2]));

            // KK.timeToInt(dt)
            // TODO: 检测是否已经拿到过该秒的数据，则可以忽略不检测价格了

            //logger.InfoFormat(" datetime parsed is {0}, arr is {1}", dt, arr1);

            // 找到坐标 of 价格区间
            if (this.coordOfPriceSection == null)
            {
                findAndSetCoordOfPriceSection();
                if (this.coordOfPriceSection == null)
                {
                    logger.WarnFormat("this.coordOfPriceSection is null");
                    return null;
                }
            }

            var p2 = this.coordOfPriceSection;
            logger.DebugFormat("价格区间 - 坐标是 - {0}. {1}", p2.ToString(), KK.currentTs() - t1);

            // 11:29:57
            int x21 = p2.x + 55, y21 = p2.y, x22 = p2.x + 55 + 170, y22 = p2.y + 18;
            //if (debugMode)
            //{
            //    robot.CaptureJpg(x21, y21, x22, y22, "" + uuid + "-02.jpg", 80);
            //}

            // ff0000-101010 
            s1 = KK.currentTs();
            string ret2 = robot.Ocr(x21, y21, x22, y22, "ff0000-000000", 0.8);

            logger.InfoFormat("价格区间 - OCR内容 elapsed {0}ms, {1}, {2}", KK.currentTs() - s1, ret2, uuid);

            char[] cs2 = ret2.ToCharArray();
            string numberStr = "";

            foreach (char c in cs2)
            {
                if (c >= '0' && c <= '9')
                {
                    numberStr += c.ToString();
                }
            }

            if (numberStr.Length % 2 != 0)
            {
                logger.WarnFormat("识别到 错误的 价格 - {0}. 数字的位数不是2的整数倍", numberStr);
                return null;
            }

            int mod2 = numberStr.Length / 2;           

            string[] arr2 = new string[2] { numberStr.Substring(0, mod2), numberStr.Substring(mod2, mod2) };

            int priceLow = int.Parse(arr2[0]);
            int priceHigh = int.Parse(arr2[1]);

            if (priceHigh < 70000 || priceHigh < 70000)
            {
                logger.WarnFormat("识别到 错误的 价格 - {0}, {1}.", priceLow, priceHigh);
                return null;
            }

            logger.InfoFormat("price parsed priceLow is {0}, pricHigh is {1}", priceLow, priceHigh);

            int currentPrice = (priceLow + priceHigh) / 2;

            var pp = new PagePrice(dt, currentPrice);
            pp.status = 0;
            pp.low = priceLow;
            pp.high = priceHigh;

            timePriceMap[dt] = pp;
            
            if (pagePrices.Contains(pp))
            {
                pagePrices.Remove(pp);
            }
            pagePrices.Add(pp);

            return pp;
        }

        public void findAndSetCoordOfCurrentTime()
        {
            var p = robot.searchTextCoordXYInFlashScreen(bench.x + 20, bench.y + 365, 370, 190, "0066cc-101010", "目前时间");
            if (p != null && p.x > 0 && p.y > 0)
            {
                this.coordOfCurrentTime = p;
            }
        }

        public void findAndSetCoordOfPriceSection()
        {
            // 找到坐标 of 价格区间
            var p = robot.searchTextCoordXYInFlashScreen(bench.x + 20, bench.y + 365, 371, 190, "0066cc-101010", "价格区间");
            if (p != null && p.x > 0 && p.y > 0)
            {
                this.coordOfPriceSection = p;
            }
        }

        public PagePrice detectTimeOfPhase02(int targetPrice)
        {
            
            
            //pp.currentPrice = currentPrice;


            //if (currentPrice == targetPrice)
            //{
            //    pp.status = 0;
            //} else if (currentPrice > targetPrice)
            //{
            //    pp.status = 1;
            //} else
            //{
            //    pp.status = -1;
            //}

            return null;
        }

        public void setBenchPoint()
        {
            bench = this.detectBenchPoint();

        }

        public void MockLoginAndPhase1()
        {

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

        public void MockPhase2()
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

        public void MockPhase022(int targetPrice)
        {
            logger.InfoFormat("第二阶段修改 - 使用基准点 {0}", bench);

            // 第二阶段
            var p41 = bench.AddDelta(676, 417);
            var p42 = bench.AddDelta(800, 415);

            this.inputPriceAtPhase2(p41, targetPrice);
            KK.Sleep(10);
            this.clickBidButtonAtPhase2(p42);
            KK.Sleep(10);

            var p43 = bench.AddDelta(734, 416);
            var p44 = bench.AddDelta(553, 500);

            // TODO: 检测 enter 键

            this.inputCaptchAtPhase2(p43, "0282");
            this.clickConfirmCaptchaAtPhase2(p44);

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            KK.Sleep(1500);
            var p36 = bench.AddDelta(661, 478);
            this.clickConfirmBidOkAtPhase2(p36);

            // TODO: 确定按钮的位置 可能 会变化, 则检测 


        }

        // 点掉 可能的 弹出框 上的 确定 按钮
        private void clickPossiblePopupSureButton()
        {
            long t1 = KK.currentTs();
            int x = bench.x + 655;
            int y = bench.y + 450;
            int ret = robot.MoveTo(x, y);
            robot.LeftClick();
            logger.InfoFormat("第二阶段 尝试点击 - 出价结果 确认 按钮 - {0}, {1}, {2}", x, y, KK.currentTs() - t1);
        }

        private SimplePoint getScreenResolution()
        {
            int screenWidth = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth);
            int screenHeight = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight);

            return new SimplePoint(screenWidth, screenHeight);
        }

        private SimplePoint detectBenchPoint()
        {
            long t1 = KK.currentTs();
            // 尝试使用 相对位置 - 上海市个人非营业性客车额度投标拍卖
            // 个人非营业性客车
            // 您使用的是
            // var checkPoint = this.searchTextCoordXYInScreen("2e6e9e-2e6e9e", "用的是");

            var checkPoint = robot.searchTextCoordXYInScreen("0074bf-101010|9c4800-101010|ffdf9c-101010|df9c48-101010|489cdf-101010|000000-101010|9cdfff-101010|00489c-101010", "用的是");
            // 
            logger.InfoFormat("检查点坐标是 - {0}. {1}", checkPoint.ToString(), KK.currentTs() - t1);

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

                //bench.y = 79;
                //if (re.y > h + 150)
                //{
                //    // 不会有 scroll
                //    bench.x = (re.x - w) / 2;
                //}
                //else
                //{
                //    bench.x = (re.x - w - 15) / 2;
                //}

                // 内嵌模式
                // 682 x 23
                bench.x = 682;
                bench.y = 23;

                logger.InfoFormat("降级使用 - 新的基准点坐标是 - {0}", bench.ToString());
            }

            return bench;
        }

        // 首屏 确定 按钮
        private void clickConfirmAtIndex(SimplePoint p1)
        {
            long t1 = KK.currentTs();
            if (p1.x > 0 && p1.y > 0)
            {
                logger.InfoFormat("找到 - 确认按钮 - {0}, {1}", p1.x, p1.y);
                robot.MoveTo(p1.x, p1.y);
                robot.LeftClick();
                logger.InfoFormat("点击了 - 确认按钮 - {0}, {1}, {2}", p1.x, p1.y, KK.currentTs() - t1);
            }
        }

        // 首屏 同意 按钮
        private void clickAgreeAtIndex(SimplePoint p2)
        {
            long t1 = KK.currentTs();
            if (p2.x > 0 && p2.y > 0)
            {
                logger.InfoFormat("找到 - 我同意拍卖须知按钮 - {0}, {1}", p2.x, p2.y);
                robot.MoveTo(p2.x, p2.y);
                robot.LeftClick();
                robot.LeftClick();
                logger.InfoFormat("点击了 - 我同意拍卖须知按钮 - {0}, {1}, {2}", p2.x, p2.y, KK.currentTs() - t1);
            }
        }

        // 登录页  投标号 输入框
        private void inputBidNumberAtLogin(SimplePoint p3, string bidNumber)
        {
            long t1 = KK.currentTs();
            if (p3.x > 0 && p3.y > 0)
            {
                logger.InfoFormat("找到 - 投标号输入框 - {0}, {1}", p3.x, p3.y);
                robot.MoveTo(p3.x, p3.y);
                robot.LeftClick();
                robot.KeyPressString(bidNumber);
                logger.InfoFormat("输入了 - 投标号输入框 - {0}, {1}, {2}", p3.x, p3.y, KK.currentTs() - t1);
            }
        }

        // 登录页   密码 输入框
        private void inputPasswordAtLogin(SimplePoint p4, string password)
        {
            long t1 = KK.currentTs();
            if (p4.x > 0 && p4.y > 0)
            {
                logger.InfoFormat("找到 - 密码输入框 - {0}, {1}", p4.x, p4.y);
                robot.MoveTo(p4.x, p4.y);
                robot.LeftClick();
                robot.KeyPressString(password);
                logger.InfoFormat("输入了 - 密码输入框 - {0}, {1}, {2}", p4.x, p4.y, KK.currentTs() - t1);
            }
        }

        // 登录页   图像校验码 输入框
        private void inputCaptchaAtLogin(SimplePoint p5, string captcha)
        {
            long t1 = KK.currentTs();
            if (p5.x > 0 && p5.y > 0)
            {
                logger.InfoFormat("找到 - 图像校验码 输入框 - {0}, {1}", p5.x, p5.y);
                robot.MoveTo(p5.x, p5.y);
                robot.LeftClick();
                robot.KeyPressString(captcha);
                logger.InfoFormat("输入了 - 图像校验码 输入框 - {0}, {1}, {2}", p5.x, p5.y, KK.currentTs() - t1);
            }
        }

        // 登录页 参加投标竞买 按钮
        private void clickLoginAtLogin(SimplePoint p6)
        {
            long t1 = KK.currentTs();
            if (p6.x > 0 && p6.y > 0)
            {
                logger.InfoFormat("找到 - 参加投标竞买 按钮 - {0}, {1}", p6.x, p6.y);
                robot.MoveTo(p6.x + 10, p6.y + 2);
                robot.LeftClick();
                logger.InfoFormat("点击了 - 参加投标竞买 按钮 - {0}, {1}, {2}", p6.x, p6.y, KK.currentTs() - t1);
            }
        }

        // 第一阶段页 输入价格 输入框
        private void inputPriceAtPhase1(SimplePoint p11, int price)
        {
            long t1 = KK.currentTs();
            if (p11.x > 0 && p11.y > 0)
            {
                logger.InfoFormat("找到 - 输入价格 输入框 - {0}, {1}", p11.x, p11.y);
                robot.MoveTo(p11.x, p11.y);
                robot.LeftClick();
                robot.KeyPressString(price.ToString());
                logger.InfoFormat("第一阶段 输入 - 输入价格 输入框 - {0}, {1}, {2}", p11.x, p11.y, KK.currentTs() - t1);
            }
        }

        // 第一阶段页 再次输入价格 输入框
        private void inputPrice2AtPhase1(SimplePoint p12, int price)
        {
            long t1 = KK.currentTs();
            if (p12.x > 0 && p12.y > 0)
            {
                logger.InfoFormat("找到 - 再次输入价格 输入框 - {0}, {1}", p12.x, p12.y);
                robot.MoveTo(p12.x, p12.y);
                robot.LeftClick();
                robot.KeyPressString(price.ToString());
                logger.InfoFormat("第一阶段 输入 - 再次输入价格 输入框 - {0}, {1}, {2}", p12.x, p12.y, KK.currentTs() - t1);
            }
        }

        // 第一阶段页 出价 按钮
        private void clickBidButtonAtPhase1(SimplePoint p12)
        {
            long t1 = KK.currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 出价 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.currentTs() - t1);
        }

        // 第一阶段页 弹框 验证码 输入框
        private void inputCaptchAtPhase1(SimplePoint p13, string captcha)
        {
            long t1 = KK.currentTs();
            if (p13.x > 0 && p13.y > 0)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
                robot.KeyPressString(captcha);
                logger.InfoFormat("第一阶段 尝试输入 - 验证码 输入框 - {0}, {1}, {2}", p13.x, p13.y, KK.currentTs() - t1);
            }
        }

        // 第一阶段页 弹框 验证码 确认 按钮
        private void clickConfirmCaptchaAtPhase1(SimplePoint p12)
        {
            long t1 = KK.currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 验证码 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.currentTs() - t1);
        }

        // 第一阶段页 弹框 出价结果 确认 按钮
        private void clickConfirmBidOkAtPhase1(SimplePoint p12)
        {
            long t1 = KK.currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 出价结果 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.currentTs() - t1);
        }

        // 第二阶段页 自行输入价格 输入框
        private void inputPriceAtPhase2(SimplePoint p13, int price)
        {
            long t1 = KK.currentTs();
            if (p13.x > 0 && p13.y > 0)
            {
                
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();

                // 先清空已输入的价格
                for (int i = 0; i < 6; i++)
                {
                    robot.PressBackspacKey();
                    robot.PressDeleteKey();
                    // KK.Sleep(1);
                }

                robot.KeyPressString(price.ToString());
                logger.InfoFormat("第二阶段 尝试输入 - 自行输入价格 输入框 - {0}, {1} {2}. {3}", p13.x, p13.y, price, KK.currentTs() - t1);
            }
        }

        // 第二阶段页 出价 按钮 
        private void clickBidButtonAtPhase2(SimplePoint p13)
        {
            long t1 = KK.currentTs();
            if (p13.x > 0 && p13.y > 0)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
                logger.InfoFormat("第二阶段 尝试点击 - 出价 按钮 - {0}, {1}. {2}", p13.x, p13.y, KK.currentTs() - t1);
            }
        }

        // 第二阶段页 弹框 验证码 输入框
        private void inputCaptchAtPhase2(SimplePoint p13, string captcha)
        {
            long t1 = KK.currentTs();
            if (p13.x > 0 && p13.y > 0)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
                robot.KeyPressString(captcha);
                logger.InfoFormat("第二阶段 尝试输入 - 验证码 输入框 - {0}, {1}, {2}. {3}", p13.x, p13.y, captcha, KK.currentTs() - t1);
            }
        }

        // 第二阶段页 弹框 验证码 确认 按钮
        private void clickConfirmCaptchaAtPhase2(SimplePoint p12)
        {
            long t1 = KK.currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第二阶段 尝试点击 - 验证码 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.currentTs() - t1);
        }

        // 第二阶段页 弹框 出价结果 确认 按钮
        private void clickConfirmBidOkAtPhase2(SimplePoint p12)
        {
            long t1 = KK.currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第二阶段 尝试点击 - 出价结果 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.currentTs() - t1);
        }


    }
}

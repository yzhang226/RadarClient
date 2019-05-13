using log4net;
using RadarBidClient.common;
using RadarBidClient.ioc;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RadarBidClient
{
    [Component]
    public class BidActionManager
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BidActionManager));


        private WindowSimulator robot;

        private ProjectConfig conf;

        private CoordPoint Datum;

        // IE进程句柄
        private int ieHwdn;


        private string processClassName = "IEFrame";

        private string processTitle = "客车";

        private bool debugModeOpen = false;

        // private string bidingWebsiteAddress = "http://127.0.0.1:8888/bid.htm";

        

        // 坐标 of 目前时间
        private CoordPoint coordOfCurrentTime { get; set; }

        // 坐标 of 价格区间
        private CoordPoint coordOfPriceSection { get; set; }

        public BidActionManager(WindowSimulator robot, ProjectConfig conf)
        {
            this.robot = robot;
            this.conf = conf;
        }

        public void ReopenNewBidWindow()
        {
            this.ReopenNewBidWindow(conf.BidAddressPrefix + "/bid.htm");
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
        

        public void ResetDictIndex()
        {
            robot.UseDict(0);
        }

        public PageTimePriceResult detectPriceAndTimeInScreen(PageTimePriceResult LastResult)
        {
            long t1 = KK.currentTs();
            string uuid = KK.uuid();
            // PagePrice pp = new PagePrice();


            if (this.Datum == null)
            {
                setBenchPoint();
            }

            if (this.coordOfCurrentTime == null || coordOfCurrentTime.x <= 0 || coordOfCurrentTime.y <= 0)
            {
                return PageTimePriceResult.ErrorCoordTime();
            }

            var p = this.coordOfCurrentTime;

            // 11:29:57
            // int x1 = p.x + 55, y1 = p.y, x2 = p.x + 55 + 75, y2 = p.y + 18;
            int x1 = p.x + 20, y1 = p.y, x2 = p.x + 20 + 150, y2 = p.y + 18;
            //if (debugModeOpen)
            //{
            //    robot.CaptureJpg(x1, y1, x2, y2, "" + uuid + "-01.jpg", 80);
            //    KK.Sleep(900);
            //}

            // ff0000-ff0000  ff0000-101010
            long s1 = KK.currentTs();
            robot.UseDict(DictIndex.INDEX_NUMBER);
            string ret1 = robot.Ocr(x1, y1, x2, y2, "ff0000-000000", 0.8);
            // string ret1 = robot.Ocr(x1, y1, x2, y2, "0066cc-101010", 0.8);
            logger.InfoFormat("目前时间 - OCR内容 {0}, {1}, {2}, {3}. elapsed {4}ms, {5}, {6}", x1, y1, x2, y2, KK.currentTs() - s1, ret1, uuid);


            if (ret1 == null || ret1.Length == 0 || ret1.Length < 6)
            {
                return PageTimePriceResult.ErrorTime();
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

            if (arr1[0].Length < 2 || arr1[1].Length < 2 || arr1[2].Length < 2)
            {
                return PageTimePriceResult.ErrorTime();
            }

            logger.InfoFormat(" datetime arr is {0}, {1}, {2}.", arr1[0], arr1[1], arr1[2]);

            DateTime dt = new DateTime(no.Year, no.Month, no.Day, int.Parse(arr1[0]), int.Parse(arr1[1]), int.Parse(arr1[2]));
            
            // 检测是否已经拿到过该秒的数据，则可以忽略不检测价格了
            if (LastResult?.data != null && dt == LastResult.data.pageTime)
            {
                return PageTimePriceResult.RepeatedTime();
            }

            //logger.InfoFormat(" datetime parsed is {0}, arr is {1}", dt, arr1);

            // 找到坐标 of 价格区间
            if (this.coordOfPriceSection == null || coordOfPriceSection.x <= 0 || coordOfPriceSection.y <= 0)
            {
                return PageTimePriceResult.ErrorCoordPrice();
            }

            var p2 = this.coordOfPriceSection;
            logger.DebugFormat("价格区间 - 坐标是 - {0}. {1}", p2.ToString(), KK.currentTs() - t1);

            // 11:29:57
            // int x21 = p2.x + 55, y21 = p2.y, x22 = p2.x + 55 + 170, y22 = p2.y + 18;
            int x21 = p2.x + 20, y21 = p2.y, x22 = p2.x + 20 + 250, y22 = p2.y + 18;
            if (debugModeOpen)
            {
                robot.CaptureJpg(x21, y21, x22, y22, getImageDirPath() + "debug\\" + uuid + "-02.jpg", 80);
                KK.Sleep(900);
            }

            // ff0000-101010 
            s1 = KK.currentTs();
            robot.UseDict(DictIndex.INDEX_NUMBER);
            string ret2 = robot.Ocr(x21, y21, x22, y22, "ff0000-000000", 0.8);
            // string ret2 = robot.Ocr(x21, y21, x22, y22, "0066cc-101010", 0.8);
            logger.InfoFormat("价格区间 - OCR内容 elapsed {0}ms, {1}, {2}.", KK.currentTs() - s1, ret2, uuid);

            if (ret2 == null || ret2.Length == 0 || ret2.Length < 10)
            {
                return PageTimePriceResult.ErrorPrice();
            }

            string numberStr = KK.ExtractNumber(ret2);
            if (numberStr.Length < 10)
            {
                logger.WarnFormat("识别到 错误的 价格 - {0}. 数字的位数不对.", numberStr);
                return PageTimePriceResult.ErrorPrice();
            }
            
            int[] arr2 = ParsePrice(numberStr);
            int priceLow = arr2[0];
            int priceHigh = arr2[1];

            if (priceHigh < 70000 || priceHigh < 70000 || priceHigh > 200000 || priceLow > 200000)
            {
                logger.WarnFormat("识别到 错误的 价格 - {0}, {1}.", priceLow, priceHigh);
                return PageTimePriceResult.ErrorPrice();
            }

            logger.InfoFormat("price parsed priceLow is {0}, pricHigh is {1}", priceLow, priceHigh);

            int currentPrice = (priceLow + priceHigh) / 2;

            var pp = new PagePrice(dt, currentPrice);
            pp.low = priceLow;
            pp.high = priceHigh;

            return PageTimePriceResult.Ok(pp);
        }

        private int[] ParsePrice(string numberStr)
        {
            string price1 = "", price2 = "";
            if (numberStr.Length == 10)
            {
                price1 = numberStr.Substring(0, 5);
                price2 = numberStr.Substring(5, 5);
            }
            else
            {
                price1 = numberStr.Substring(0, 5);
                if (int.Parse(price1) < 99700)
                {
                    price2 = numberStr.Substring(numberStr.Length - 5, 5);
                }
                else
                {
                    price2 = numberStr.Substring(numberStr.Length - 5, 5);
                    if (int.Parse(price2) < 90000)
                    {
                        price2 = numberStr.Substring(numberStr.Length - 6, 6);
                    }
                }

            }

            return new int[] { int.Parse(price1), int.Parse(price2) };
        }

        public void findAndSetCoordOfCurrentTime()
        {
            robot.UseDict(DictIndex.INDEX_CURRENT_TIME);
            var p = robot.searchTextCoordXYInFlashScreen(Datum.x + 20, Datum.y + 365, 370, 190, "0066cc-101010", "目前时间");
            if (p != null && p.x > 0 && p.y > 0)
            {
                logger.InfoFormat("find coord of current-time is {0}", p.ToString());
                this.coordOfCurrentTime = p;
            }
        }

        public void findAndSetCoordOfPriceSection()
        {
            // 找到坐标 of 价格区间
            robot.UseDict(DictIndex.INDEX_PRICE_SECTION);
            var p = robot.searchTextCoordXYInFlashScreen(Datum.x + 20, Datum.y + 365, 371, 190, "0066cc-101010", "价格区间");
            if (p != null && p.x > 0 && p.y > 0)
            {
                logger.InfoFormat("find coord of price-range is {0}", p.ToString());

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

        private static List<CoordPoint> FenceEndPoints = new List<CoordPoint>();

        private static List<CoordPoint> FenceEndPointsReverse = new List<CoordPoint>();

        public void setBenchPoint()
        {
            Datum = this.detectBenchPoint();
            // 设置 验证码区域的 确认/取消 按钮 的 栅栏
            // 440 438
            // 按钮长宽 114 x 27
            // 栅栏 起始点 delta(440, 448) 长宽 422 x 87
            // 假设 每一个cell 长宽是  55 x 21
            var startPoint = Datum.AddDelta(440, 448);
            // int areaLen = 425, areaWidth = 87;
            int columns = 4;
            int rows = 3;

            logger.InfoFormat("fence startPoint is {0}.", startPoint.ToString());

            // int cellLen = areaLen / columns, cellWidth = areaWidth / rows;
            int cellLen = 113, cellWidth = 29;

            for (int c = 1; c <= columns; c++) 
            {
                for (int r = 1; r <= rows; r++)
                {
                    var fence = startPoint.AddDelta(c * cellLen, r * cellWidth);
                    logger.InfoFormat("fence is row={0} column={1}, point is {2}.", r, c, fence.ToString());
                    FenceEndPoints.Add(fence);
                }
            }

            FenceEndPointsReverse = new List<CoordPoint>(FenceEndPoints);
            FenceEndPointsReverse.Reverse();

            logger.InfoFormat("init FenceEndPoints, size {0}", FenceEndPoints.Count);

        }


        public void MockLogin()
        {

            this.setBenchPoint();

            Task.Factory.StartNew(() =>
            {
                // 首屏 确定 按钮
                var p11 = Datum.AddDelta(741, 507);
                // Task task1 = 
                this.clickConfirmAtIndex(p11);

                // 首屏 同意 按钮
                var p12 = Datum.AddDelta(730, 509);
                this.clickAgreeAtIndex(p11);

                // 登录页
                var p21 = Datum.AddDelta(610, 200);
                var p22 = Datum.AddDelta(610, 255);
                var p23 = Datum.AddDelta(610, 314);

                var p24 = Datum.AddDelta(630, 378);

                this.inputBidNumberAtLogin(p21, "22222222");
                this.inputPasswordAtLogin(p22, "2222");
                this.inputCaptchaAtLogin(p23, "301726");
                this.clickLoginAtLogin(p24);
                
            });
        }


        public void MockLoginAndPhase1()
        {

            this.setBenchPoint();


            Task.Factory.StartNew(() => {


                // 首屏 确定 按钮
                var p11 = Datum.AddDelta(741, 507);
                // Task task1 = 
                this.clickConfirmAtIndex(p11);
                // Task.WaitAll(task1);

                // 首屏 同意 按钮
                var p12 = Datum.AddDelta(730, 509);
                // Task task2 = 
                this.clickAgreeAtIndex(p11);
                // Task.WaitAll(task2);

                // 登录页
                var p21 = Datum.AddDelta(610, 200);
                var p22 = Datum.AddDelta(610, 255);
                var p23 = Datum.AddDelta(610, 314);

                var p24 = Datum.AddDelta(630, 378);

                this.inputBidNumberAtLogin(p21, "22222222");
                this.inputPasswordAtLogin(p22, "2222");
                this.inputCaptchaAtLogin(p23, "301726");
                this.clickLoginAtLogin(p24);

                // 第一阶段
                Thread.Sleep(4 * 1000);
                var p31 = Datum.AddDelta(690, 314);
                var p32 = Datum.AddDelta(690, 373);

                var p33 = Datum.AddDelta(800, 375);

                var p34 = Datum.AddDelta(744, 416);
                var p35 = Datum.AddDelta(552, 498);

                this.inputPriceAtPhase1(p31, 89000);
                this.inputPrice2AtPhase1(p32, 89000);

                this.clickBidButtonAtPhase1(p33);

                // TODO: 验证码 - 打码
                this.inputCaptchAtPhase1(p34, "5787");
                this.clickConfirmCaptchaAtPhase1(p35);

                // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
                Thread.Sleep(2 * 1000);
                var p36 = Datum.AddDelta(661, 478);
                this.clickConfirmBidOkAtPhase1(p36);
            });
        }

        public void MockPhase2()
        {
            logger.InfoFormat("第二阶段 使用基准点 {0}", Datum);

            // 第二阶段
            var p41 = Datum.AddDelta(676, 417);
            var p42 = Datum.AddDelta(800, 415);

            this.inputPriceAtPhase2(p41, 89000);
            this.ClickOfferBtn(p42);

            var p43 = Datum.AddDelta(734, 416);
            var p44 = Datum.AddDelta(553, 500);

            this.InputCaptchAtPoint(p43, "0282");
            this.ClickBtnUseFenceFromLeftToRight(p44);

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            Thread.Sleep(2 * 1000);
            var p36 = Datum.AddDelta(661, 478);
            this.ClickBtnOnceAtPoint(p36);
        }

        public CoordPoint MockPhase2AtCaptcha(int bidPrice)
        {
            logger.InfoFormat("第二阶段 使用基准点 {0}", Datum);

            // 第二阶段
            var p41 = Datum.AddDelta(676, 417);
            var p42 = Datum.AddDelta(800, 415);

            this.inputPriceAtPhase2(p41, bidPrice);
            this.ClickOfferBtn(p42);

            return p41;
        }

        public void MockCancelPhase2AtCaptcha()
        {
            logger.InfoFormat("第二阶段 使用基准点 {0}", Datum);

            // 第二阶段 742 502
            var p42 = Datum.AddDelta(742, 502);

            // TODO: 取消按钮的可能会变化，所以这里使用全点击的方式
            // 按钮长宽 114 x 27
            // 空闲长宽 421 x 76

            foreach (var p in FenceEndPointsReverse)
            {
                robot.MoveTo(p.x, p.y);
                robot.LeftClick();
                // KK.Sleep(50);
            }


            // this.clickCancelButtonAtPhase2(p42);

        }

        public void MockPhase022(int targetPrice)
        {
            logger.InfoFormat("第二阶段修改 - 使用基准点 {0}", Datum);

            // 第二阶段
            var p41 = Datum.AddDelta(676, 417);
            var p42 = Datum.AddDelta(800, 415);

            this.inputPriceAtPhase2(p41, targetPrice);
            KK.Sleep(10);
            this.ClickOfferBtn(p42);
            KK.Sleep(10);

            var p43 = Datum.AddDelta(734, 416);
            var p44 = Datum.AddDelta(553, 500);

            // TODO: 检测 enter 键

            // 上传验证码 

            this.InputCaptchAtPoint(p43, "0282");
            this.ClickBtnUseFenceFromLeftToRight(p44);

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            KK.Sleep(1500);
            var p36 = Datum.AddDelta(661, 478);
            this.ClickBtnOnceAtPoint(p36);

            // TODO: 确定按钮的位置 可能 会变化, 则检测 


        }


        public void MockPhase022(int targetPrice, BiddingContext context)
        {
            logger.InfoFormat("第二阶段修改 - 使用基准点 {0}", Datum);

            // 第二阶段
            var p41 = Datum.AddDelta(676, 417);
            var p42 = Datum.AddDelta(800, 415);

            this.inputPriceAtPhase2(p41, targetPrice);
            KK.Sleep(2);
            this.ClickOfferBtn(p42);
            KK.Sleep(2);

            var p43 = Datum.AddDelta(734, 416);
            var p44 = Datum.AddDelta(553, 500);

            // TODO: 检测 enter 键

            // 对验证码区域截屏且上传 
            KK.Sleep(100);
            CaptchaAnswerImage img = CaptureCaptchaAndUploadTask();
            context.PutAwaitImage(img, null);

            string answer = "";
            while (true)
            {
                try
                {
                    answer = context.GetAnswer(img.Uuid);
                    // TODO: 是否考虑超时
                    if (answer?.Length > 0)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("", e);
                }
                finally
                {
                    KK.Sleep(50);
                }

            }

            logger.InfoFormat("get answer#{0} for task#{1}", answer, img.Uuid);
            this.InputCaptchAtPoint(p43, answer);
            this.ClickBtnUseFenceFromLeftToRight(p44);

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            // TODO: 应该检测 区域 是否有 出价有效
            KK.Sleep(800);
            var p36 = Datum.AddDelta(661, 478);
            this.ClickBtnOnceAtPoint(p36);

            // 清除以前输入的价格
            this.CleanPriceAtPoint(p41, true);


        }



        public CaptchaAnswerImage CaptureCaptchaAndUploadTask()
        {   
            CaptchaAnswerImage img = CaptureCaptchaImageAtPoint();
            // UploadPhase2CaptchaImage(img);

            return img;
        }

        // 点掉 可能的 弹出框 上的 确定 按钮
        private void clickPossiblePopupSureButton()
        {
            long t1 = KK.currentTs();
            int x = Datum.x + 655;
            int y = Datum.y + 450;
            int ret = robot.MoveTo(x, y);
            robot.LeftClick();
            logger.InfoFormat("第二阶段 尝试点击 - 出价结果 确认 按钮 - {0}, {1}, {2}", x, y, KK.currentTs() - t1);
        }

        private CoordPoint getScreenResolution()
        {
            int screenWidth = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth);
            int screenHeight = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight);

            return new CoordPoint(screenWidth, screenHeight);
        }

        private CoordPoint detectBenchPoint()
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
                if (re.x > 1900)
                {
                    // 不会有 scroll
                    // bench.x = (re.x - w) / 2;
                    bench.x = 682;
                    bench.y = 23;
                }
                else
                {
                    // bench.x = (re.x - w - 15) / 2;
                    bench.x = 354;
                    bench.y = 19;
                }

                // 内嵌模式
                // 682 x 23
                // bench.x = 682;
                // bench.y = 23;

                logger.InfoFormat("降级使用 - 新的基准点坐标是 - {0}", bench.ToString());
            }

            return bench;
        }

        // 首屏 确定 按钮
        private void clickConfirmAtIndex(CoordPoint p1)
        {
            long t1 = KK.currentTs();
            if (p1.x > 0 && p1.y > 0)
            {
                logger.InfoFormat("找到 - 确认按钮 - {0}, {1}", p1.x, p1.y);
                robot.MoveTo(p1.x, p1.y);
                robot.LeftClick();
                logger.InfoFormat("点击了 - 确认按钮 - {0}, {1}, {2}", p1.x, p1.y, KK.currentTs() - t1);
            }
            //return 1;

            //return /*null*/;
        }

        // 首屏 同意 按钮
        private void clickAgreeAtIndex(CoordPoint p2)
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
            //return Task.Factory.StartNew(() => {
                
            //    return 1;
            //});
        }

        // 登录页  投标号 输入框
        private void inputBidNumberAtLogin(CoordPoint p3, string bidNumber)
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
        private void inputPasswordAtLogin(CoordPoint p4, string password)
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
        private void inputCaptchaAtLogin(CoordPoint p5, string captcha)
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
        private void clickLoginAtLogin(CoordPoint p6)
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
        private void inputPriceAtPhase1(CoordPoint p11, int price)
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
        private void inputPrice2AtPhase1(CoordPoint p12, int price)
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
        private void clickBidButtonAtPhase1(CoordPoint p12)
        {
            long t1 = KK.currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 出价 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.currentTs() - t1);
        }

        // 第一阶段页 弹框 验证码 输入框
        private void inputCaptchAtPhase1(CoordPoint p13, string captcha)
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
        private void clickConfirmCaptchaAtPhase1(CoordPoint p12)
        {
            long t1 = KK.currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 验证码 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.currentTs() - t1);
        }

        // 第一阶段页 弹框 出价结果 确认 按钮
        private void clickConfirmBidOkAtPhase1(CoordPoint p12)
        {
            long t1 = KK.currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 出价结果 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.currentTs() - t1);
        }

        // 第二阶段页 自行输入价格 输入框
        private void inputPriceAtPhase2(CoordPoint p13, int price)
        {
            long t1 = KK.currentTs();
            if (p13.x > 0 && p13.y > 0)
            {
                
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();

                CleanPriceAtPoint(p13, false);

                robot.KeyPressString(price.ToString());
                logger.InfoFormat("第二阶段 尝试输入 - 自行输入价格 输入框 - {0}, {1} {2}. {3}", p13.x, p13.y, price, KK.currentTs() - t1);
            }
        }

        public void InputPriceAtPoint(CoordPoint coord, int price)
        {
            long t1 = KK.currentTs();
            robot.MoveTo(coord.x, coord.y);
            robot.LeftClick();

            CleanPriceAtPoint(coord, false);

            robot.KeyPressString(price.ToString());

            logger.DebugFormat("输入 - 自行输入价格 输入框 - {0}, {1}, elapsed {2}", coord.ToString(), price, KK.currentTs() - t1);
        }

        public void CleanPriceAtPoint(CoordPoint p13, bool NeedMoveTo)
        {
            if (NeedMoveTo)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
            }
            
            // 先清空已输入的价格
            for (int i = 0; i < 6; i++)
            {
                robot.PressBackspacKey();
                robot.PressDeleteKey();
            }
        }

        // 第二阶段页 出价 按钮 
        public void ClickOfferBtn(CoordPoint p13)
        {
            long t1 = KK.currentTs();
            robot.MoveTo(p13.x, p13.y);
            robot.LeftClick();
            robot.LeftClick();
            logger.InfoFormat("点击 出价 按钮 - {0}, elapsed {1}.", p13.ToString(), KK.currentTs() - t1);
        }

        // 第二阶段页 出价 按钮 
        private void clickCancelButtonAtPhase2(CoordPoint p13)
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
        public void InputCaptchAtPoint(CoordPoint p13, string captcha)
        {
            long t1 = KK.currentTs();
            robot.MoveTo(p13.x, p13.y);
            robot.LeftClick();

            // 先清空已输入的验证码
            for (int i = 0; i < 4; i++)
            {
                robot.PressBackspacKey();
                robot.PressDeleteKey();
            }

            robot.KeyPressString(captcha);
            logger.InfoFormat("输入 - 验证码 - {0}, {1}, elapsed {2}.", p13.ToString(), captcha, KK.currentTs() - t1);
        }

        // 第二阶段页 弹框 验证码 确认 按钮
        public void ClickBtnUseFenceFromLeftToRight(CoordPoint p12)
        {
            long t1 = KK.currentTs();

            // 按钮的位置 可能 会变化, 这里使用 栅栏模式多次点击
            foreach (var p in FenceEndPoints)
            {
                robot.MoveTo(p.x, p.y);
                robot.LeftClick();
            }

            logger.DebugFormat("栅栏模式（从右到左） 点击 - 按钮 - {0}, elpased {1}.", p12.ToString(), KK.currentTs() - t1);
        }

        public void ClickBtnUseFenceFromRightToLeft(CoordPoint p12)
        {
            long t1 = KK.currentTs();

            // 按钮的位置 可能 会变化, 这里使用 栅栏模式多次点击
            foreach (var p in FenceEndPointsReverse)
            {
                robot.MoveTo(p.x, p.y);
                robot.LeftClick();
            }

            logger.DebugFormat("栅栏模式（从左到右） 点击 - 按钮 - {0}, elpased {1}.", p12.ToString(), KK.currentTs() - t1);
        }

        // 第二阶段页 弹框 出价结果 确认 按钮
        public void ClickBtnOnceAtPoint(CoordPoint p12)
        {
            long t1 = KK.currentTs();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("点击 - 按钮（一次） - {0}, elapsed {1}.", p12.ToString(), KK.currentTs() - t1);
        }

        /// <summary>
        /// 第二阶段 - 截图验证码区域 且 上传验证码图片
        /// </summary>
        /// <returns></returns>
        public CaptchaAnswerImage CaptureCaptchaImageAtPoint()
        {
            var Datum = GetDatum();

            DateTime dt = DateTime.Now;
            var uuid = KK.uuid();
            CaptchaAnswerImage img = new CaptchaAnswerImage();
            img.Uuid = uuid;
            img.CaptureTime = dt;

            // 442 338 ， 380 53
            int x11 = Datum.x + 442, y11 = Datum.y + 338;
            int x21 = x11 + 380, y21 = y11 + 53;
            var img01Path = getImageDirPath() + "" + uuid + "-" + dt.ToString("HHmmss") + "-phase02-01.jpg";
            int ret1 = robot.CaptureJpg(x11, y11, x21, y21, img01Path, 80);
            img.ImagePath1 = img01Path;

            // 基准点偏移 445 390, 240 85
            int x1 = Datum.x + 445, y1 = Datum.y + 390;
            int x2 = x1 + 230, y2 = y1 + 90;
            var img02Path = getImageDirPath() + "" + uuid + "-" + dt.ToString("HHmmss") + "-phase02-02.jpg";
            int ret2 = robot.CaptureJpg(x1, y1, x2, y2, img02Path, 80);

            img.ImagePath2 = img02Path;

            return img;
        }

        public int CaptureImage(CoordRectangle rect, string filePath)
        {
            return robot.CaptureJpg(rect.x1, rect.y1, rect.x2, rect.y2, filePath, 90);
        }


        public string getImageDirPath()
        {
            // d:\work\bid\radarbid\radarbidclient\radarbidclient\bin\x86\debug\resource\dlls\
            string path = robot.GetBasePath();
            int idx = path.LastIndexOf("\\resource\\");
            return path.Substring(0, idx) + "\\Captures\\";
        }

        public CoordPoint GetDatum()
        {
            return this.Datum;
        }

    }
}

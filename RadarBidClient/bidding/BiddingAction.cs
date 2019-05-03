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

namespace RadarBidClient
{
    [Component]
    public class BidActionManager
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BidActionManager));


        private WindowSimulator robot;

        private ProjectConfig conf;

        private SimplePoint bench;

        // IE进程句柄
        private int ieHwdn;


        private string processClassName = "IEFrame";

        private string processTitle = "客车";

        private bool debugModeOpen = false;

        // private string bidingWebsiteAddress = "http://127.0.0.1:8888/bid.htm";

        //private Dictionary<DateTime, PagePrice> timePriceMap = new Dictionary<DateTime, PagePrice>();
        // private List<PagePrice> pagePrices = new List<PagePrice>();

        // 坐标 of 目前时间
        private SimplePoint coordOfCurrentTime { get; set; }

        // 坐标 of 价格区间
        private SimplePoint coordOfPriceSection { get; set; }

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

        //public void awaitPrice(int targetPrice, int targetMinute, int targetSecond)
        //{
        //    int ret = 0;
        //    do
        //    {
        //        DateTime no = DateTime.Now;
        //        if (no.Minute < targetMinute)
        //        {

        //        }
        //        else if (no.Minute == targetMinute)
        //        {
        //            if (no.Second <= targetSecond)
        //            {
        //                PagePrice pp = detectTimeOfPhase02(targetPrice);
        //                if (pp.status == 0)
        //                {
        //                    this.MockPhase022(targetPrice);
        //                }
        //            }
        //            else if (no.Second > targetSecond)
        //            {
        //                // 直接提交
        //                this.MockPhase022(targetPrice);
        //            } 
        //            else
        //            {
        //                break;
        //            }
                    
        //        } else
        //        {
        //            logger.InfoFormat("minute of time over");
        //            break;
        //        }
        //        KK.Sleep(100);
        //    } while (ret < 0);
        //}

        //public void afterDetectPriceAndTime(int targetPrice, int targetMinute, int targetSecond)
        //{
        //    if (pagePrices.Count == 0)
        //    {
        //        return;
        //    }

        //    PagePrice last = pagePrices.LastOrDefault();
        //    if (last == null)
        //    {
        //        return;
        //    }

        //    if (targetPrice == last.basePrice)
        //    {
        //        this.MockPhase022(targetPrice);
        //    }

        //}

        public void ResetDictIndex()
        {
            robot.UseDict(0);
        }

        public PageTimePriceResult detectPriceAndTimeInScreen(PageTimePriceResult LastResult)
        {
            long t1 = KK.currentTs();
            string uuid = KK.uuid();
            // PagePrice pp = new PagePrice();

            if (this.bench == null)
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
            logger.InfoFormat("目前时间 - OCR内容 {0}, {1}, {2}, {3}. elapsed {4}ms, {5}, {6}", x1, y1, x2, y2, KK.currentTs() - s1, ret1, uuid);

            if (ret1 == null || ret1.Length == 0)
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

            logger.InfoFormat("价格区间 - OCR内容 elapsed {0}ms, {1}, {2}.", KK.currentTs() - s1, ret2, uuid);

            if (ret2 == null || ret2.Length == 0)
            {
                return PageTimePriceResult.ErrorPrice();
            }

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
                return PageTimePriceResult.ErrorPrice();
            }

            int mod2 = numberStr.Length / 2;           

            string[] arr2 = new string[2] { numberStr.Substring(0, mod2), numberStr.Substring(mod2, mod2) };

            int priceLow = int.Parse(arr2[0]);
            int priceHigh = int.Parse(arr2[1]);

            if (priceHigh < 70000 || priceHigh < 70000)
            {
                logger.WarnFormat("识别到 错误的 价格 - {0}, {1}.", priceLow, priceHigh);
                return PageTimePriceResult.ErrorPrice();
            }

            logger.InfoFormat("price parsed priceLow is {0}, pricHigh is {1}", priceLow, priceHigh);

            int currentPrice = (priceLow + priceHigh) / 2;

            var pp = new PagePrice(dt, currentPrice);
            pp.low = priceLow;
            pp.high = priceHigh;

            //timePriceMap[dt] = pp;
            
            //if (pagePrices.Contains(pp))
            //{
            //    pagePrices.Remove(pp);
            //}
            //pagePrices.Add(pp);

            return PageTimePriceResult.Ok(pp);
        }

        public void findAndSetCoordOfCurrentTime()
        {
            robot.UseDict(DictIndex.INDEX_CURRENT_TIME);
            var p = robot.searchTextCoordXYInFlashScreen(bench.x + 20, bench.y + 365, 370, 190, "0066cc-101010", "目前时间");
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
            var p = robot.searchTextCoordXYInFlashScreen(bench.x + 20, bench.y + 365, 371, 190, "0066cc-101010", "价格区间");
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

        private static List<SimplePoint> FenceEndPoints = new List<SimplePoint>();

        private static List<SimplePoint> FenceEndPointsReverse = new List<SimplePoint>();

        public void setBenchPoint()
        {
            bench = this.detectBenchPoint();
            // 设置 验证码区域的 确认/取消 按钮 的 栅栏
            // 440 438
            // 按钮长宽 114 x 27
            // 栅栏 起始点 delta(440, 448) 长宽 422 x 87
            // 假设 每一个cell 长宽是  55 x 21
            var startPoint = bench.AddDelta(440, 448);
            int areaLen = 422, areaWidth = 87;
            int columns = 8;
            int rows = 4;

            logger.InfoFormat("fence startPoint is {2}.", startPoint.ToString());

            // int cellLen = areaLen / columns, cellWidth = areaWidth / rows;
            int cellLen = 53, cellWidth = 22;

            for (int c = 1; c <= columns; c++) 
            {
                for (int r = 1; r <= rows; r++)
                {
                    var fence = startPoint.AddDelta(c * cellLen, r * cellWidth);
                    logger.InfoFormat("fence is row={0} column={1}, point is {2}.", r, c, fence.ToString());
                    FenceEndPoints.Add(fence);
                }
            }

            FenceEndPointsReverse = new List<SimplePoint>(FenceEndPoints);
            FenceEndPointsReverse.Reverse();

            logger.InfoFormat("init FenceEndPoints, size {0}", FenceEndPoints.Count);

        }

        public async void MockLoginAndPhase1()
        {

            this.setBenchPoint();


            Task.Factory.StartNew(() => {


                // 首屏 确定 按钮
                var p11 = bench.AddDelta(741, 507);
                // Task task1 = 
                this.clickConfirmAtIndex(p11);
                // Task.WaitAll(task1);

                // 首屏 同意 按钮
                var p12 = bench.AddDelta(730, 509);
                // Task task2 = 
                this.clickAgreeAtIndex(p11);
                // Task.WaitAll(task2);

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
            });
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

        public void MockPhase2AtCaptcha(int bidPrice)
        {
            logger.InfoFormat("第二阶段 使用基准点 {0}", bench);

            // 第二阶段
            var p41 = bench.AddDelta(676, 417);
            var p42 = bench.AddDelta(800, 415);

            this.inputPriceAtPhase2(p41, bidPrice);
            this.clickBidButtonAtPhase2(p42);
            
        }

        public void MockCancelPhase2AtCaptcha()
        {
            logger.InfoFormat("第二阶段 使用基准点 {0}", bench);

            // 第二阶段 742 502
            var p42 = bench.AddDelta(742, 502);

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

            // 上传验证码 

            this.inputCaptchAtPhase2(p43, "0282");
            this.clickConfirmCaptchaAtPhase2(p44);

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            KK.Sleep(1500);
            var p36 = bench.AddDelta(661, 478);
            this.clickConfirmBidOkAtPhase2(p36);

            // TODO: 确定按钮的位置 可能 会变化, 则检测 


        }


        public void MockPhase022(int targetPrice, BiddingContext context)
        {
            logger.InfoFormat("第二阶段修改 - 使用基准点 {0}", bench);

            // 第二阶段
            var p41 = bench.AddDelta(676, 417);
            var p42 = bench.AddDelta(800, 415);

            this.inputPriceAtPhase2(p41, targetPrice);
            KK.Sleep(2);
            this.clickBidButtonAtPhase2(p42);
            KK.Sleep(2);

            var p43 = bench.AddDelta(734, 416);
            var p44 = bench.AddDelta(553, 500);

            // TODO: 检测 enter 键

            // 对验证码区域截屏且上传 
            KK.Sleep(100);
            CaptchaAnswerImage img = CaptureCaptchaAndUploadTask();
            context.PutAwaitImage(img);

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

            logger.InfoFormat("get answer#{0} for task#{}", answer, img.Uuid);
            this.inputCaptchAtPhase2(p43, answer);
            this.clickConfirmCaptchaAtPhase2(p44);

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            // TODO: 应该检测 区域 是否有 出价有效
            KK.Sleep(1000);
            var p36 = bench.AddDelta(661, 478);
            this.clickConfirmBidOkAtPhase2(p36);


        }

        public void UploadPhase2CaptchaImage(CaptchaAnswerImage img)
        {

            // 
            string url = conf.CaptchaAddressPrefix + "/v1/biding/captcha-task";
            CaptchaImageUploadRequest req = new CaptchaImageUploadRequest();
            req.token = "devJustTest";
            req.uid = img.Uuid;
            req.timestamp = KK.currentTs();
            req.from = "test";


            int httpStatus;
            DataResult<CaptchaImageUploadResponse> dr = RestClient.PostWithFiles<DataResult<CaptchaImageUploadResponse>>(url, req, new List<string> { img.ImagePath1, img.ImagePath2 }, out httpStatus);

            logger.InfoFormat("upload catpcha task, result is {0}", Jsons.ToJson(dr));
        
             
        }

        public CaptchaAnswerImage CaptureCaptchaAndUploadTask()
        {   
            CaptchaAnswerImage img = CapturePhase2CaptchaImage();
            UploadPhase2CaptchaImage(img);

            return img;
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
            //return 1;

            //return /*null*/;
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
            //return Task.Factory.StartNew(() => {
                
            //    return 1;
            //});
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
                robot.LeftClick();
                logger.InfoFormat("第二阶段 尝试点击 - 出价 按钮 - {0}, {1}. {2}", p13.x, p13.y, KK.currentTs() - t1);
            }
        }

        // 第二阶段页 出价 按钮 
        private void clickCancelButtonAtPhase2(SimplePoint p13)
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


            // TODO: 确定按钮的位置 可能 会变化, 则检测 
            foreach (var p in FenceEndPoints)
            {
                robot.MoveTo(p.x, p.y);
                robot.LeftClick();
                // KK.Sleep(50);
            }

            // int ret = robot.MoveTo(p12.x, p12.y);
            // robot.LeftClick();
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

        /// <summary>
        /// 第二阶段 - 截图验证码区域 且 上传验证码图片
        /// </summary>
        /// <returns></returns>
        public CaptchaAnswerImage CapturePhase2CaptchaImage()
        {
            if (this.bench == null)
            {
                setBenchPoint();
            }

            DateTime dt = DateTime.Now;
            var uuid = KK.uuid();
            CaptchaAnswerImage img = new CaptchaAnswerImage();
            img.Uuid = uuid;
            img.CaptureTime = dt;

            // 442 338 ， 380 53
            int x11 = bench.x + 442, y11 = bench.y + 338;
            int x21 = x11 + 380, y21 = y11 + 53;
            var img01Path = getImageDirPath() + "" + uuid + "-" + dt.ToString("HHmmss") + "-phase02-01.jpg";
            int ret1 = robot.CaptureJpg(x11, y11, x21, y21, img01Path, 80);
            img.ImagePath1 = img01Path;

            // 基准点偏移 445 390, 240 85
            int x1 = bench.x + 445, y1 = bench.y + 390;
            int x2 = x1 + 230, y2 = y1 + 90;
            var img02Path = getImageDirPath() + "" + uuid + "-" + dt.ToString("HHmmss") + "-phase02-02.jpg";
            int ret2 = robot.CaptureJpg(x1, y1, x2, y2, img02Path, 80);

            img.ImagePath2 = img02Path;

            return img;
        }

        public string getImageDirPath()
        {
            // d:\work\bid\radarbid\radarbidclient\radarbidclient\bin\x86\debug\resource\dlls\
            string path = robot.GetBasePath();
            int idx = path.LastIndexOf("\\resource\\");
            return path.Substring(0, idx) + "\\Captures\\";
        }

    }
}

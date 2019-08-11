using log4net;
using Radar.Bidding.Model;
using Radar.Bidding.Model.Dto;
using Radar.Common;
using Radar.Common.Model;
using Radar.IoC;
using Radar.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Radar.Bidding
{
    [Component]
    public class BidActionManager : InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BidActionManager));

        private WindowSimulator robot;

        private ProjectConfig conf;

        private CoordPoint _datum;

        // IE进程句柄
        private int ieHwdn;

        private string processClassName = "IEFrame";

        private string processTitle = "客车";

        private bool debugModeOpen = false;

        /// <summary>
        /// 坐标 of 目前时间
        /// </summary>
        private CoordPoint coordOfCurrentTime;

        /// <summary>
        /// 坐标 of 价格区间
        /// </summary>
        private CoordPoint coordOfPriceSection;


        private List<CoordPoint> fenceEndPoints = new List<CoordPoint>();

        private List<CoordPoint> fenceEndPointsReverse = new List<CoordPoint>();


        private CoordPoint Datum
        {
            get
            {
                if (_datum != null)
                {
                    return _datum;
                }

                this.PrepareDatumPoint();

                return _datum;
            }

        }


        public BidActionManager(WindowSimulator robot, ProjectConfig conf)
        {
            this.robot = robot;
            this.conf = conf;
        }

        /// <summary>
        /// 移动鼠标指针
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int MoveCursor(CoordPoint p)
        {
            CoordPoint po = this.Datum.AddDelta(p.x, p.y);

            return robot.MoveTo(po.x, po.y);
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

        public void UseDict(DictIndex dictIdx)
        {
            robot.UseDict(dictIdx);
        }

        public PageTimePriceResult DetectPriceAndTimeInScreen(PageTimePriceResult LastResult)
        {
            long t1 = KK.CurrentMills();
            string uuid = KK.uuid();

            if (this.coordOfCurrentTime == null || coordOfCurrentTime.x <= 0 || coordOfCurrentTime.y <= 0)
            {
                return PageTimePriceResult.ErrorCoordTime();
            }

            var p = this.coordOfCurrentTime;

            // 11:29:57
            int x1 = p.x + 20, y1 = p.y, x2 = p.x + 20 + 150, y2 = p.y + 18;

            long s1 = KK.CurrentMills();
            robot.UseDict(DictIndex.INDEX_NUMBER);
            string ret1 = robot.Ocr(x1, y1, x2, y2, "ff0000-000000", 0.8);
            // string ret1 = robot.Ocr(x1, y1, x2, y2, "0066cc-101010", 0.8);
            logger.DebugFormat("目前时间 - OCR内容 {0}, {1}, {2}, {3}. elapsed {4}ms, {5}, {6}", x1, y1, x2, y2, KK.CurrentMills() - s1, ret1, uuid);


            if (ret1 == null || ret1.Length == 0 || ret1.Length < 6)
            {
                return PageTimePriceResult.ErrorTime();
            }

            DateTime no = DateTime.Now;

            string td = KK.ExtractDigits(ret1);

            if (td == null || td.Length != 6)
            {
                return PageTimePriceResult.ErrorTime();
            }

            logger.DebugFormat("Parsed time is {0}.", td);

            DateTime dt = new DateTime(no.Year, no.Month, no.Day, int.Parse(td.Substring(0, 2)), int.Parse(td.Substring(2, 2)), int.Parse(td.Substring(4, 2)));
            
            // 检测是否已经拿到过该秒的数据，则可以忽略不检测价格了
            if (LastResult?.data != null && dt == LastResult.data.pageTime)
            {
                return PageTimePriceResult.RepeatedTime();
            }

            // 找到坐标 of 价格区间
            if (this.coordOfPriceSection == null || coordOfPriceSection.x <= 0 || coordOfPriceSection.y <= 0)
            {
                return PageTimePriceResult.ErrorCoordPrice();
            }

            var p2 = this.coordOfPriceSection;
            logger.DebugFormat("价格区间 - 坐标是 - {0}. {1}", p2.ToString(), KK.CurrentMills() - t1);

            // 11:29:57
            int x21 = p2.x + 20, y21 = p2.y, x22 = p2.x + 20 + 250, y22 = p2.y + 18;

            // ff0000-101010 
            s1 = KK.CurrentMills();
            robot.UseDict(DictIndex.INDEX_NUMBER);
            string ret2 = robot.Ocr(x21, y21, x22, y22, "ff0000-000000", 0.8);

            if (dt.Minute > 26)
            {
                logger.InfoFormat("价格区间 - OCR内容, {0} @ {1}, elapsed {2}ms", ret2, dt, KK.CurrentMills() - s1);
            } else
            {
                logger.DebugFormat("价格区间 - OCR内容, {0} @ {1}, elapsed {2}ms", ret2, dt, KK.CurrentMills() - s1);
            }
            

            if (ret2 == null || ret2.Length == 0 || ret2.Length < 10)
            {
                return PageTimePriceResult.ErrorPrice();
            }

            string numberStr = KK.ExtractDigits(ret2);
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

            int basePrice = (priceLow + priceHigh) / 2;

            if (dt.Minute > 25)
            {
                logger.InfoFormat("parsed price - base#{0}, low#{1}, high#{2}", basePrice, priceLow, priceHigh);
            } 
            else
            {
                logger.DebugFormat("parsed price - base#{0}, low#{1}, high#{2}", basePrice, priceLow, priceHigh);
            }
            
            var pp = new PagePrice(dt, basePrice);
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

        public void FindAndSetCoordOfCurrentTime()
        {
            robot.UseDict(DictIndex.INDEX_CURRENT_TIME);
            var p = robot.SearchTextCoordXYInFlashScreen(Datum.x + 20, Datum.y + 365, 370, 190, "0066cc-101010", "目前时间");
            if (p != null && p.x > 0 && p.y > 0)
            {
                logger.DebugFormat("find coord of current-time is {0}", p.ToString());
                this.coordOfCurrentTime = p;
            }
        }

        public void FindAndSetCoordOfPriceSection()
        {
            // 找到坐标 of 价格区间
            robot.UseDict(DictIndex.INDEX_PRICE_SECTION);
            var p = robot.SearchTextCoordXYInFlashScreen(Datum.x + 20, Datum.y + 365, 371, 190, "0066cc-101010", "价格区间");
            if (p != null && p.x > 0 && p.y > 0)
            {
                logger.DebugFormat("find coord of price-range is {0}", p.ToString());

                this.coordOfPriceSection = p;
            }
        }

        public string FindTextByOcr(CoordRectangle rect, string color, DictIndex dictIndex = DictIndex.INDEX_ALL)
        {
            
            robot.UseDict(dictIndex);

            string ret = robot.Ocr(rect.x1, rect.y1, rect.x2, rect.y2, color, 0.8);
            return ret;
        }

        /// <summary>
        /// 返回基于 基准点 的坐标位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CoordPoint DeltaPoint(int x, int y)
        {
            return this.Datum.AddDelta(x, y);
        }

        /// <summary>
        /// 返回基于 基准点 的坐标位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CoordPoint AddDelta(int x, int y)
        {
            return this.Datum.AddDelta(x, y);
        }

        /// <summary>
        /// 返回基于 基准点 的坐标位置
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public CoordPoint DeltaPoint(CoordPoint p)
        {
            return this.Datum.AddDelta(p.x, p.y);
        }

        public void MockLogin()
        {
            Task.Factory.StartNew(() =>
            {
                

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

        public void DismissCurtain()
        {
            // 首屏 确定 按钮
            // var p11 = Datum.AddDelta(741, 507);
            var p11 = new CoordPoint(741, 507);

            // 首屏 同意 按钮
            // var p12 = Datum.AddDelta(730, 509);
            var p12 = new CoordPoint(730, 509);

            this.ClickBtnOnceAtPoint(p11, "首屏确定按钮");
            this.ClickBtnOnceAtPoint(p12, "首屏同意按钮");
        }

        public void MockLoginAndPhase1()
        {

            Task.Factory.StartNew(() => {


                // 首屏 确定 按钮
                var p11 = Datum.AddDelta(741, 507);
                // Task task1 = 
                this.ClickConfirmAtIndex(p11);
                // Task.WaitAll(task1);

                // 首屏 同意 按钮
                var p12 = Datum.AddDelta(730, 509);
                // Task task2 = 
                this.ClickAgreeAtIndex(p11);
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

            this.InputTextAtPoint(p41, 89000.ToString(), true, "模拟第二阶段出价");
            this.ClickButtonAtPoint(p42, true, "模拟第二阶段出价");

            var p43 = Datum.AddDelta(734, 416);
            var p44 = Datum.AddDelta(553, 500);

            this.InputTextAtPoint(p43, "0282", true, "phase2 submit验证码");
            this.ClickButtonByFenceWayLToR(p44);

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            Thread.Sleep(2 * 1000);
            var p36 = Datum.AddDelta(661, 478);
            this.ClickButtonAtPoint(p36, false, null);
        }

        private CoordPoint GetScreenResolution()
        {
            int screenWidth = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth);
            int screenHeight = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight);

            return new CoordPoint(screenWidth, screenHeight);
        }

        private void PrepareDatumPoint()
        {
            DetectAndSetDatum();
        }

        private void DetectAndSetDatum()
        {
            long t1 = KK.CurrentMills();
            // 尝试使用 相对位置 - 上海市个人非营业性客车额度投标拍卖
            // 个人非营业性客车
            // 您使用的是
            // var checkPoint = this.searchTextCoordXYInScreen("2e6e9e-2e6e9e", "用的是");

            var checkPoint = robot.searchTextCoordXYInScreen("0074bf-101010|9c4800-101010|ffdf9c-101010|df9c48-101010|489cdf-101010|000000-101010|9cdfff-101010|00489c-101010", "用的是");
            // 
            logger.InfoFormat("检查点坐标是 - {0}. {1}", checkPoint.ToString(), KK.CurrentMills() - t1);

            // 204, 287
            var bench = checkPoint.AddDelta(-206, -286);
            // 719, 364 - = 513，78 ， (510, 78)
            logger.InfoFormat("基准点坐标是 - {0}", bench.ToString());

            // 900 x 700
            // int w = 900;
            // int h = 700;

            if (bench.x <= 0 || bench.y <= 0)
            {
                var re = GetScreenResolution();
                logger.InfoFormat("降级使用 - 屏幕分辨率 - {0}", re.ToString());

                //bench.y = 79;
                if (re.x > 1900)
                {
                    // 不会有 scroll
                    // bench.x = (re.x - w) / 2;
                    bench.x = 672;
                    bench.y = 23;
                }
                else
                {
                    // bench.x = (re.x - w - 15) / 2;
                    bench.x = 344;
                    bench.y = 19;
                }

                // 内嵌模式
                // 682 x 23
                // bench.x = 682;
                // bench.y = 23;

                logger.InfoFormat("降级使用 - 新的基准点坐标是 - {0}", bench.ToString());
            }

            _datum = bench;

        }

        // 
        private void ClickConfirmAtIndex(CoordPoint p1)
        {
            long t1 = KK.CurrentMills();
            if (p1.x > 0 && p1.y > 0)
            {
                logger.InfoFormat("找到 - 确认按钮 - {0}, {1}", p1.x, p1.y);
                robot.MoveTo(p1.x, p1.y);
                robot.LeftClick();
                logger.InfoFormat("点击了 - 确认按钮 - {0}, {1}, {2}", p1.x, p1.y, KK.CurrentMills() - t1);
            }
            //return 1;

            //return /*null*/;
        }

        // 
        private void ClickAgreeAtIndex(CoordPoint p2)
        {
            long t1 = KK.CurrentMills();
            if (p2.x > 0 && p2.y > 0)
            {
                logger.InfoFormat("找到 - 我同意拍卖须知按钮 - {0}, {1}", p2.x, p2.y);
                robot.MoveTo(p2.x, p2.y);
                robot.LeftClick();
                robot.LeftClick();
                logger.InfoFormat("点击了 - 我同意拍卖须知按钮 - {0}, {1}, {2}", p2.x, p2.y, KK.CurrentMills() - t1);
            }
            //return Task.Factory.StartNew(() => {
                
            //    return 1;
            //});
        }

        // 
        private void inputBidNumberAtLogin(CoordPoint p3, string bidNumber)
        {
            long t1 = KK.CurrentMills();
            if (p3.x > 0 && p3.y > 0)
            {
                logger.InfoFormat("找到 - 投标号输入框 - {0}, {1}", p3.x, p3.y);
                robot.MoveTo(p3.x, p3.y);
                robot.LeftClick();
                robot.KeyPressString(bidNumber);
                logger.InfoFormat("输入了 - 投标号输入框 - {0}, {1}, {2}", p3.x, p3.y, KK.CurrentMills() - t1);
            }
        }

        // 
        private void inputPasswordAtLogin(CoordPoint p4, string password)
        {
            long t1 = KK.CurrentMills();
            if (p4.x > 0 && p4.y > 0)
            {
                logger.InfoFormat("找到 - 密码输入框 - {0}, {1}", p4.x, p4.y);
                robot.MoveTo(p4.x, p4.y);
                robot.LeftClick();
                robot.KeyPressString(password);
                logger.InfoFormat("输入了 - 密码输入框 - {0}, {1}, {2}", p4.x, p4.y, KK.CurrentMills() - t1);
            }
        }

        // 
        private void inputCaptchaAtLogin(CoordPoint p5, string captcha)
        {
            long t1 = KK.CurrentMills();
            if (p5.x > 0 && p5.y > 0)
            {
                logger.InfoFormat("找到 - 图像校验码 输入框 - {0}, {1}", p5.x, p5.y);
                robot.MoveTo(p5.x, p5.y);
                robot.LeftClick();
                robot.KeyPressString(captcha);
                logger.InfoFormat("输入了 - 图像校验码 输入框 - {0}, {1}, {2}", p5.x, p5.y, KK.CurrentMills() - t1);
            }
        }

        // 
        private void clickLoginAtLogin(CoordPoint p6)
        {
            long t1 = KK.CurrentMills();
            if (p6.x > 0 && p6.y > 0)
            {
                logger.InfoFormat("找到 - 参加投标竞买 按钮 - {0}, {1}", p6.x, p6.y);
                robot.MoveTo(p6.x + 10, p6.y + 2);
                robot.LeftClick();
                logger.InfoFormat("点击了 - 参加投标竞买 按钮 - {0}, {1}, {2}", p6.x, p6.y, KK.CurrentMills() - t1);
            }
        }

        // 
        private void inputPriceAtPhase1(CoordPoint p11, int price)
        {
            long t1 = KK.CurrentMills();
            if (p11.x > 0 && p11.y > 0)
            {
                logger.InfoFormat("找到 - 输入价格 输入框 - {0}, {1}", p11.x, p11.y);
                robot.MoveTo(p11.x, p11.y);
                robot.LeftClick();
                robot.KeyPressString(price.ToString());
                logger.InfoFormat("第一阶段 输入 - 输入价格 输入框 - {0}, {1}, {2}", p11.x, p11.y, KK.CurrentMills() - t1);
            }
        }

        // 
        private void inputPrice2AtPhase1(CoordPoint p12, int price)
        {
            long t1 = KK.CurrentMills();
            if (p12.x > 0 && p12.y > 0)
            {
                logger.InfoFormat("找到 - 再次输入价格 输入框 - {0}, {1}", p12.x, p12.y);
                robot.MoveTo(p12.x, p12.y);
                robot.LeftClick();
                robot.KeyPressString(price.ToString());
                logger.InfoFormat("第一阶段 输入 - 再次输入价格 输入框 - {0}, {1}, {2}", p12.x, p12.y, KK.CurrentMills() - t1);
            }
        }

        // 
        private void clickBidButtonAtPhase1(CoordPoint p12)
        {
            long t1 = KK.CurrentMills();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 出价 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.CurrentMills() - t1);
        }

        // 
        private void inputCaptchAtPhase1(CoordPoint p13, string captcha)
        {
            long t1 = KK.CurrentMills();
            if (p13.x > 0 && p13.y > 0)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
                robot.KeyPressString(captcha);
                logger.InfoFormat("第一阶段 尝试输入 - 验证码 输入框 - {0}, {1}, {2}", p13.x, p13.y, KK.CurrentMills() - t1);
            }
        }

        // 
        private void clickConfirmCaptchaAtPhase1(CoordPoint p12)
        {
            long t1 = KK.CurrentMills();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 验证码 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.CurrentMills() - t1);
        }

        // 
        private void clickConfirmBidOkAtPhase1(CoordPoint p12)
        {
            long t1 = KK.CurrentMills();
            int ret = robot.MoveTo(p12.x, p12.y);
            robot.LeftClick();
            logger.InfoFormat("第一阶段 尝试点击 - 出价结果 确认 按钮 - {0}, {1}, {2}", p12.x, p12.y, KK.CurrentMills() - t1);
        }

        public void CleanTextAtPoint(CoordPoint p13, int textLength, bool needMoveTo)
        {
            this.CleanTextAtPoint(p13, textLength, needMoveTo, null);
        }

        public void CleanTextAtPoint(CoordPoint p13, int textLength, bool needMoveTo, string memo)
        {
            long s1 = KK.CurrentMills();
            if (needMoveTo)
            {
                robot.MoveTo(p13.x, p13.y);
                robot.LeftClick();
            }

            for (int i = 0; i < textLength; i++)
            {
                robot.PressBackspacKey();
                robot.PressDeleteKey();
            }

            if (memo?.Length > 0)
            {
                logger.InfoFormat("{0}#删除文本: {1} @ {2}, elapsed {3}ms", memo, textLength, p13, KK.CurrentMills() - s1);
            }
        }

        public void ClickButtonAtPoint(CoordPoint p13, bool needMoreOnceClick, string memo)
        {
            long t1 = KK.CurrentMills();
            robot.MoveTo(p13.x, p13.y);
            robot.LeftClick();
            if (needMoreOnceClick)
            {
                robot.LeftClick();
            }

            logger.InfoFormat("{0}#点击按钮 @ {1} with more#{2}, elapsed {3}.", memo, p13.ToString(), needMoreOnceClick, KK.CurrentMills() - t1);
        }

        public void InputTextAtPoint(CoordPoint p13, string text, bool needClearFirst, string memo)
        {
            long t1 = KK.CurrentMills();
            robot.MoveTo(p13.x, p13.y);
            robot.LeftClick();

            // 先清空已输入的验证码
            if (needClearFirst)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    robot.PressBackspacKey();
                    robot.PressDeleteKey();
                }
            }

            robot.KeyPressString(text);
            logger.InfoFormat("{0}#输入: {1} @ {2}, Elapsed {3}.", memo, text, p13.ToString(), KK.CurrentMills() - t1);
        }

        // 
        public void ClickButtonByFenceWayLToR(CoordPoint pot)
        {
            long t1 = KK.CurrentMills();

            // 按钮的位置 可能 会变化, 这里使用 栅栏模式多次点击
            foreach (var p in fenceEndPoints)
            {
                robot.MoveTo(p.x, p.y);
                robot.LeftClick();
            }

            logger.InfoFormat("栅栏模式（从右到左） 点击 - 按钮 - {0}, elpased {1}.", pot.ToString(), KK.CurrentMills() - t1);
        }

        public void ClickButtonByFenceWayRToL(CoordPoint pot)
        {
            long t1 = KK.CurrentMills();

            // 按钮的位置 可能 会变化, 这里使用 栅栏模式多次点击
            foreach (var p in fenceEndPointsReverse)
            {
                robot.MoveTo(p.x, p.y);
                robot.LeftClick();
            }

            logger.InfoFormat("栅栏模式（从左到右） 点击 - 按钮 - {0}, elpased {1}.", pot.ToString(), KK.CurrentMills() - t1);
        }

        /// <summary>
        /// 如果 isAbsolute  != true,  在相对坐标(x, y)上点击一次 
        /// 否则 在绝对坐标(x, y)上点击一次 
        /// </summary>
        /// <param name="p12"></param>
        /// <param name="isAbsolute"></param>
        public void ClickBtnOnceAtPoint(CoordPoint p12, string memo = "", bool isAbsolute = false, int delayMills = 0, int clickCount = 1)
        {
            if (delayMills > 0)
            {
                KK.Sleep(delayMills);
            }

            long t1 = KK.CurrentMills();
            CoordPoint po = p12;
            if (!isAbsolute)
            {
                po = Datum.AddDelta(p12.x, p12.y);
            }

            int ret = robot.MoveTo(po.x, po.y);
            robot.LeftClick();
            if (clickCount > 1)
            {
                robot.LeftClick();
            }
            
            logger.InfoFormat("点击按钮#{0} @ {1} with count#{2}, elapsed {3}.", memo, po.ToString(), clickCount, KK.CurrentMills() - t1);
        }

        public int CaptureImage(CoordRectangle rect, string filePath)
        {
            return robot.CaptureJpg(rect.x1, rect.y1, rect.x2, rect.y2, filePath, 90);
        }

        public string CaptureFlashScreen()
        {

            CoordRectangle rect2 = CoordRectangle.From(Datum, 900, 700);
            var img02Path = string.Format("{0}\\{1}-{2:HHmmss}-flash-screen.jpg", KK.FlashScreenDir(), KK.uuid(), DateTime.Now);
            CaptureImage(rect2, img02Path);

            return img02Path;
        }

        public string CaptureFullScreen()
        {
            var scr = GetScreenResolution();
            CoordRectangle rect2 = CoordRectangle.From(0, 0, scr.x, scr.y);
            var img02Path = string.Format("{0}\\{1}-{2:HHmmss}-full-screen.jpg", KK.FlashScreenDir(), KK.uuid(), DateTime.Now);
            CaptureImage(rect2, img02Path);

            return img02Path;
        }

        private void InitFencePoint()
        {
            // 设置 验证码区域的 确认/取消 按钮 的 栅栏
            // 440 438
            // 按钮长宽 114 x 27
            // 栅栏 起始点 delta(440, 448) 长宽 422 x 87
            // 假设 每一个cell 长宽是  55 x 21
            var startPoint = this.Datum.AddDelta(440, 448);
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
                    logger.DebugFormat("fence is row={0} column={1}, point is {2}.", r, c, fence.ToString());
                    fenceEndPoints.Add(fence);
                }
            }

            fenceEndPointsReverse = new List<CoordPoint>(fenceEndPoints);
            fenceEndPointsReverse.Reverse();

            logger.DebugFormat("init FenceEndPoints, size {0}", fenceEndPoints.Count);
        }


        public void AfterPropertiesSet()
        {
            InitFencePoint();
        }

        public ScreenImageUploadResponse UploadFileToSaber(string filePath, int uploadType = 0)
        {
            string url = conf.UploadRobotScreenUrl;
            CaptchaImageUploadRequest req = new CaptchaImageUploadRequest();
            req.machineCode = robot.GetMachineCode();
            req.token = "devJustTest";
            req.uid = KK.GetFileNameNoSuffix(filePath);
            req.timestamp = KK.CurrentMills();
            req.from = "test";
            req.uploadType = uploadType;

            int httpStatus;
            DataResult<ScreenImageUploadResponse> dr = HttpClients
                .PostWithFiles<DataResult<ScreenImageUploadResponse>>(url, req, new List<string> { filePath }, out httpStatus);

            logger.InfoFormat("upload file#{0}, result is {1}", req.uid, Jsons.ToJson(dr));

            return DataResults.IsOK(dr) ? dr.Data : null;
        }

    }
}

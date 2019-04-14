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


        private string bidingWebsiteAddress = "http://119.3.64.205:8888/login.htm";

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

                bench.y = 79;
                if (re.y > h + 150)
                {
                    // 不会有 scroll
                    bench.x = (re.x - w) / 2;
                }
                else
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

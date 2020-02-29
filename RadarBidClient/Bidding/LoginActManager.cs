using log4net;
using Radar.Bidding.Model;
using Radar.Common;
using Radar.Common.Threads;
using Radar.IoC;
using Radar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar.Bidding
{
    [Component]
    public class LoginActManager
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(Phase2ActManager));

        private ProjectConfig conf;

        private BidActionManager actionManager;

        private bool isIdCardShow;


        public LoginActManager(ProjectConfig conf, BidActionManager actionManager)
        {
            this.conf = conf;
            this.actionManager = actionManager;
        }

        public void BeforeLogin()
        {
            actionManager.DismissCurtain();
        }

        public Task LoginBidAccount(string bidNo, string password, string idCardNo, bool clickLoginButton)
        {
            // ThreadUtils
            Task ta = ThreadUtils.StartNewTaskSafe(() =>
            {
                this._LoginBidAccount(bidNo, password, idCardNo, clickLoginButton);
            });
            return ta;
        }

        private void _LoginBidAccount(string bidNo, string password, string idCardNo, bool clickLoginButton)
        {
            // 登录页
            var p21 = actionManager.DeltaPoint(610, 168);
            var p22 = actionManager.DeltaPoint(610, 218);
            var p23 = actionManager.DeltaPoint(610, 264);
            
            actionManager.InputTextAtPoint(p21, bidNo, true, "投标号");
            KK.Sleep(KK.RandomInt(100, 500));

            actionManager.InputTextAtPoint(p22, password, true, "密码");
            KK.Sleep(KK.RandomInt(100, 500));

            // 可能需要输入身份证 - 身份证
            isIdCardShow = IsIdCardNeeded();
            if (isIdCardShow)
            {
                actionManager.InputTextAtPoint(p23, idCardNo, true, "身份证");
            } else
            {
                var p231 = actionManager.DeltaPoint(610, 256);
                actionManager.InputTextAtPoint(p231, idCardNo, true, "身份证");
            }

            KK.Sleep(KK.RandomInt(100, 500));

            //this.inputCaptchaAtLogin(p23, "301726");
            //this.clickLoginAtLogin(p24);
            if (clickLoginButton)
            {
                ClickLoginButton();
            }

            logger.InfoFormat("login account#{0} ", bidNo);

        }

        public void ClickLoginButton()
        {
            // 可能有身份证输入框
            var d2 = new CoordPoint(642, 473);
            actionManager.ClickBtnOnceAtPoint(d2, "投标竞拍T2");
            KK.Sleep(200);

            // 没有身份证输入框
            var d1 = new CoordPoint(642, 427);
            actionManager.ClickBtnOnceAtPoint(d1, "投标竞拍T1");
        }


        public bool IsIdCardNeeded()
        {
            actionManager.UseDict(DictIndex.INDEX_ALL);

            var p1 = actionManager.DeltaPoint(408, 243);
            var rect = CoordRectangle.From(p1, 469, 44);
            // var text = actionManager.FindTextByOcr(rect, "777777-777777");
            var text = actionManager.FindTextByOcr(rect, "686868-101010");
            logger.InfoFormat("IsIdCardNeeded ocr 识别内容是 {0}", text);
            return text.Contains("身");
        }


    }
}

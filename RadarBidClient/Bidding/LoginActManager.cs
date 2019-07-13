using log4net;
using Radar.Bidding.Model;
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
            Task ta = Task.Factory.StartNew(() =>
            {
                this._LoginBidAccount(bidNo, password, idCardNo, clickLoginButton);
            });
            return ta;
        }

        private void _LoginBidAccount(string bidNo, string password, string idCardNo, bool clickLoginButton)
        {
            // 登录页
            var Datum = actionManager.Datum;

            var p21 = Datum.AddDelta(610, 168);
            var p22 = Datum.AddDelta(610, 218);
            var p23 = Datum.AddDelta(610, 264);
            
            actionManager.InputTextAtPoint(p21, bidNo, true, "投标号");
            actionManager.InputTextAtPoint(p22, password, true, "密码");

            // TODO: 验证码方式变了

            // 可能需要输入身份证 - 身份证
            bool isIdCardNeeded = IsIdCardNeeded(Datum);
            if (isIdCardNeeded)
            {
                actionManager.InputTextAtPoint(p23, idCardNo, true, "身份证");
            }

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
            var Datum = actionManager.Datum;

            var p24 = Datum.AddDelta(642, 473);
            actionManager.ClickButtonAtPoint(p24, false, "投标竞拍");
        }


        public bool IsIdCardNeeded(CoordPoint Datum)
        {
            var p1 = Datum.AddDelta(408, 243);
            var rect = CoordRectangle.From(p1, 469, 44);
            var text = actionManager.FindTextByOcr(rect, "777777-777777");
            logger.InfoFormat("ocr 识别内容是 {0}", text);
            return text.Contains("身");
        }


    }
}

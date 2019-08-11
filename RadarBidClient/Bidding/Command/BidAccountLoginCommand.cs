using log4net;
using Radar.Bidding.Model;
using Radar.Bidding.Model.Dto;
using Radar.Bidding.Service;
using Radar.Common;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.Common.Threads;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar.Bidding.Command
{
    [Component]
    public class BidAccountLoginCommand : BaseCommand<BidAccountLoginRequest>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BidAccountLoginCommand));

        private ClientService clientService;

        private BidActionManager bidActionManager;

        private LoginActManager loginActManager;

        public BidAccountLoginCommand(ClientService clientService, BidActionManager bidActionManager, LoginActManager loginActManager)
        {
            this.clientService = clientService;
            this.bidActionManager = bidActionManager;
            this.loginActManager = loginActManager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.BID_ACCOUNT_LOGIN;
        }

        protected override JsonCommand DoExecute(BidAccountLoginRequest req)
        {
            logger.InfoFormat("Execute BidAccountLogin: MachineCode is {0}, coords is {1}, ScreenModeVal is {2}", req.MachineCode, req.LoginCoords, req.ScreenModeVal);

            // string bidNo, string password, string idCardNo, bool clickLoginButton
            var coords = req.LoginCoords;
            var arr = coords.Split(';');
            var p1 = CoordPoint.FromAndAdjustRemote(arr[0]);
            var p2 = CoordPoint.FromAndAdjustRemote(arr[1]);
            var p3 = CoordPoint.FromAndAdjustRemote(arr[2]);

            bool isAbsolute = false;
            if (req.ScreenModeVal == 10)
            {
                isAbsolute = true;
            }


            Task ta = loginActManager.LoginBidAccount(req.BidAccountNo, req.BidAccountPswd, req.BidAccountIdCard, false);

            ta.ContinueWith((task) =>
            {
                bidActionManager.ClickBtnOnceAtPoint(p1, "验证码坐标 点1", isAbsolute, KK.RandomInt(100, 500));
                bidActionManager.ClickBtnOnceAtPoint(p2, "验证码坐标 点2", isAbsolute, KK.RandomInt(100, 500));
                bidActionManager.ClickBtnOnceAtPoint(p3, "验证码坐标 点3", isAbsolute, KK.RandomInt(100, 500));

                loginActManager.ClickLoginButton();
            });

            // ta.Wait();

            // ThreadUtils.StartNewTaskSafe();


            return null;
        }
    }
}

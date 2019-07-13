using log4net;
using Radar.Bidding.Model;
using Radar.Bidding.Model.Dto;
using Radar.Bidding.Service;
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
            logger.InfoFormat("Execute BidAccountLogin: MachineCode is {0}, coords is {1}", req.MachineCode, req.LoginCoords);

            // string bidNo, string password, string idCardNo, bool clickLoginButton
            var coords = req.LoginCoords;
            var arr = coords.Split(';');
            var p1 = CoordPoint.From(arr[0]);
            var p2 = CoordPoint.From(arr[1]);
            var p3 = CoordPoint.From(arr[2]);


            Task ta = loginActManager.LoginBidAccount(req.BidAccountNo, req.BidAccountPswd, req.BidAccountIdCard, false);

            ta.Wait();

            ThreadUtils.StartNewBackgroudThread(() =>
            {

                bidActionManager.ClickOnceAtPointRelative(p1.x, p1.y);
                bidActionManager.ClickOnceAtPointRelative(p2.x, p2.y);
                bidActionManager.ClickOnceAtPointRelative(p3.x, p3.y);

                loginActManager.ClickLoginButton();

            });


            return null;
        }
    }
}

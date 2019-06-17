using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Command
{
    [Component]
    public class ClientPriceTellCommand : BaseCommand<string>
    {
        [Component]
        private BiddingScreen biddingScreen;

        public ClientPriceTellCommand(BiddingScreen biddingScreen)
        {
            this.biddingScreen = biddingScreen;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.CAPTURE_BID_SCREEN;
        }

        protected override JsonCommand DoExecute(string args)
        {
            string[] arr = args.Split(',');
            int hour = int.Parse(arr[0]);
            int minute = int.Parse(arr[1]);
            int price = int.Parse(arr[2]);

            return null;
        }

    }

    

}

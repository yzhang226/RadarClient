using log4net;
using Radar.Bidding.Model;
using Radar.Common;
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
    public class PriceTellCommand : BaseCommand<string>
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(PriceTellCommand));

        [Component]
        private BiddingScreen biddingScreen;

        public PriceTellCommand(BiddingScreen biddingScreen)
        {
            this.biddingScreen = biddingScreen;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.PRICE_TELL;
        }

        protected override JsonCommand DoExecute(string args)
        {
            string[] arr = args.Split(',');
            long mills = long.Parse(arr[0]);
            int basePrice = int.Parse(arr[1]);

            DateTime dt = KK.ToDateTime(mills);

            var pr = new PagePrice(dt, basePrice);

            if (biddingScreen.GetBiddingContext().IsPagePriceCalced(pr))
            {
                logger.InfoFormat("price-tell price#{0} already calced", pr);
                return null;
            }

            logger.InfoFormat("price-tell price#{0} still donot be calced, try to do calc rule.", pr);

            biddingScreen.AfterSuccessDetect(pr);

            return null;
        }

    }

    

}

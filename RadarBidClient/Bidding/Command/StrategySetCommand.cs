using log4net;
using Radar.Bidding.Model.Dto;
using Radar.Bidding.Service;
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
    public class StrategySetCommand : BaseCommand<BidStrategiesSetRequest>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Phase1PriceSubmitCommand));

        private ClientService clientService;

        private BidActionManager bidActionManager;

        private BiddingScreen biddingScreen;

        public StrategySetCommand(ClientService clientService, BidActionManager bidActionManager, BiddingScreen biddingScreen)
        {
            this.clientService = clientService;
            this.bidActionManager = bidActionManager;
            this.biddingScreen = biddingScreen;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.BID_STRATEGIES_SET;
        }

        protected override JsonCommand DoExecute(BidStrategiesSetRequest req)
        {
            logger.InfoFormat("Execute StrategySet: MachineCode is {0}, BidStrategies is {1}", req.MachineCode, req.BidStrategies);

            // submitStrategyManager.WriteNewStrategyToFile(req.BidStrategies);

            biddingScreen.RewriteAndResetStrategyFile(req.BidStrategies);

            return null;
        }
    }

}

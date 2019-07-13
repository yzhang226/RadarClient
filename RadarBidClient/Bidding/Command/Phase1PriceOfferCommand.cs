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
    public class Phase1PriceOfferCommand : BaseCommand<Phase1PriceOfferRequest>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Phase1PriceOfferCommand));

        private ClientService clientService;

        private BidActionManager bidActionManager;

        private Phase1Manager phase1Manager;

        public Phase1PriceOfferCommand(ClientService clientService, BidActionManager bidActionManager, Phase1Manager phase1Manager)
        {
            this.clientService = clientService;
            this.bidActionManager = bidActionManager;
            this.phase1Manager = phase1Manager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.PHASE1_PRICE_OFFER;
        }

        protected override JsonCommand DoExecute(Phase1PriceOfferRequest req)
        {
            logger.InfoFormat("Execute Phase1PriceOffer: MachineCode is {0}, Price is {1}", req.MachineCode, req.Price);

            phase1Manager.OfferPrice(req.Price, true);
            
            return null;
        }
    }

}

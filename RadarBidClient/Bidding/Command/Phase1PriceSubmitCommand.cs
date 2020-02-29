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
    public class Phase1PriceSubmitCommand : BaseCommand<Phase1PriceSubmitRequest>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Phase1PriceSubmitCommand));

        private ClientService clientService;

        private BidActionManager bidActionManager;

        private Phase1Manager phase1Manager;

        public Phase1PriceSubmitCommand(ClientService clientService, BidActionManager bidActionManager, Phase1Manager phase1Manager)
        {
            this.clientService = clientService;
            this.bidActionManager = bidActionManager;
            this.phase1Manager = phase1Manager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.PHASE1_PRICE_SUBMIT;
        }

        protected override JsonCommand DoExecute(Phase1PriceSubmitRequest req)
        {
            logger.InfoFormat("Execute Phase1PriceSubmit: MachineCode is {0}, Answer is {1}", req.MachineCode, req.Answer);

            phase1Manager.SubmitOfferedPrice(req.Answer);

            return null;
        }
    }

}

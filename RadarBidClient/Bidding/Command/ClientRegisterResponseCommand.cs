using log4net;
using Radar.Bidding.Model.Dto;
using Radar.Bidding.Service;
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
    public class ClientRegisterResponseCommand : BaseCommand<BidderRegisterResponse>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ClientRegisterResponseCommand));

        private ClientService clientService;

        public ClientRegisterResponseCommand(ClientService clientService)
        {
            this.clientService = clientService;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.RESP_REGISTER_LOGIN;
        }

        protected override JsonCommand DoExecute(BidderRegisterResponse req)
        {
            logger.InfoFormat("Response of ClientLogin: clientNo is {0}", req.clientNo);

            clientService.AssignedClientNo = req.clientNo;

            return null;
        }
    }
}

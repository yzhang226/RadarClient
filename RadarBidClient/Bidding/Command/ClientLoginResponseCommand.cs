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
    public class ClientLoginResponseCommand : BaseCommand<BidderLoginResponse>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ClientLoginResponseCommand));

        private ClientService clientService;

        public ClientLoginResponseCommand(ClientService clientService)
        {
            this.clientService = clientService;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.RESP_CLIENT_LOGIN;
        }

        protected override JsonCommand DoExecute(BidderLoginResponse req)
        {
            logger.InfoFormat("Response of ClientLogin: clientNo is {0}", req.clientNo);

            clientService.AssignedClientNo = req.clientNo;

            return null;
        }
    }
}

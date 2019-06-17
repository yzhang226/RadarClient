using log4net;
using Radar.Bidding.Model.Dto;
using Radar.Bidding.Net;
using Radar.Common;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.Common.Raw;
using Radar.Common.Utils;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Service
{

    [Component]
    public class ClientService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ClientService));

        private int _clientNo;


        private SocketClient socketClient;

        private WindowSimulator simulator;

        public ClientService(SocketClient socketClient, WindowSimulator simulator)
        {
            this.socketClient = socketClient;
            this.simulator = simulator;
        }

        public void DoClientLogin()
        {
            BidderLoginRequest req = new BidderLoginRequest();
            req.machineCode = simulator.GetMachineCode();

            JsonCommand comm = JsonCommands.OK(CommandDirective.CLIENT_LOGIN, req);

            RawMessage msg = MessageUtils.BuildJsonMessage(_clientNo, comm);


            socketClient.Send(msg);

        }

        public int AssignedClientNo
        {
            get
            {
                return _clientNo;
            }
            set
            {
                logger.InfoFormat("Set AssignedClientNo is {0}", value);

                this._clientNo = value;
            }
        }


    }
}

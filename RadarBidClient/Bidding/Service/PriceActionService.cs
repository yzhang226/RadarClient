using log4net;
using Radar.Bidding.Model.Dto;
using Radar.Bidding.Net;
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
    public class PriceActionService
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(PriceActionService));

        private SocketClient socketClient;

        private WindowSimulator simulator;

        private ClientService clientService;


        public PriceActionService(SocketClient socketClient, WindowSimulator simulator, ClientService clientService)
        {
            this.socketClient = socketClient;
            this.simulator = simulator;
            this.clientService = clientService;
        }

        public void ReportPriceOffered(int price, DateTime screenTime, string memo = "")
        {
            // 
            DoPriceAction(PriceAction.PRICE_OFFER, price, screenTime, memo);
        }

        public void ReportPriceSubbmitted(int price, DateTime screenTime, string memo = "")
        {
            // 
            DoPriceAction(PriceAction.PRICE_SUBMIT, price, screenTime, memo);
        }

        public void ReportPriceShowed(int price, DateTime screenTime, string memo = "")
        {
            // 
            DoPriceAction(PriceAction.PRICE_SHOW, price, screenTime, memo);
        }

        private void DoPriceAction(PriceAction action, int price, DateTime screenTime, string memo = "")
        {
            PriceActionRequest req = new PriceActionRequest();
            req.MachineCode = simulator.GetMachineCode();
            req.Action = action;
            req.Price = price;
            req.ScreenTime = screenTime;
            req.OccurTime = DateTime.Now;

            JsonCommand comm = JsonCommands.OK(CommandDirective.CLIENT_PRICE_TELL, req);

            RawMessage msg = MessageUtils.BuildJsonMessage(clientService.AssignedClientNo, comm);

            socketClient.Send(msg);

            logger.InfoFormat("send price-action#{0}#{1} at srceeTime#{2} tcp request...", price, action, screenTime);
        }

    }
}

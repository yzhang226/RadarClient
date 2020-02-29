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

        private ClientService clientService;


        public PriceActionService(SocketClient socketClient, ClientService clientService)
        {
            this.socketClient = socketClient;
            this.clientService = clientService;
        }

        public void ReportPriceOffered(int screenPrice, int targetPrice, DateTime screenTime, DateTime occurTime, string memo = "")
        {
            // 
            DoPriceAction(PriceAction.PRICE_OFFER, screenPrice, targetPrice, screenTime, 0, occurTime, memo);
        }

        public void ReportPriceSubbmitted(int screenPrice, int targetPrice, DateTime screenTime, int usedDelayMills, DateTime occurTime, string memo = "")
        {
            // 
            DoPriceAction(PriceAction.PRICE_SUBMIT, screenPrice, targetPrice, screenTime, usedDelayMills, occurTime, memo);
        }

        public void ReportPriceShowed(int screenPrice, DateTime screenTime, DateTime occurTime, string memo = "")
        {
            // 
            DoPriceAction(PriceAction.PRICE_SHOW, screenPrice, 0, screenTime, 0, occurTime, memo);
        }

        private void DoPriceAction(PriceAction action, int screenPrice, int targetPrice, DateTime screenTime, int usedDelayMills, DateTime occurTime, string memo = "")
        {
            PriceActionRequest req = new PriceActionRequest();
            req.MachineCode = clientService.GetMachineCode();
            req.OccurTime = occurTime == null ? DateTime.Now : occurTime;
            req.ScreenTime = screenTime;
            req.UsedDelayMills = usedDelayMills;
            req.Action = action;
            req.ScreenPrice = screenPrice;
            req.TargetPrice = targetPrice;

            JsonCommand comm = JsonCommands.OK(CommandDirective.PRICE_TELL, req.ToLine());

            RawMessage msg = MessageUtils.BuildJsonMessage(ClientService.AssignedClientNo, comm);

            socketClient.Send(msg);

            logger.InfoFormat("report price#{0}#{1}action#{2} at screenTime#{3} occurTime{4}, usedDelayMills#{5}", screenPrice, targetPrice, action, screenTime, req.OccurTime, usedDelayMills);
        }

    }
}

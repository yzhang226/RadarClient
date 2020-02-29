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

        /// <summary>
        /// 一个应用实例仅有一个客户编号 
        /// </summary>
        private static int _clientNo;


        private SocketClient socketClient;

        private WindowSimulator simulator;

        private string _machineCode;

        private string _seatNo;

        public ClientService(SocketClient socketClient, WindowSimulator simulator)
        {
            this.socketClient = socketClient;
            this.simulator = simulator;
        }

        public void DoClientRegister()
        {
            BidderRegisterRequest req = new BidderRegisterRequest();
            req.MachineCode = GetMachineCode();
            req.ClientVersion = Ver.ver;
            req.LocalIpAddress = KK.GetLocalIP();
            req.SeatNo = GetClientSeatNo();


            JsonCommand comm = JsonCommands.OK(CommandDirective.CLIENT_REGISTER, req);

            RawMessage msg = MessageUtils.BuildJsonMessage(_clientNo, comm);


            socketClient.Send(msg);

            logger.InfoFormat("send DoClientRegister tcp request...");
        }

        public string GetClientSeatNo()
        {
            if (_seatNo == null)
            {
                _seatNo = KK.ReadClientSeatNo();
            }
            return _seatNo;
        }

        public string GetMachineCode()
        {
            if (_machineCode == null)
            {
                _machineCode = simulator.GetMachineCode();
            }

            return _machineCode;
        }

        public static int AssignedClientNo
        {
            get
            {
                return _clientNo;
            }
            set
            {
                logger.InfoFormat("Set AssignedClientNo is {0}", value);

                _clientNo = value;
            }
        }


    }
}

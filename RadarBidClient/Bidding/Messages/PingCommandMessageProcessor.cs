using log4net;
using Radar.Bidding.Command;
using Radar.Bidding.Model;
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

namespace Radar.Bidding.Messages
{
    [Component]
    public class PingCommandMessageProcessor : IMessageProcessor, InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PingCommandMessageProcessor));

        public RawMessageType MessageType()
        {
            return RawMessageType.PING_COMMAND;
        }

        public JsonCommand Handle(RawMessage message)
        {
            // logger.InfoFormat("receive pong");

            PingPongCounter.ME.IncrementPong();
            return null;
        }

        public void AfterPropertiesSet()
        {
            logger.InfoFormat("Ping message-processor AfterPropertiesSet called");
            MessageDispatcher.ME.Register(this);
        }

    }
}

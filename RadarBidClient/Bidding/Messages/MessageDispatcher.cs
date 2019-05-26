using log4net;
using Radar.Model;
using Radar.Common;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Messages
{


    public class MessageDispatcher
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Radar.Bidding.Messages.MessageDispatcher));

        private static readonly Dictionary<int, Radar.Bidding.Messages.IMessageProcessor> processors = new Dictionary<int, Radar.Bidding.Messages.IMessageProcessor>();

        public static readonly Radar.Bidding.Messages.MessageDispatcher me = new Radar.Bidding.Messages.MessageDispatcher();

        private MessageDispatcher()
        {

        }

        public void Dispatch(RawMessage message)
        {
            Radar.Bidding.Messages.IMessageProcessor processor = processors[message.messageType];
            if (processor != null)
            {
                processor.Handle(message);
            }
            else
            {
                logger.ErrorFormat("no processor for type#{0}, text#{1}.", message.messageType, message.bodyText);
            }
        }

        public void Register(Radar.Bidding.Messages.IMessageProcessor processor)
        {
            processors[processor.MessageType()] = processor;
            logger.InfoFormat("register message-type#{0} with processor#{1}", processor.MessageType(), processor);
        }

    }




}

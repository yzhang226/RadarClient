using log4net;
using Radar.Model;
using Radar.Common;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Radar.Common.Raw;
using Radar.Common.Utils;
using Radar.Common.Enums;

namespace Radar.Bidding.Messages
{


    public class MessageDispatcher
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MessageDispatcher));

        private static readonly Dictionary<int, IMessageProcessor> processors = new Dictionary<int, IMessageProcessor>();

        public static readonly MessageDispatcher me = new MessageDispatcher();

        private MessageDispatcher()
        {

        }

        public void Dispatch(RawMessage message)
        {
            IMessageProcessor processor = processors[message.messageType];
            if (processor != null)
            {
                processor.Handle(message);
            }
            else
            {
                logger.ErrorFormat("no processor for type#{0}, text#{1}.", message.messageType, message.bodyText);
            }
        }

        public void Register(IMessageProcessor processor)
        {
            int mType = EnumHelper.ToValue<RawMessageType>(processor.MessageType());
            processors[mType] = processor;
            logger.InfoFormat("register message-type#{0}({1}) with processor#{2}", processor.MessageType(), mType, processor);
        }

    }




}

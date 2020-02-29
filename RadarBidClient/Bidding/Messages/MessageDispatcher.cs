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
using Radar.Common.Model;
using System.Threading.Tasks;

namespace Radar.Bidding.Messages
{


    public class MessageDispatcher
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MessageDispatcher));

        private static readonly Dictionary<int, IMessageProcessor> processors = new Dictionary<int, IMessageProcessor>();

        public static readonly MessageDispatcher ME = new MessageDispatcher();

        private MessageDispatcher()
        {

        }

        public void Dispatch(RawMessage message)
        {
            Dispatch(message, null);
        }

        public void Dispatch(RawMessage message, Func<RawMessage, string> dispatchCallback)
        {
            IMessageProcessor processor = processors[message.messageType];
            if (processor != null)
            {
                Task.Factory.StartNew(() =>
                {
                    JsonCommand ret = processor.Handle(message);

                    if (dispatchCallback != null && ret != null && ret.Directive != CommandDirective.NONE)
                    {
                        RawMessage retRaw = MessageUtils.BuildJsonMessage(message.clientNo, ret);
                        string callbackRet = dispatchCallback.Invoke(retRaw);
                    }

                });        

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

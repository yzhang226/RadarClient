using log4net;
using Radar.Common;
using Radar.IoC;
using Radar.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Messages
{
    [Component]
    public class RawMessageProcessor : Radar.Bidding.Messages.IMessageProcessor, InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Radar.Bidding.Messages.RawMessageProcessor));

        public int MessageType()
        {
            return 10002;
        }

        public Radar.Bidding.Model.DataResult<string> Handle(RawMessage message)
        {
            Radar.Bidding.Model.CommandRequest co = MessageUtils.ParseAsCommandRequest(message.getBodyText());

            Radar.Bidding.Command.ICommand<string> processor = Radar.Bidding.Command.CommandProcessorFactory.GetProcessor(co.CommandName);
            if (processor == null)
            {
                logger.ErrorFormat("command#{0} has no command-processor", co.CommandName);
                return DataResults.Fail<string>("no command-processor");
            }

            Radar.Bidding.Model.DataResult<string> dr = processor.Execute(co.args);

            return dr;
        }

        public void AfterPropertiesSet()
        {
            logger.InfoFormat("AfterPropertiesSet called");
            Radar.Bidding.Messages.MessageDispatcher.me.Register(this);
        }

    }
}

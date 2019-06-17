using log4net;
using Radar.Bidding.Command;
using Radar.Bidding.Model;
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
    public class JsonCommandMessageProcessor : IMessageProcessor, InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(JsonCommandMessageProcessor));

        public RawMessageType MessageType()
        {
            return RawMessageType.JSON_COMMAND;
        }

        public JsonCommand Handle(RawMessage message)
        {
            JsonCommand comm = MessageUtils.ParseAsCommandRequest(message.clientNo, message.getBodyText());

            ICommand<string> commandProcessor = CommandProcessorFactory.GetProcessor(comm.Directive);
            if (commandProcessor == null)
            {
                logger.ErrorFormat("command#{0} has no command-processor", comm.directiveVal);
                return JsonCommands.Fail("no command-processor");
            }

            JsonCommand dr = commandProcessor.Execute(comm);

            return dr;
        }

        public void AfterPropertiesSet()
        {
            logger.InfoFormat("AfterPropertiesSet called");
            MessageDispatcher.me.Register(this);
        }

    }
}

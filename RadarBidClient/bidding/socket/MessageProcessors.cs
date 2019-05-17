using log4net;
using RadarBidClient.bidding.model;
using RadarBidClient.command;
using RadarBidClient.ioc;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.bidding.socket
{

    [Component]
    public class CommandMessageProcessor : MessageProcessor, InitializingBean
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(CommandMessageProcessor));

        private BidActionManager actionManager;

        public CommandMessageProcessor(BidActionManager actionManager)
        {
            this.actionManager = actionManager;
        }

        public int messageType()
        {
            return 10002;
        }

        public void ExecuteCommand(int messageType, string command)
        {

            logger.InfoFormat("start execute command#{0}", command);

            CommandRequest co = parse(command);

            // LocalCommandExecutor.executor.
            CommandProcessor<string> processor = CommandProcessorFactory.GetProcessor(co.CommandName);
            if (processor == null)
            {

                return;
            }


            processor.Execute(co.args);


            if (co.action == "MockLoginAndPhase1")
            {
                actionManager.MockLoginAndPhase1();
            }
            else if (co.action == "MockPhase2")
            {
                actionManager.MockPhase2();
            }
            else if (co.action == "ReopenNewBidWindow")
            {
                actionManager.ReopenNewBidWindow();
            }

            logger.InfoFormat("end execute command#{0}", command);
        }

        private CommandRequest parse(string command)
        {
            CommandRequest req = new CommandRequest();
            string comm = command.Trim();
            int len = comm.Length;

            int idx = comm.IndexOf("(");
            int idx2 = comm.IndexOf(")", idx + 1);

            if (idx > -1 && idx2 > -1)
            {
                req.action = comm.Substring(0, idx);

                if (idx2 > idx + 1)
                {
                    string[] arr = comm.Substring(idx + 1, idx2 - idx - 1).Split(',');

                    List<string> lis = new List<string>();
                    foreach (string a in arr)
                    {
                        string tr = a.Trim();
                        if (tr.Length == 0)
                        {
                            continue;
                        }
                        lis.Add(tr);
                    }

                    req.args = lis.ToArray();
                }
                else
                {
                    req.args = new string[0];
                }

            }
            else
            {
                req.action = comm;
                req.args = new string[0];
            }

            req.CommandName = (CommandEnum) Enum.ToObject(typeof(CommandEnum), int.Parse(req.action));

            return req;
        }

        public ProcessResult process(RawMessage message)
        {
            this.ExecuteCommand(message.getMessageType(), message.getBodyText());

            return null;
        }

        public void AfterPropertiesSet()
        {
            logger.InfoFormat("AfterPropertiesSet called");
            MessageDispatcher.dispatcher.register(this);
        }

    }

    public class LoginResponseProcessor : MessageProcessor
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(LoginResponseProcessor));

        public int messageType()
        {
            return 10011;
        }

        public ProcessResult process(RawMessage message)
        {
            logger.InfoFormat("login result#{0}", message.getBodyText());

            return null;
        }
    }

}

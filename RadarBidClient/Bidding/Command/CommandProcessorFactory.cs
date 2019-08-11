using log4net;
using Radar.Bidding.Command;
using Radar.Bidding.Model;
using Radar.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public class CommandProcessorFactory
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BidActionManager));

        // public static readonly CommandProcessorFactory executor = new CommandProcessorFactory();

        // TODO: 这里使用泛型遇到了问题 - 没有 任意类型的泛型 ? - 例如没有 List<?> 。所以这里使用了Base
        private static readonly Dictionary<CommandDirective, ICommand<string>> commands = new Dictionary<CommandDirective, ICommand<string>>();

        public static void Register(ICommand<string> comm)
        {
            commands[comm.GetDirective()] = comm;
            logger.InfoFormat("register command-directive#{0}", comm.GetDirective());
        }

        public static ICommand<string> GetProcessor(CommandDirective directive)
        {
            return commands[directive];
        }

    }
}

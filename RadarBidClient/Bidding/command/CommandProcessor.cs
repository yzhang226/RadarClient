using log4net;
using Radar.bidding;
using Radar.bidding.model;
using Radar.command;
using Radar.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.command
{

    public class CommandProcessorFactory
    {
        // public static readonly CommandProcessorFactory executor = new CommandProcessorFactory();

        // TODO: 这里使用泛型遇到了问题 - 没有 任意类型的泛型 ? - 例如没有 List<?> 。所以这里使用了Base
        private static readonly Dictionary<CommandEnum, BaseCommandProcessor> commands = new Dictionary<CommandEnum, BaseCommandProcessor>();

        public static void Register(BaseCommandProcessor command)
        {
            commands[command.CommandName()] = command;
        }

        public static CommandProcessor<string> GetProcessor(CommandEnum commandName)
        {
            return commands[commandName];
        }

    }

    public abstract class BaseCommandProcessor : CommandProcessor<string>
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(BaseCommandProcessor));

        public BaseCommandProcessor()
        {
            logger.InfoFormat("create local command process#{0}", this);

            CommandProcessorFactory.Register(this);
        }

        public abstract CommandEnum CommandName();

        public abstract DataResult<string> Execute(string[] args);

    }

    public interface CommandProcessor<T>
    {

        CommandEnum CommandName();

        DataResult<T> Execute(String[] args);

    }

    public class AccountLoginCommandProcessor : BaseCommandProcessor
    {
        public override CommandEnum CommandName()
        {
            return CommandEnum.ACCOUNT_LOGIN;
        }

        public override DataResult<string> Execute(string[] args)
        {
            // TODO: 
            return DataResults.OK("");
        }
    }

    public class PhaseOneBidCommandProcessor : BaseCommandProcessor
    {
        public override CommandEnum CommandName()
        {
            return CommandEnum.PHASE_ONE_OFFER_PRICE;
        }

        public override DataResult<string> Execute(string[] args)
        {
            // TODO: 
            return DataResults.OK("");
        }
    }

}

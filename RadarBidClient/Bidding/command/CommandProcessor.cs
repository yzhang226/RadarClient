using log4net;
using Radar.Bidding;
using Radar.Bidding.Model;
using Radar.Command;
using Radar.Common;
using Radar.IoC;
using Radar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Command
{

    public class CommandProcessorFactory
    {
        // public static readonly CommandProcessorFactory executor = new CommandProcessorFactory();

        // TODO: 这里使用泛型遇到了问题 - 没有 任意类型的泛型 ? - 例如没有 List<?> 。所以这里使用了Base
        private static readonly Dictionary<ControlDirective, BaseCommand> commands = new Dictionary<ControlDirective, BaseCommand>();

        public static void Register(BaseCommand command)
        {
            commands[command.GetDirective()] = command;
        }

        public static ICommand<string> GetProcessor(ControlDirective commandName)
        {
            return commands[commandName];
        }

    }

    
    public abstract class BaseCommand : ICommand<string>, InitializingBean
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(BaseCommand));

        public BaseCommand()
        {
            
        }

        public abstract ControlDirective GetDirective();

        public abstract DataResult<string> Execute(string[] args);

        public void AfterPropertiesSet()
        {
            logger.InfoFormat("Register Command#{0}", this);

            CommandProcessorFactory.Register(this);
        }
    }

    public interface ICommand<T>
    {

        ControlDirective GetDirective();

        DataResult<T> Execute(String[] args);

    }

    public class SystemTimeSyncCommand : BaseCommand
    {
        public override ControlDirective GetDirective()
        {
            return ControlDirective.SYNC_SYSTEM_TIME;
        }

        public override DataResult<string> Execute(string[] args)
        {
            bool bo = TimeSynchronizer.SyncFromNtpServer();
            return DataResults.OK(bo.ToString());
        }
    }

    public class CaptureUploadBidScreenCommand : BaseCommand
    {
        public override ControlDirective GetDirective()
        {
            return ControlDirective.CAPTURE_UPLOAD_BID_SCREEN;
        }

        public override DataResult<string> Execute(string[] args)
        {
            bool bo = TimeSynchronizer.SyncFromNtpServer();
            return DataResults.OK(bo.ToString());
        }
    }

    public class AccountLoginCommandProcessor : BaseCommand
    {
        public override ControlDirective GetDirective()
        {
            return ControlDirective.ACCOUNT_LOGIN;
        }

        public override DataResult<string> Execute(string[] args)
        {
            // TODO: 
            return DataResults.OK("");
        }
    }

    public class PhaseOneBidCommandProcessor : BaseCommand
    {
        public override ControlDirective GetDirective()
        {
            return ControlDirective.PHASE_ONE_OFFER_PRICE;
        }

        public override DataResult<string> Execute(string[] args)
        {
            // TODO: 
            return DataResults.OK("");
        }
    }

}

using log4net;
using Radar.Bidding.Model;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public abstract class BaseCommand : Radar.Bidding.Command.ICommand<string>, InitializingBean
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(Radar.Bidding.Command.BaseCommand));

        public BaseCommand()
        {

        }

        public abstract ReceiveDirective GetDirective();

        public abstract DataResult<string> Execute(string[] args);

        public void AfterPropertiesSet()
        {
            logger.InfoFormat("Register Command#{0}", this);

            Radar.Bidding.Command.CommandProcessorFactory.Register(this);
        }
    }
}

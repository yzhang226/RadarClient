using log4net;
using Radar.Bidding.Command;
using Radar.Bidding.Model;
using Radar.Common;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public abstract class BaseCommand<REQ> : ICommand<string>, InitializingBean
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(BaseCommand<REQ>));

        public BaseCommand()
        {

        }

        public abstract CommandDirective GetDirective();

        public DataResult<string> Execute(JsonCommand command)
        {
            REQ req = Jsons.FromJson<REQ>(command.data);
            DataResult<string> dr = DoExecute(req);
            return dr;
        }

        protected abstract DataResult<string> DoExecute(REQ req);

        public void AfterPropertiesSet()
        {
            logger.InfoFormat("Register Command#{0}", this);

            CommandProcessorFactory.Register(this);
        }
    }
}

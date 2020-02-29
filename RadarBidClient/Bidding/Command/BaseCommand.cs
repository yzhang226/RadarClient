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

        private Type reqType;

        public BaseCommand()
        {
            Type selfType = this.GetType();
            Type genericT = selfType.BaseType;
            reqType = genericT.GetGenericArguments()[0];
            logger.DebugFormat("command#{0} 's request type is {1}.", genericT, reqType);
        }

        public abstract CommandDirective GetDirective();

        public JsonCommand Execute(JsonCommand command)
        {
            REQ req;
            if (typeof(string).IsAssignableFrom(reqType))
            {
                // req = (REQ) command.data;
                req = (REQ) Convert.ChangeType(command.data, typeof(REQ));
            }
            else
            {
                req = Jsons.FromJson<REQ>(command.data);
            }

            JsonCommand dr = DoExecute(req);
            return dr;
        }

        protected abstract JsonCommand DoExecute(REQ req);

        public void AfterPropertiesSet()
        {
            logger.DebugFormat("Register Command#{0}", this);

            CommandProcessorFactory.Register(this);
        }
    }
}

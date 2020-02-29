using Radar.Common;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.Common.Times;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    [Component]
    public class SystemTimeSyncCommand : BaseCommand<string>
    {
        public override CommandDirective GetDirective()
        {
            return CommandDirective.SYNC_SYSTEM_TIME;
        }

        protected override JsonCommand DoExecute(string args)
        {
            bool bo = TimeSynchronizer.SyncFromNtpServer();
            return null;
        }
    }
}

using Radar.Bidding.Model;
using Radar.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public class SystemTimeSyncCommand : Radar.Bidding.Command.BaseCommand
    {
        public override ReceiveDirective GetDirective()
        {
            return ReceiveDirective.SYNC_SYSTEM_TIME;
        }

        public override DataResult<string> Execute(string[] args)
        {
            bool bo = Radar.Common.Times.TimeSynchronizer.SyncFromNtpServer();
            return DataResults.OK(bo.ToString());
        }
    }
}

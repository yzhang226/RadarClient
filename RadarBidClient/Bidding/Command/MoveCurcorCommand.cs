using log4net;
using Radar.Bidding.Model;
using Radar.Common;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Command
{

    [Component]
    public class MoveCurcorCommand : BaseCommand<string>
    {
        private BidActionManager bidActionManager;

        public MoveCurcorCommand(BidActionManager bidActionManager)
        {
            this.bidActionManager = bidActionManager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.MOVE_CURSOR;
        }

        protected override JsonCommand DoExecute(string args)
        {
            CoordPoint p = CoordPoint.FromAndAdjustRemote(args);
            int ret = bidActionManager.MoveCursor(p);
            return null;
        }
    }
    

}

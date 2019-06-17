using log4net;
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
            string[] arr = args.Split(',');

            int ret = bidActionManager.MoveCursor(int.Parse(arr[0]), int.Parse(arr[1]));

            return null;
        }
    }
    

}

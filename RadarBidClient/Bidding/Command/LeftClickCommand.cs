using Radar.Bidding.Model;
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
    public class LeftClickCommand : BaseCommand<string>
    {
        private BidActionManager bidActionManager;

        public LeftClickCommand(BidActionManager bidActionManager)
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
            bool needMoreOnceClick = arr.Length > 3 ? bool.Parse(arr[2]) : false;
            string memo = arr.Length > 4 ? arr[3] : "click";
            CoordPoint cp = bidActionManager.DeltaPoint(int.Parse(arr[0]), int.Parse(arr[1]));
            
            bidActionManager.ClickButtonAtPoint(cp, needMoreOnceClick, memo);

            return null;
        }
    }

}

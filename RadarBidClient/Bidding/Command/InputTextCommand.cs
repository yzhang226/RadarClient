using Radar.Bidding.Model;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Radar.Bidding.Command
{

    [Component]
    public class InputTextCommand : BaseCommand<string>
    {
        private BidActionManager bidActionManager;

        public InputTextCommand(BidActionManager bidActionManager)
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
            string x1 = arr[0];
            string y1 = arr[1];

            // 
            int idx = args.IndexOf(",", x1.Length + y1.Length + 2);
            string text = args.Substring(idx + 1);

            // bool needClear = arr.Length > 4 ? bool.TryParse(arr[3], out ret) : false;
            // string memo = arr.Length > 5 ? arr[4] : "input";

            CoordPoint cp = bidActionManager.DeltaPoint(int.Parse(x1), int.Parse(y1));

            bidActionManager.InputTextAtPoint(cp, text, true, "input");

            return null;
        }
    }

}

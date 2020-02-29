using Radar.Bidding.Model;
using Radar.Bidding.Model.Dto;
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
    public class InputTextCommand : BaseCommand<InputTextCommandRequest>
    {
        private BidActionManager bidActionManager;

        public InputTextCommand(BidActionManager bidActionManager)
        {
            this.bidActionManager = bidActionManager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.MOVE_CLICK_INPUT_TEXT;
        }

        protected override JsonCommand DoExecute(InputTextCommandRequest req)
        {
            string[] arr = req.Coord.Split(',');
            string x1 = arr[0];
            string y1 = arr[1];

            // get coord
            CoordPoint p = new CoordPoint(int.Parse(x1), int.Parse(y1)).DeltaRemote();

            // if need clear first
            bool needClear = true;

            // extract text
            string text = req.Text;

            CoordPoint cp = p;
            if (req.ScreenModeVal != 10)
            {
                cp = bidActionManager.DeltaPoint(p);
            }

            bidActionManager.InputTextAtPoint(cp, text, needClear, "remote-input");

            return null;
        }
    }

}

using Radar.Bidding.Model;
using Radar.Bidding.Model.Dto;
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
    public class LeftClickCommand : BaseCommand<LeftClickCommandRequest>
    {
        private BidActionManager bidActionManager;

        public LeftClickCommand(BidActionManager bidActionManager)
        {
            this.bidActionManager = bidActionManager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.MOVE_LEFT_CLICK;
        }

        protected override JsonCommand DoExecute(LeftClickCommandRequest req)
        {
            string[] arr = req.Coord.Split(',');
            CoordPoint p = new CoordPoint(int.Parse(arr[0]), int.Parse(arr[1])).DeltaRemote();

            bool isAbsolute = false;
            if (req.ScreenModeVal == 10)
            {
                isAbsolute = true;
            }
            
            
            bidActionManager.ClickBtnOnceAtPoint(p, "LC", isAbsolute);

            return null;
        }
    }

}

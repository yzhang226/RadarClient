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
    public class CaptureBidScreenCommand : BaseCommand<string>
    {
        private BidActionManager bidActionManager;

        public CaptureBidScreenCommand(BidActionManager bidActionManager)
        {
            this.bidActionManager = bidActionManager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.CAPTURE_BID_SCREEN;
        }

        protected override DataResult<string> DoExecute(string args)
        {

            string imgPath = bidActionManager.CaptureFlashScreen();

            return DataResults.OK(imgPath);
        }
    }
    

}

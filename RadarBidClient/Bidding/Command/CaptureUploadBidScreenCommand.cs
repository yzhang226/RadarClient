using log4net;
using Radar.Bidding.Model.Dto;
using Radar.Common;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.Common.Utils;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Command
{

    [Component]
    public class CaptureUploadBidScreenCommand : BaseCommand<string>
    {

        private BidActionManager bidActionManager;

        public CaptureUploadBidScreenCommand(BidActionManager bidActionManager)
        {
            this.bidActionManager = bidActionManager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.CAPTURE_UPLOAD_BID_SCREEN;
        }

        protected override JsonCommand DoExecute(string args)
        {
            string imgPath = bidActionManager.CaptureFlashScreen();
            ScreenImageUploadResponse resp = bidActionManager.UploadRobotScreenImage(imgPath);

            // return JsonCommands.OK(CommandDirective.CAPTURE_UPLOAD_BID_SCREEN, resp);
            return null;
        }
    }
    

}

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
    public class CaptureUploadFullScreenCommand : BaseCommand<string>
    {

        private BidActionManager bidActionManager;

        public CaptureUploadFullScreenCommand(BidActionManager bidActionManager)
        {
            this.bidActionManager = bidActionManager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.CAPTURE_UPLOAD_FULL_SCREEN;
        }

        protected override JsonCommand DoExecute(string args)
        {
            string imgPath = bidActionManager.CaptureFullScreen();
            ScreenImageUploadResponse resp = bidActionManager.UploadFileToSaber(imgPath);
            
            return null;
        }
    }
    

}

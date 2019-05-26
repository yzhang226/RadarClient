using log4net;
using Radar.Bidding;
using Radar.Bidding.Model;
using Radar.Common;
using Radar.IoC;
using Radar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Command
{

    public class CaptureUploadBidScreenCommand : Radar.Bidding.Command.BaseCommand
    {
        public override ReceiveDirective GetDirective()
        {
            return ReceiveDirective.CAPTURE_UPLOAD_BID_SCREEN;
        }

        public override DataResult<string> Execute(string[] args)
        {
            
            return DataResults.OK(false.ToString());
        }
    }
    

}

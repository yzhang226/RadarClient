﻿using log4net;
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
    public class CaptureUploadBidScreenCommand : BaseCommand<string>
    {

        public override CommandDirective GetDirective()
        {
            return CommandDirective.CAPTURE_UPLOAD_BID_SCREEN;
        }

        protected override DataResult<string> DoExecute(string args)
        {
            
            return DataResults.OK(false.ToString());
        }
    }
    

}

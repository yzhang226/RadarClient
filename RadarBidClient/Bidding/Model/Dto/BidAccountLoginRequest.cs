using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model.Dto
{
    public class BidAccountLoginRequest
    {


        public string MachineCode { get; set; }

        public string BidAccountNo { get; set; }

        public string BidAccountPswd { get; set; }

        public string BidAccountIdCard { get; set; }

        public string LoginCoords { get; set; }

        public int ScreenModeVal { get; set; }

    }
}

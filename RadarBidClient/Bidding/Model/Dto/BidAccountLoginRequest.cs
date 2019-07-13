using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model.Dto
{
    public class BidAccountLoginRequest
    {


        public String MachineCode { get; set; }

        public String BidAccountNo { get; set; }

        public String BidAccountPswd { get; set; }

        public String BidAccountIdCard { get; set; }

        public String LoginCoords { get; set; }


    }
}

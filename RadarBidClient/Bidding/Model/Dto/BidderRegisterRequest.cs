using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model.Dto
{
    public class BidderRegisterRequest
    {

        // public string MachineAddress { get; set; }

        public string MachineCode { get; set; }

        public string ClientVersion { get; set; }

        public string LocalIpAddress { get; set; }
        
    }
}

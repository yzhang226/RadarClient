using Radar.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model.Dto
{
    public class PriceActionRequest
    {

        public string MachineCode { get; set; }

        public PriceAction Action { get; set; }

        public int Price { get; set; }

        public DateTime ScreenTime { get; set; }

        public DateTime OccurTime { get; set; }

        public string Memo { get; set; }


    }
}

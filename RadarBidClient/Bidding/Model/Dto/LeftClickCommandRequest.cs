using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model.Dto
{
    public class LeftClickCommandRequest
    {

        public string MachineCode { get; set; }

        public string Coord { get; set; }

        public int ScreenModeVal { get; set; }

        public string Args { get; set; }


    }
}

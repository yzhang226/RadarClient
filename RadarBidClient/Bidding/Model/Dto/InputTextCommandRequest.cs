using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model.Dto
{
    public class InputTextCommandRequest
    {

        public string MachineCode { get; set; }

        public string Coord { get; set; }

        public int ScreenModeVal { get; set; }

        public string Text { get; set; }

        public string Args { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class CommandRequest
    {
        public int messageType;

        public ReceiveDirective CommandName;

        public string action;

        public string[] args;
    }
}

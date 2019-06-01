using Radar.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common.Model
{
    public class CommandRequest
    {
        public int MessageType { get; set; }

        public CommandDirective CommandName { get; set; }

        public string action;

        public string[] args;
    }
}

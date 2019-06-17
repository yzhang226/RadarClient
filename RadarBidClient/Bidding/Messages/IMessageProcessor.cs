using Radar.Bidding.Model;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.Common.Raw;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Messages
{
    public interface IMessageProcessor
    {

        JsonCommand Handle(RawMessage message);

        RawMessageType MessageType();

    }
}

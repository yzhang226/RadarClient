using Radar.Common;
using Radar.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Messages
{
    public interface IMessageProcessor
    {

        Radar.Bidding.Model.DataResult<string> Handle(RawMessage message);

        int MessageType();

    }
}

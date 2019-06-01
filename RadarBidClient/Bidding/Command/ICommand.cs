using Radar.Bidding.Model;
using Radar.Common.Enums;
using Radar.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public interface ICommand<T>
    {

        CommandDirective GetDirective();

        DataResult<T> Execute(JsonCommand req);

    }
}

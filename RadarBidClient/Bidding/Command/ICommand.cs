using Radar.Bidding.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public interface ICommand<T>
    {

        ReceiveDirective GetDirective();

        DataResult<T> Execute(String[] args);

    }
}

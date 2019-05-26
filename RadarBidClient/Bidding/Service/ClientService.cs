using Radar.Bidding.Net;
using Radar.Common;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Service
{

    [Component]
    public class ClientService
    {
        private SocketClient socketClient;

        public void DoClientLogin()
        {
            RawMessage msg = new RawMessage();

            // socketClient.Send();
        }




    }
}

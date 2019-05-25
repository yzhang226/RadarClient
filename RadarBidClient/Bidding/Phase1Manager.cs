using log4net;
using Radar.ioc;
using Radar.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.bidding
{
    [Component]
    public class Phase1Manager
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Phase1Manager));

        private ProjectConfig conf;

        private BidActionManager actionManager;


        public Phase1Manager(ProjectConfig conf, BidActionManager ActionManager)
        {
            this.conf = conf;
            this.actionManager = ActionManager;
        }




    }
}

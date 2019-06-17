using log4net;
using Radar.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model
{
    public class ClientMinuteState
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(ClientMinutePrice));

        private int hour;

        private int minute;

        /**
         * 客户端秒数价格
         */
        private ClientMinutePrice mp;

        public ClientMinuteState(int hour, int minute)
        {
            this.hour = hour;
            this.minute = minute;
            this.mp = new ClientMinutePrice();
        }


        public void addClientSecPrice(int sec, int price)
        {
            if (KK.IsNotSecond(sec))
            {
                logger.ErrorFormat("illegal second#{0}", sec);
                return;
            }


            mp.AddSecPrice(sec, price);

        }

        public void Clear()
        {

        }

    }
}

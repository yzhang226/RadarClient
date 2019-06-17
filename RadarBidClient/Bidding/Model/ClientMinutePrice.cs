using log4net;
using Radar.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model
{
    public class ClientMinutePrice
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(ClientMinutePrice));

        private int[] prices;

        public ClientMinutePrice()
        {
            prices = new int[60];
            for (int i = 0; i < 60; i++)
            {
                prices[i] = 0;
            }
        }

        public void AddSecPrice(int sec, int price)
        {
            if (KK.IsNotSecond(sec))
            {
                logger.ErrorFormat("illegal second#{0}", sec);
                return;
            }

            // 已经存在, 则不需要
            if (prices[sec] > 0)
            {

                return;
            }

            prices[sec] = price;
        }

        public int GetSecPrice(int sec)
        {
            if (KK.IsNotSecond(sec))
            {
                logger.ErrorFormat("illegal second#{0}", sec);
                return -1;
            }
            return prices[sec];
        }

        public void Clear()
        {
            for (int i = 0; i < 60; i++)
            {
                prices[i] = 0;
            }
        }


    }
}

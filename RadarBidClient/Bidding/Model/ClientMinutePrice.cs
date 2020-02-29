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

        private PagePrice[] prices;

        public ClientMinutePrice()
        {
            prices = new PagePrice[60];
            ReInit();
        }

        public void AddPriceIfNotSet(DateTime dt, int basePrice)
        {
            AddPriceIfNotSet(new PagePrice(dt, basePrice, basePrice - 300, basePrice + 300));
        }

        /// <summary>
        /// 如果未设置, 则设置该秒价格
        /// </summary>
        /// <param name="pr"></param>
        public void AddPriceIfNotSet(PagePrice pr)
        {
            if (GetSecPrice(pr.pageTime.Second) != null)
            {
                logger.InfoFormat("pageTime#{0} already setted ", pr);
                return;
            }

            prices[pr.pageTime.Second] = pr;
        }

        public PagePrice GetSecPrice(int sec)
        {
            return prices[sec];
        }

        public void ReInit()
        {
            for (int i = 0; i < 60; i++)
            {
                prices[i] = null;
            }
        }


    }
}

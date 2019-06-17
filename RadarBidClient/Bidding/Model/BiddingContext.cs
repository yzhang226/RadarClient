using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class BiddingContext
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BiddingContext));
        /// <summary>
        /// 
        /// </summary>
        private List<PagePrice> prices = new List<PagePrice>();

        private Dictionary<DateTime, PagePrice> priceMap = new Dictionary<DateTime, PagePrice>();



        /// <summary>
        /// 
        /// </summary>
        // private Dictionary<int, int> PriceAfter29 = new Dictionary<int, int>();

        private ClientMinutePrice m29 = new ClientMinutePrice();

        private Dictionary<int, PriceSubmitOperate> submitOperateMap = new Dictionary<int, PriceSubmitOperate>();

        private static Dictionary<string, PriceSubmitOperate> uuidOfsubmitOperateMap = new Dictionary<string, PriceSubmitOperate>();

        public void AddPrice(int sec, int basePrice)
        {
            //PriceAfter29[sec] = basePrice;
            m29.AddSecPrice(sec, basePrice);
        }

        public int GetPrice(int sec)
        {
            return m29.GetSecPrice(sec);
            //if (!PriceAfter29.ContainsKey(sec))
            //{
            //    return -1;
            //}

            //return PriceAfter29[sec];
        }

        public PriceSubmitOperate AddPriceSetting(SubmitPriceSetting settting)
        {
            var oper = new PriceSubmitOperate();
            oper.setting = settting;
            oper.status = -1;

            submitOperateMap[settting.second] = oper;

            return oper;
        }

        public Dictionary<int, PriceSubmitOperate> GetSubmitOperateMap()
        {
            return submitOperateMap;
        }



        public bool RemoveSubmitOperate(int second)
        {
            if (submitOperateMap.ContainsKey(second))
            {
                return false;
            }

            return submitOperateMap.Remove(second);
        }

        public void CleanSubmitOperate()
        {
            submitOperateMap.Clear();
        }

        public static PriceSubmitOperate GetSubmitOperateByUuid(string uuid)
        {
            if (!uuidOfsubmitOperateMap.ContainsKey(uuid))
            {
                return null;
            }
            return uuidOfsubmitOperateMap[uuid];
        }


        public void AddPagePrice(PagePrice price)
        {
            prices.Add(price);
            priceMap[price.pageTime] = price;
        }

        public void PutAwaitImage(CaptchaAnswerImage image, PriceSubmitOperate oper)
        {
            CaptchaTaskContext.me.PutAwaitImage(image);
            if (oper != null)
            {
                uuidOfsubmitOperateMap[image.Uuid] = oper;
            }
        }

    }
}

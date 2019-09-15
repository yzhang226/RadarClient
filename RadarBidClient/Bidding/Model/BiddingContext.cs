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

        private ClientMinutePrice m29 = new ClientMinutePrice();
        private int[] flags = new int[60];

        private Dictionary<int, PriceStrategyOperate> submitOperateMap = new Dictionary<int, PriceStrategyOperate>();

        private static Dictionary<string, PriceStrategyOperate> uuidOfsubmitOperateMap = new Dictionary<string, PriceStrategyOperate>();

        private int lastCalcedSec;

        public BiddingContext()
        {
            for (int i=0; i<60; i++)
            {
                flags[i] = 0;
            }
        }

        public void AddPrice(PagePrice pr)
        {
            m29.AddPriceIfNotSet(pr);
        }

        public int GetPrice(int sec)
        {
            var pp = m29.GetSecPrice(sec);
            
            return pp != null ? pp.basePrice : 0;
        }

        /// <summary>
        /// ���� �Ƿ� �ѱ������ 
        /// </summary>
        /// <param name="pr"></param>
        /// <returns></returns>
        public bool IsPagePriceCalced(PagePrice pr)
        {
            return flags[pr.pageTime.Second] == 1;
        }

        /// <summary>
        ///  ���Զ� ���� ���� 
        ///  ��� ���� ��ǰ�ѱ���������򷵻� false
        ///  ��� ���� ��ǰδ����������򷵻� true, ������ ���� �ѱ������
        /// </summary>
        /// <param name="pr"></param>
        /// <returns></returns>
        public bool TryStartPagePrice(PagePrice pr)
        {
            lock (logger)
            {
                if (IsPagePriceCalced(pr))
                {
                    return false;
                }

                if (pr.pageTime.Second < lastCalcedSec)
                {
                    logger.WarnFormat("price#{0} less then last-calc-sec#{1}, is not legal", pr, lastCalcedSec);
                    return false;
                }

                flags[pr.pageTime.Second] = 1;
                lastCalcedSec = pr.pageTime.Second;

                return true;
            }
        }

        public PriceStrategyOperate AddPriceSetting(SubmitPriceSetting settting)
        {
            var oper = new PriceStrategyOperate();
            oper.setting = settting;
            oper.status = -1;

            submitOperateMap[settting.second] = oper;

            return oper;
        }

        public Dictionary<int, PriceStrategyOperate> GetSubmitOperateMap()
        {
            return submitOperateMap;
        }



        public bool RemoveSubmitOperate(int second)
        {
            PriceStrategyOperate opert = null;
            bool ret = submitOperateMap.TryGetValue(second, out opert);

            logger.InfoFormat("try RemoveSubmitOperate second#{0}, opert#{1}, ret#{2} map count#{3}", second, opert, ret, submitOperateMap.Count);

            if (opert != null)
            {
                submitOperateMap.Remove(second);
            }

            return false;
        }

        public void CleanSubmitOperate()
        {
            submitOperateMap.Clear();
        }

        public static PriceStrategyOperate GetSubmitOperateByUuid(string uuid)
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

        public void PutAwaitImage(CaptchaAnswerImage image, PriceStrategyOperate oper)
        {
            CaptchaTaskContext.me.PutAwaitImage(image);
            if (oper != null)
            {
                uuidOfsubmitOperateMap[image.Uuid] = oper;
            }
        }

    }
}

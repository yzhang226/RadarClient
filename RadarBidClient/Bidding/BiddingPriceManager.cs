﻿using log4net;
using Radar.Bidding.Model;
using Radar.Common;
using Radar.Common.Enums;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding
{
    public class BiddingPriceManager
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(BiddingScreen));

        private static readonly DateTime FinalTime = new DateTime(2019, 08, 18, 11, 29, 57);

        private SubmitStrategyManager strategyManager;

        private BiddingContext biddingContext;

        private Func<BiddingPriceRequest, bool> cancelStrategyFunc;

        private List<SubmitPriceSetting> strategies;
        private Dictionary<int, BiddingPriceRequest> strategySecondRequests = new Dictionary<int, BiddingPriceRequest>();

        public BiddingPriceManager(SubmitStrategyManager strategyManager, BiddingContext biddingContext, Func<BiddingPriceRequest, bool> cancelStrategyFunc)
        {
            this.strategyManager = strategyManager;
            this.biddingContext = biddingContext;

            this.strategies = strategyManager.LoadStrategies();
        }

        public BiddingPriceRequest Calcs(PagePrice pp)
        {
            var cnt = strategies.Count;
            BiddingPriceRequest previous = null;
            BiddingPriceRequest req = null;
            for (var i = 0; i < cnt; i++)
            {
                var stra = strategies[i];
                if (pp.pageTime.Second < stra.second)
                {
                    continue;
                }

                req = Calc(pp, stra);
                if (req == null)
                {
                    continue;
                }

                // try cancel previous strategy
                //if (previous != null && cancelStrategyFunc != null)
                //{
                //    cancelStrategyFunc(previous);
                //    previous.OperateStatus = StrategyOperateStatus.CANCELLED;
                //    previous.CanncelMemo = "canceled";
                //    strategies.RemoveAt(i);
                //    logger.InfoFormat("strategy#{0} is canceled.", stra.second);
                //}

                previous = req;
            }

            // 
            return req;
        }

        /// <summary>
        /// 取消 已过期的
        /// </summary>
        /// <param name="lastStrategySecond"></param>
        /// <returns></returns>
        public bool CancelnOutOfDateRequest(int lastStrategySecond)
        {
            var reqs = GetPreviousUnSubmitRequest(lastStrategySecond);

            foreach (var req in reqs)
            {
                if (req.StrategySecond >= lastStrategySecond)
                {
                    continue;
                }

                req.OperateStatus = StrategyOperateStatus.CANCELLED;
                logger.InfoFormat("strategy#{0} is canceled.", req.StrategySecond);

                RemoveStrategy(req.StrategySecond);
            }

            return true;
        }

        public List<BiddingPriceRequest> GetPreviousUnSubmitRequest(int lastStrategySecond)
        {
            var cnt = strategies.Count;
            List<BiddingPriceRequest> reqs = new List<BiddingPriceRequest>();
            for (var i = 0; i < cnt; i++)
            {
                var stra = strategies[i];
                BiddingPriceRequest req = null;
                strategySecondRequests.TryGetValue(stra.second, out req);

                if (stra.second < lastStrategySecond && req != null)
                {
                    reqs.Add(req);
                }

            }

            return reqs;
        }

        public void RemoveStrategy(int second)
        {
            var cnt = strategies.Count;
            for (var i = 0; i < cnt; i++)
            {
                var stra = strategies[i];
                if (stra.second == second)
                {
                    strategies.RemoveAt(i);
                    strategySecondRequests.Remove(stra.second);
                    logger.InfoFormat("strategy#{0} is removed", second);
                    break;
                }
            }
        }

        public BiddingPriceRequest Calc(PagePrice pp, SubmitPriceSetting strategy)
        {
            if (pp.pageTime.Second < strategy.second)
            {
                return null;
            }

            BiddingPriceRequest req = null;
            strategySecondRequests.TryGetValue(strategy.second, out req);
            if (req == null)
            {
                req = CheckPriceOffer(pp, strategy);
                strategySecondRequests[strategy.second] = req;
            }

            if (req == null)
            {
                return null;
            }

            if (req.OperateStatus == StrategyOperateStatus.PRICE_SUBMITTED || req.OperateStatus == StrategyOperateStatus.CANCELLED)
            {
                logger.WarnFormat("strategy#{0} status#{1} still try calc", strategy.second, req.OperateStatus);
                return null;
            }

            if (UsePriceMatchRule(pp, req) 
                || UseFinalRule(pp, req) 
                || UseBack2PriceRule(pp, req) 
                || UseBack3PriceRule(pp, req))
            {
                return req;
            }

            return req;
        }

        public BiddingPriceRequest CheckPriceOffer(PagePrice pp, SubmitPriceSetting strategy)
        {
            if (pp.pageTime.Second != strategy.second)
            {
                return null;
            }
            
            BiddingPriceRequest req = new BiddingPriceRequest();
            req.OperateStatus = StrategyOperateStatus.NEED_OFFER_PRICE;
            req.OfferedScreenTime = pp.pageTime;
            req.OfferedScreenPrice = pp.basePrice;
            req.ComputedDelayMills = 0;
            req.StrategySecond = strategy.second;
            req.TargetPrice = strategy.deltaPrice + pp.basePrice;
            req.CaptchaUuid = "";

            return req;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public bool InputAnswer(string uuid)
        {
            var req = GetRequestByUuid(uuid);

            if (req == null)
            {
                logger.WarnFormat("uuid#{0} do not match BiddingPriceRequest", uuid);

                return false;
            }

            req.OperateStatus = StrategyOperateStatus.CAPTCHA_INPUTTED;
            logger.InfoFormat("strategy#{0} captcha-answer inputted", req.StrategySecond);
            return true;
        }

        public BiddingPriceRequest GetRequestByUuid(string uuid)
        {
            foreach (var item in strategySecondRequests)
            {
                var req = item.Value;
                if (uuid.Equals(req.CaptchaUuid))
                {
                    return req;
                }
            }

            return null;
        }

        private bool UsePriceMatchRule(PagePrice pp, BiddingPriceRequest req)
        {
            if (req.TargetPrice <= (pp.basePrice + 300))
            {
                req.CanSubmit = true;
                req.SubmitMemo = "Price Matched";
                return true;
            }

            return false;
        }

        private bool UseFinalRule(PagePrice pp, BiddingPriceRequest req)
        {
            if (pp.pageTime.Hour >= FinalTime.Hour
            && pp.pageTime.Minute >= FinalTime.Minute
            && pp.pageTime.Second >= FinalTime.Second)
            {
                DateTime occurTime = DateTime.Now;
                int delay = KK.RandomInt(ForceStart, ForceEnd);
                req.CanSubmit = true;
                req.ComputedDelayMills = delay;
                req.SubmitMemo = "Final Rule";

                return true;
            }

            return false;
        }

        private bool UseBack2PriceRule(PagePrice pp, BiddingPriceRequest req)
        {
            var sec = pp.pageTime.Second;
            var offeredPrice = req.TargetPrice;
            var PriceAt50 = biddingContext.GetPrice(50);
            int PriceBack2 = biddingContext.GetPrice(sec - 2);
            bool price50Matched = sec > 50 && PriceAt50 > 0 && offeredPrice >= (PriceAt50 + DrawbackDeltaPrice + 100);
            if (!price50Matched)
            {
                return false;
            }

            bool priceBack2Matched = sec >= 54 && PriceBack2 > 0 && (pp.basePrice - PriceBack2) >= 200;
            if (!priceBack2Matched)
            {
                return false;
            }

            bool matched = false;
            int delay = -1;
            DateTime occurTime = DateTime.Now;
            if (offeredPrice == (pp.basePrice + 300 + 100)) // 提前 100
            {
                delay = KK.RandomInt(Delay1Start, Delay1End);
                matched = true;
            }
            else if (offeredPrice == (pp.basePrice + 300 + 200))
            {
                delay = KK.RandomInt(Delay2Start, Delay2End);
                matched = true;
            }
            else if (offeredPrice == (pp.basePrice + 300 + 300))
            {
                delay = KK.RandomInt(Delay3Start, Delay3End);
                matched = true;
            }

            if (matched)
            {
                req.CanSubmit = true;
                req.ComputedDelayMills = delay;
                req.SubmitMemo = "Back2 Rule";
            }

            return matched;
        }

        private bool UseBack3PriceRule(PagePrice pp, BiddingPriceRequest req)
        {
            var sec = pp.pageTime.Second;
            var offeredPrice = req.TargetPrice;
            var PriceAt50 = biddingContext.GetPrice(50);
            int PriceBack2 = biddingContext.GetPrice(sec - 2);
            bool price50Matched = sec > 50 && PriceAt50 > 0 && offeredPrice >= (PriceAt50 + DrawbackDeltaPrice + 100);
            if (!price50Matched)
            {
                return false;
            }

            bool priceBack3Matched = sec >= 56 && (pp.basePrice - biddingContext.GetPrice(sec - 3)) == 200;
            if (!priceBack3Matched)
            {
                return false;
            }

            bool matched = false;
            int delay = -1;
            DateTime occurTime = DateTime.Now;
            if (offeredPrice == pp.basePrice + 300 + 100) // 提前100
            {
                delay = KK.RandomInt(delayOneS, delayOneE);
                matched = true;
            }
            // 20190714 增加判断 连续变动价格（100）
            else if (offeredPrice == pp.basePrice + 300 + 200) // 提前 200
            {
                delay = KK.RandomInt(delayTwoS, delayTwoE);
                matched = true;
            }

            if (matched)
            {
                req.CanSubmit = true;
                req.ComputedDelayMills = delay;
                req.SubmitMemo = "Back3 Rule";
            }

            return matched;
        }
        

        private static readonly int DrawbackDeltaPrice = 500;

        private static readonly int Delay1Start = 0;
        private static readonly int Delay1End = 200;

        private static readonly int Delay2Start = 0;
        private static readonly int Delay2End = 600;

        private static readonly int Delay3Start = 500;
        private static readonly int Delay3End = 900;

        private static readonly int delayOneS = 0;
        private static readonly int delayOneE = 600;

        private static readonly int delayTwoS = 500;
        private static readonly int delayTwoE = 900;

        private static readonly int ForceStart = 200;
        private static readonly int ForceEnd = 1200;

    }
}

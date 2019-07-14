﻿ using log4net;
using Radar.Bidding.Model;
using Radar.Common;
using Radar.Common.Threads;
using Radar.IoC;
using Radar.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Radar.Bidding
{
    [Component]
    public class BiddingScreen : InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BiddingScreen));

        private ProjectConfig conf;
        private BidActionManager actionManager;

        private BiddingContext biddingContext = new BiddingContext();

        public CaptchaAnswerImage Phase2PreviewCaptcha;

        private static bool isCollectingWork = true;
        private static Thread collectingThread;

        private bool IsPreviewDone = false;

        private bool IsOneRoundStarted = false;

        private SubmitStrategyManager strategyManager;

        private WebBrowser webBro;

        private Phase2ActManager phase2Manager;


        private static TimeSpan PreviewStartTime = new TimeSpan(11, 29, 15);

        private static TimeSpan PreviewEndTime = new TimeSpan(11, 29, 29);

        private TextBlock ShowUpBlock;

        // private static bool ThreadContinue = true;

        private int DrawbackDeltaPrice = 500;

        private int Delay1Start = 0;
        private int Delay1End = 200;

        private int Delay2Start = 0;
        private int Delay2End = 600;

        private int Delay3Start = 500;
        private int Delay3End = 900;

        private int delayOneS = 0;
        private int delayOneE = 600;

        private int delayTwoS = 500;
        private int delayTwoE = 900;

        private int ForceStart = 200;
        private int ForceEnd = 1200;

        // private CaptchaAnswerImage LastImage;

        public BiddingScreen(ProjectConfig conf, BidActionManager actionManager, Phase2ActManager phase2Manager, SubmitStrategyManager strategyManager)
        {
            this.conf = conf;
            this.actionManager = actionManager;
            this.phase2Manager = phase2Manager;
            this.strategyManager = strategyManager;
        }

        public void SetWebBrowser(WebBrowser webBro)
        {
            this.webBro = webBro;
        }

        public void AfterPropertiesSet()
        {
            if (conf.EnableAutoBidding)
            {
                this.ResetAndRestart();
            }
        }

        public void SetShowUpBlock(TextBlock ShowUpBlock)
        {
            this.ShowUpBlock = ShowUpBlock;
        }

        public void ResetAndRestart()
        {
            ResetContext();
            StartCollectingThread();
        }

        public void ResetContext()
        {
            RefreshBiddingPage();

            biddingContext = new BiddingContext();
            ResetStrategyByReload();

            IsPreviewDone = false;
            IsOneRoundStarted = false;
        }

        public void ResetStrategyByReload()
        {
            biddingContext.CleanSubmitOperate();

            List<SubmitPriceSetting> settings = this.strategyManager.LoadStrategies();
            foreach (SubmitPriceSetting sps in settings)
            {
                biddingContext.AddPriceSetting(sps);
            }
        }

        public void RewriteAndResetStrategyFile(string strategyText)
        {
            strategyManager.WriteNewStrategyToFile(strategyText);
            this.ResetStrategyByReload();

        }

        public void RefreshBiddingPage()
        {
            if (this.webBro == null)
            {
                return;
            }

            Action action1 = () =>
            {
                this.webBro.Refresh();
            };

            this.webBro.Dispatcher.BeginInvoke(action1);
        }


        private void SetShowUpText(string text)
        {
            if (ShowUpBlock == null)
            {
                return;
            }

            Action action1 = () =>
            {
                ShowUpBlock.Text = text;
            };

            ShowUpBlock.Dispatcher.BeginInvoke(action1);
        }


        /// <summary>
        /// 需要一个线程 收集页面信息 - 价格时间, 从11:29:00开始
        /// </summary>
        private void StartCollectingThread()
        {
            StopCollectingThread();

            isCollectingWork = true;
            collectingThread = ThreadUtils.StartNewBackgroudThread(LoopDetectPriceAndTimeInScreen);
        }

        public static void StopCollectingThread()
        {
            isCollectingWork = false;
            ThreadUtils.TryStopThreadByWait(collectingThread, 60, 60, "collectingThread");
        }

        public void Reset()
        {
            biddingContext = new BiddingContext();
        }


        private void LoopDetectPriceAndTimeInScreen()
        {
            logger.InfoFormat("begin loopDetectPriceAndTimeInScreen");
            int i = 0;

            PageTimePriceResult lastResultx = null;
            while (isCollectingWork)
            {
                long ss = KK.CurrentMills();

                try
                {
                    long s1 = KK.CurrentMills();
                    PageTimePriceResult lastResult = actionManager.DetectPriceAndTimeInScreen(lastResultx);

                    // 重复检测
                    if (lastResult.status == 300)
                    {
                        continue;
                    }

                    SetShowUpText(ToShowUpText(lastResult));

                    // 处理异常情况
                    if (lastResult.status != 0)
                    {
                        ProcessErrorDetect(lastResult);

                        // TODO: 这里也需要处理 可能需要的提交，因为远程会下发一个可能的当前秒数
                        // 目的是防止出现卡秒现象

                        continue;
                    }                    

                    lastResultx = lastResult;

                    PagePrice pp = lastResult.data;

                    logger.DebugFormat("detectPriceAndTimeInScreen elapsed {0}ms", KK.CurrentMills() - s1);

                    if (pp != null)
                    {
                        if (pp.pageTime.Minute == 28)
                        {
                            if (IsOneRoundStarted)
                            {
                                ResetContext();
                                IsOneRoundStarted = false;
                            }
                        }
                        else if (pp.pageTime.Minute == 29)
                        {
                            if (!IsOneRoundStarted)
                            {
                                IsOneRoundStarted = true;
                            }
                        }

                        s1 = KK.CurrentMills();
                        AfterSuccessDetect(pp);
                        logger.DebugFormat("afterDetect elapsed {0}ms", KK.CurrentMills() - s1);
                    }
                    else
                    {
                        // TODO: 

                    }

                }
                catch (Exception e)
                {
                    logger.Error("detect price and time error", e);
                }
                finally
                {
                    // TODO: 这里可能不需要每次重置dict
                    // actionManager.ResetDictIndex();

                    KK.Sleep(12);
                }

                logger.DebugFormat("round {0} loopDetectPriceAndTimeInScreen elapsed {1}ms", i++, KK.CurrentMills() - ss);
            }

            logger.InfoFormat("END loopDetectPriceAndTimeInScreen ");
        }

        private string ToShowUpText(PageTimePriceResult LastResult)
        {
            string prefix = string.Format("本机时间：{0:HH:mm:ss}。\n解析的内容是：", DateTime.Now);

            if (LastResult.status != 0)
            {
                return string.Format("{0}ERROR: {1}", prefix, LastResult.status);
            }
            
            PagePrice pp = LastResult.data;

            return string.Format("{0}{1:HH:mm:ss}.\n{2} - {3}.", prefix, pp.pageTime, pp.low, pp.high);
        }

        private TimeSpan FinalTime = new TimeSpan(11, 29, 57);

        private void AfterSuccessDetect(PagePrice pp)
        {

            // 1. 11:29:15 获取验证码 用于预览
            if (!IsPreviewDone && pp.pageTime.TimeOfDay >= PreviewStartTime && pp.pageTime.TimeOfDay <= PreviewEndTime)
            {
                PreviewPhase2Captcha(pp);
                return;
            }

            int hour = pp.pageTime.Hour;

            if (hour != 11)
            {
                return;
            }

            long s1 = KK.CurrentMills();

            int minute = pp.pageTime.Minute;
            int sec = pp.pageTime.Second;
            
            
            DateTime now = DateTime.Now;

            var strats = new Dictionary<int, PriceSubmitOperate>(biddingContext.GetSubmitOperateMap());

            // TODO: 临时改动
            int baseMinute = 28;

            if (minute == 29)
            // if (minute > baseMinute)
            {
                biddingContext.AddPrice(sec, pp.basePrice);
            }

            foreach (var item in strats)
            {
                int fixSec = item.Key;

                var oper = item.Value;
                SubmitPriceSetting stra = oper.setting;
                int fixMinute = stra.minute > 0 ? stra.minute : 29;

                if (fixMinute != minute)
                // if (fixMinute < baseMinute)
                {
                    continue;
                }

                // 到点就出价
                if (sec == fixSec)
                {

                    // 0. 检查之前秒数的策略状态
                    foreach (var va in strats)
                    {
                        if (va.Key < fixSec)
                        {
                            // 取消之前的策略
                            logger.InfoFormat("previous strategy second#{0} detected#{1}, cancel this strategy.", va.Key, va.Value.status);
                            va.Value.status = 99;
                            biddingContext.RemoveSubmitOperate(va.Key);
                        }
                    }


                    stra.basePrice = pp.basePrice;

                    logger.InfoFormat("find target second#{0}, delta#{1}, base-price#{2}, offer-price#{3}", fixSec, stra.deltaPrice, pp.basePrice, stra.GetOfferedPrice());

                    // 1. 此处直接出价
                    CaptchaAnswerImage ansImg = phase2Manager.OfferPrice(stra.GetOfferedPrice(), true);
                    biddingContext.PutAwaitImage(ansImg, oper);

                    oper.answerUuid = ansImg.Uuid;
                    oper.status = 0;
                    oper.status = 20;

                }

                // 检查是否到价
                if (sec >= fixSec && (oper.status != 1 && oper.status != 99 && oper.status != -1))
                {
                    // 2. 检查提交价格策略
                    int offeredPrice = stra.GetOfferedPrice();
                    int PriceAt50 = biddingContext.GetPrice(50);
                    string answer = CaptchaTaskContext.me.GetAnswer(oper.answerUuid);

                    // logger.InfoFormat("try target second#{0}, offer-price#{1}, delta#{2}. delta ", fixSec, OfferedPrice, (pp.basePrice + 300));

                    if (offeredPrice <= (pp.basePrice + 300))
                    {
                        logger.InfoFormat("N10 submit target second#{0}, offer-price#{1}, delta#{2}.", fixSec, offeredPrice, stra.deltaPrice);

                        if (answer?.Length > 0)
                        {
                            SubmitOfferedPrice(fixSec, oper, answer);
                        }
                        else
                        {
                            // TODO: 到价时，依然未有验证码
                            // oper.status = 20;
                            logger.WarnFormat("N10 target second#{0} has no captcha-answer", fixSec);
                        }
                        
                    }

                    if (sec > 50
                                && hour >= FinalTime.Hours
                                && minute >= FinalTime.Minutes
                                && sec >= FinalTime.Seconds)
                    {
                        int delay = KK.RandomInt(ForceStart, ForceEnd);

                        logger.InfoFormat("N100 Trigger force submit second#{0}, OfferedPrice#{1}, base-price#{2}. {3}. delay#{4}", fixSec, offeredPrice, pp.basePrice, (now.TimeOfDay >= FinalTime), delay);
                        
                        KK.Sleep(delay);

                        SubmitOfferedPrice(fixSec, oper, answer);

                        logger.InfoFormat("N100 Force submit second#{0}, delay#{1}, OfferedPrice#{2}, base-price#{3}.", fixSec, delay, offeredPrice, pp.basePrice);
                    }

                    if (sec > 50 && PriceAt50 > 0 && offeredPrice >= (PriceAt50 + DrawbackDeltaPrice + 100))
                    {
                        int PriceBack2 = biddingContext.GetPrice(sec - 2);

                        // 20190616 增加 当前时间 >= 112954
                        if (sec >= 54 && PriceBack2 > 0 && (pp.basePrice - PriceBack2) >= 200)
                        {
                            bool matched = false;
                            int delay = -1;
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
                                
                                KK.Sleep(delay);

                                SubmitOfferedPrice(fixSec, oper, answer);

                                logger.InfoFormat("N10 matched second#{0}, delay#{1}, OfferedPrice#{2}, base-price#{3}. PriceBack2#{4}", fixSec, delay, offeredPrice, pp.basePrice, PriceBack2);
                            }

                        }
                    }
                    else if (sec >= 56 && (pp.basePrice - biddingContext.GetPrice(sec - 3)) == 20)
                    {
                        if (offeredPrice == pp.basePrice + 300 + 100) // 提前100
                        {
                            int delay = KK.RandomInt(delayOneS, delayOneE);
                            KK.Sleep(delay);

                            SubmitOfferedPrice(fixSec, oper, answer);

                            logger.InfoFormat("N20 matched second#{0}, delay#{1}, OfferedPrice#{2}, base-price#{3}. PriceBack3#{4}", fixSec, delay, offeredPrice, pp.basePrice, biddingContext.GetPrice(sec - 3));
                        }

                        // 20190714 增加判断 连续变动价格（100）
                        else if (offeredPrice == pp.basePrice + 300 + 200) // 提前 200
                        {
                            int delay = KK.RandomInt(delayTwoS, delayTwoE);
                            KK.Sleep(delay);

                            SubmitOfferedPrice(fixSec, oper, answer);

                            logger.InfoFormat("N21 matched second#{0}, delay#{1}, OfferedPrice#{2}, base-price#{3}. PriceBack3#{4}", fixSec, delay, offeredPrice, pp.basePrice, biddingContext.GetPrice(sec - 3));
                        }

                        else
                        {
                            logger.InfoFormat("N25 sec#56 ELSE OfferedPrice#{0}, pp.basePrice#{0}", offeredPrice, pp.basePrice);
                        }
                    }
                    else
                    {
                        logger.InfoFormat("N39 ELSE - {0}, {1}, {2}. ", now.TimeOfDay, FinalTime, (now.TimeOfDay >= FinalTime));
                    }

                }
                

            }

            logger.InfoFormat("afterDetect elapsed {0}ms", KK.CurrentMills() - s1);

        }

        private int SubmitOfferedPrice(int fixSec, PriceSubmitOperate oper, string answer)
        {
            if (oper.status == 21)
            {
                phase2Manager.SubmitOfferedPrice();
            }
            else
            {
                if (answer == null || answer.Length == 0)
                {
                    logger.ErrorFormat("target second#{0} has no captcha-answer", fixSec);
                    return -1;
                }

                logger.InfoFormat("submit target second#{0}, and fill captcha answer#{1}", fixSec, answer);
                phase2Manager.SubmitOfferedPrice(answer);
            }
            oper.status = 1;
            biddingContext.RemoveSubmitOperate(fixSec);

            return 0;
        }

        private void ProcessErrorDetect(PageTimePriceResult ret)
        {
            if (ret.status == -1)
            {
                actionManager.FindAndSetCoordOfCurrentTime();
            }
            else if (ret.status == -2)
            {
                actionManager.FindAndSetCoordOfPriceSection();
            }
            else if (ret.status == -11)
            {
                actionManager.FindAndSetCoordOfCurrentTime();
            }
            else if (ret.status == -12)
            {
                actionManager.FindAndSetCoordOfPriceSection();
            }

        }



        public CaptchaAnswerImage PreviewPhase2Captcha(PagePrice pp)
        {
            // TODO: 这里使用异步处理，否则出现不能显示验证码。
            // TODO: 这里可以归为一类问题：模拟时，必须等到所有操作才能显示页面。需要解决。

            CaptchaAnswerImage img = null;
            Task.Factory.StartNew(() =>
            {
                img = phase2Manager.OfferPrice(pp.basePrice + 1500, false);
                phase2Manager.CancelOfferedPrice();

                biddingContext.PutAwaitImage(img, null);
                Phase2PreviewCaptcha = img;
            });

            IsPreviewDone = true;

            return img;
        }


    }

}

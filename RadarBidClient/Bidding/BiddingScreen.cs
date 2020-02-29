using log4net;
using Radar.Bidding.Model;
using Radar.Bidding.Service;
using Radar.Common;
using Radar.Common.Enums;
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

        private PriceActionService priceActionService;

        private WebBrowser webBro;

        private Phase2ActManager phase2Manager;

        private BiddingPriceManager biddingPriceManager;
        
        private TextBlock ShowUpBlock;

        public TextBlock StrategyBlock { get; set; }

        public TextBlock ActionBlock { get; set; }

        private List<String> lastActions;


        private static readonly DateTime PreviewStartTime = new DateTime(2019, 08, 18, 11, 29, 15);

        private static readonly DateTime PreviewEndTime = new DateTime(2019, 08, 18, 11, 29, 29);

        public BiddingScreen(ProjectConfig conf, BidActionManager actionManager, Phase2ActManager phase2Manager, 
            SubmitStrategyManager strategyManager, PriceActionService priceActionService)
        {
            this.conf = conf;
            this.actionManager = actionManager;
            this.phase2Manager = phase2Manager;
            this.strategyManager = strategyManager;
            this.priceActionService = priceActionService;
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

            biddingContext = new BiddingContext();
            logger.InfoFormat("ResetContext last-sec is {0}", biddingContext.GetLastCalcedSec());

            ResetStrategyByReload();

            lastActions = new List<string>();
            SetLastAcionText();

            RefreshBiddingPage();

            IsPreviewDone = false;
            IsOneRoundStarted = false;

            logger.InfoFormat("ResetContext done");
        }

        public void ResetStrategyByReload()
        {
            biddingContext.Clean();

            List<SubmitPriceSetting> settings = this.strategyManager.LoadStrategies();
            foreach (SubmitPriceSetting sps in settings)
            {
                biddingContext.AddPriceSetting(sps);
            }

            biddingPriceManager = new BiddingPriceManager(strategyManager, biddingContext, CancelStrategyRequest);

            SetStrategyText();
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

        public BiddingContext GetBiddingContext()
        {
            return biddingContext;
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
                        AfterSuccessDetectInner(pp);
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
        
        /// <summary>
        /// 取消策略请求
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public bool CancelStrategyRequest(BiddingPriceRequest req)
        {
            // 取消之前的策略
            req.OperateStatus = StrategyOperateStatus.CANCELLED;

            // TODO: 应该实际取点击 取消 按钮
            return true;
        }
        
        public bool CaptchaAnswerInputCallback(CaptchaAnswerImage img)
        {
            // biddingPriceManager.InputAnswer(img.Uuid);

            var req = biddingPriceManager.GetRequestByUuid(img.Uuid);

            if (req != null && req.OperateStatus == StrategyOperateStatus.CAPTCHA_AWAIT)
            {
                phase2Manager.InputCaptchForSubmit(img.Answer);
                req.OperateStatus = StrategyOperateStatus.CAPTCHA_INPUTTED;
                logger.InfoFormat("strategy#{0} captcha-answer is inputted", req.StrategySecond);
            }

            return true;
        }

        private void AfterSuccessDetectInner(PagePrice pp)
        {
            // 1. 11:29:15 获取验证码 用于预览
            if (!IsPreviewDone && pp.pageTime.TimeOfDay >= PreviewStartTime.TimeOfDay && pp.pageTime.TimeOfDay <= PreviewEndTime.TimeOfDay)
            {
                PreviewPhase2Captcha(pp);
                // 第一次时，也重新加载策略，防止策略被刷掉
                this.ResetStrategyByReload();
                return;
            }

            // 本地 上报 29分25秒之后的价格
            if (isLegalMinte(pp.pageTime) && pp.pageTime.Second > 15)
            {
                AsyncReportPriceShow(pp, DateTime.Now);
            }

            AfterSuccessDetect(pp);
        }


        private bool isLegalMinte(DateTime dt)
        {
            return dt.Hour == 11 && dt.Minute == 29;
        }

        public void AfterSuccessDetect(PagePrice pp)
        {
            if (!isLegalMinte(pp.pageTime))
            {
                return;
            }

            // 如果该秒已经计算过
            if (!biddingContext.TryStartPagePrice(pp))
            {
                return;
            }

            long s1 = KK.CurrentMills();

            DateTime now = DateTime.Now;

            biddingContext.AddPrice(pp);

            var req = biddingPriceManager.Calcs(pp);

            if (req == null)
            {
                logger.InfoFormat("afterDetect elapsed {0}ms", KK.CurrentMills() - s1);
                return;
            }

            logger.InfoFormat("req is {0}", Jsons.ToJson(req));

            // 这里存在，正在输入价格或上传验证码时，会有来自价格同步的触发，存在并发场景
            // 所以先用了同步锁
            lock (logger)
            {
                if (req.OperateStatus == StrategyOperateStatus.NEED_OFFER_PRICE) 
                {
                    // 出价前 检查 距离上次 出价时间 是否在 6秒 内
                    // 如果在6秒内，则不能出价了，因为会报 操作频繁
                    List<BiddingPriceRequest> unSubmited = biddingPriceManager.GetPreviousUnSubmitRequest(pp.pageTime.Second);
                    bool canOffer = true;
                    if (unSubmited?.Count > 0)
                    {
                        var last = unSubmited.Last();
                        int duration = (int) (pp.pageTime - last.OfferedScreenTime).TotalSeconds;
                        if (duration < 6)
                        {
                            canOffer = false;
                        }
                        logger.InfoFormat("unSubmited between now duration is {0}, canOffer is {1}", duration, canOffer);
                    }

                    // 然后直接出价
                    if (canOffer)
                    {
                        // CaptchaAnswerImage ansImg = 
                        phase2Manager.OfferPrice(req.TargetPrice, true, (ansImg) => {
                            biddingContext.PutAwaitImage(ansImg, null);
                            req.CaptchaUuid = ansImg.Uuid;
                            return true;
                        });
                        
                        req.OperateStatus = StrategyOperateStatus.CAPTCHA_AWAIT;

                        biddingPriceManager.CancelnOutOfDateRequest(req.StrategySecond);

                        //
                        AsyncReportPriceOffered(pp, req.TargetPrice, DateTime.Now);
                    }
                    else
                    {
                        // 不能出价时，则取消该策略 TODO: 不能依赖 canOffer 做移除
                        logger.InfoFormat("strategy#{0} canOffer is false, need remove", req.StrategySecond);
                        biddingPriceManager.RemoveStrategy(req.StrategySecond);
                    }
                }
                else if (req.OperateStatus == StrategyOperateStatus.STRATEGY_RANGE_MATCHED) {
                    // 命中区间策略
                    // 移除该策略
                    logger.InfoFormat("range strategy#{0} matched, need remove", pp.pageTime.Second);
                    biddingPriceManager.RemoveStrategy(pp.pageTime.Second);
                }

                if (req.CanSubmit)
                {
                    req.SubmittedScreenTime = pp.pageTime;
                    req.SubmittedScreenPrice = pp.basePrice;
                    SubmitOfferedPrice(req);
                }

            }

            logger.InfoFormat("afterDetect elapsed {0}ms", KK.CurrentMills() - s1);
        }

        private void AsyncSetStrategyAndActionText()
        {
            ThreadUtils.StartNewTaskSafe(() =>
            {
                SetStrategyText();
                SetLastAcionText();
            });
        }

        public void SetStrategyText()
        {
            if (StrategyBlock == null)
            {
                return;
            }

            var strategyInfoText = biddingPriceManager.GetStrategyInfo();

            Action action1 = () =>
            {

                StrategyBlock.Text = strategyInfoText;
            };

            StrategyBlock.Dispatcher.BeginInvoke(action1);
        }

        private void SetLastAcionText()
        {
            if (ActionBlock == null || lastActions == null)
            {
                return;
            }

            var infoText = "";
            lastActions.ForEach(x =>
            {
                infoText += x + "\n";
            });


            Action action1 = () =>
            {
                ActionBlock.Text = infoText;
            };

            ActionBlock.Dispatcher.BeginInvoke(action1);
        }

        // private int SubmitOfferedPrice(int fixSec, PriceStrategyOperate oper, string answer, PagePrice pp, int offeredPrice, int usedDelayMills, DateTime occurTime)
        private int SubmitOfferedPrice(BiddingPriceRequest req)
        {
            bool submitted = false;
            string memo = "";
            DateTime occurTime = DateTime.Now;
            if (req.OperateStatus == StrategyOperateStatus.CAPTCHA_INPUTTED)
            {
                if (req.ComputedDelayMills > 0)
                {
                    KK.Sleep(req.ComputedDelayMills);
                }
                phase2Manager.SubmitOfferedPrice();
                submitted = true;

                logger.InfoFormat("strategy#{0} exec submit", req.StrategySecond);
            }
            else
            {
                string answer = CaptchaTaskContext.me.GetAnswer(req.CaptchaUuid);
                if (answer == null || answer.Length == 0)
                {
                    
                    submitted = false;
                    logger.ErrorFormat("strategy#{0} can not exec submit, because has no captcha-answer#{1}", req.StrategySecond, req.CaptchaUuid);

                    return -1;
                } else
                {
                    if (req.ComputedDelayMills > 0)
                    {
                        KK.Sleep(req.ComputedDelayMills);
                    }
                    phase2Manager.SubmitOfferedPrice(answer);

                    memo = "WiAns";

                    logger.WarnFormat("strategy#{0} exec submit and fill captcha-answer#{1}", req.StrategySecond, answer);
                }

                
            }

            if (submitted)
            {
                req.OperateStatus = StrategyOperateStatus.PRICE_SUBMITTED;
                biddingPriceManager.RemoveStrategy(req.StrategySecond);

                AsyncReportPriceSubbmitted(req, occurTime, memo);
            }
            
            return 0;
        }

        private void AsyncReportPrice(PriceAction act, int screenPrice, int targetPrice, DateTime screenTime, DateTime occurTime, string memo, int usedDelayMills = 0)
        {
            ThreadUtils.StartNewTaskSafe(() => {
                string actionText = null;
                if (act == PriceAction.PRICE_OFFER)
                {
                    priceActionService.ReportPriceOffered(screenPrice, targetPrice, screenTime, occurTime, memo);
                    actionText = screenTime.ToLongTimeString() + " - 出价: " + targetPrice;
                }
                else if (act == PriceAction.PRICE_SUBMIT)
                {
                    priceActionService.ReportPriceSubbmitted(screenPrice, targetPrice, screenTime, usedDelayMills, occurTime, memo);
                    actionText = screenTime + " - 提交";
                }
                else if (act == PriceAction.PRICE_SHOW)
                {
                    priceActionService.ReportPriceShowed(screenPrice, screenTime, occurTime, memo);
                }

                if (actionText != null)
                {
                    SetStrategyText();

                    lastActions.Add(actionText);
                    SetLastAcionText();
                }
                
            });

        }

        private void AsyncReportPriceSubbmitted(BiddingPriceRequest req, DateTime occurTime, string memo)
        {
            AsyncReportPrice(PriceAction.PRICE_SUBMIT, req.SubmittedScreenPrice, req.TargetPrice, req.SubmittedScreenTime, occurTime, memo, req.ComputedDelayMills);
        }

        private void AsyncReportPriceOffered(PagePrice pp, int offeredPrice, DateTime occurTime)
        {
            AsyncReportPrice(PriceAction.PRICE_OFFER, pp.basePrice, offeredPrice,  pp.pageTime, occurTime,  "offer");
        }

        private void AsyncReportPriceShow(PagePrice pp, DateTime occurTime)
        {
            AsyncReportPrice(PriceAction.PRICE_SHOW, pp.basePrice, 0, pp.pageTime, occurTime, "show");
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



        public void PreviewPhase2Captcha(PagePrice pp)
        {
            // TODO: 这里使用异步处理，否则出现不能显示验证码。
            // TODO: 这里可以归为一类问题：模拟时，必须等到所有操作才能显示页面。需要解决。

            logger.InfoFormat("Execute PreviewPhase2Captcha @{0}", pp.pageTime);

            phase2Manager.OfferPrice(pp.basePrice + 1500, false, (img) => {
                biddingContext.PutAwaitImage(img, null);
                Phase2PreviewCaptcha = img;

                phase2Manager.CancelOfferedPrice();
                return true;
            });

            // CaptchaAnswerImage img = null;
            //ThreadUtils.StartNewTaskSafe(() =>
            //{
            //    img = phase2Manager.OfferPrice(pp.basePrice + 1500, false);
            //    
            //});

            IsPreviewDone = true;

            //return img;
        }


    }

}

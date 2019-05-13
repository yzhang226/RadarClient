using log4net;
using RadarBidClient.common;
using RadarBidClient.ioc;
using RadarBidClient.model;
using RadarBidClient.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RadarBidClient.bidding
{
    [Component]
    public class BiddingScreen : InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BiddingScreen));

        private ProjectConfig conf;
        private BidActionManager actionManager;

        private BiddingContext biddingContext = new BiddingContext();

        public CaptchaAnswerImage Phase2PreviewCaptcha;

        private static bool IsCollectingWork = true;
        private static Thread collectingThread;
        private static Thread inquiryAnswerThread;

        private bool IsPreviewDone = false;

        private bool IsOneRoundStarted = false;

        private SubmitStrategyManager strategyManager;

        private WebBrowser webBro;

        private Phase2Manager phase2Manager;


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

        private int ForceStart = 500;
        private int ForceEnd = 1500;

        // private CaptchaAnswerImage LastImage;

        public BiddingScreen(ProjectConfig conf, BidActionManager actionManager, Phase2Manager phase2Manager, SubmitStrategyManager strategyManager)
        {
            this.conf = conf;
            this.actionManager = actionManager;
            this.phase2Manager = phase2Manager;
            this.strategyManager = strategyManager;
        }

        public static void StopCollectingWorkThread()
        {
            IsCollectingWork = false;
            // DispatcherTimer
            if (collectingThread != null)
            {
                KK.Sleep(1000);

                if ( (collectingThread.ThreadState & ThreadState.Running) == ThreadState.Running)
                {
                    collectingThread.Abort();
                }

                logger.InfoFormat("collectingThread.ThreadState is {0}", collectingThread.ThreadState);
            }

            if (inquiryAnswerThread != null)
            {
                KK.Sleep(1000);

                if ((inquiryAnswerThread.ThreadState & ThreadState.Running) == ThreadState.Running)
                {
                    inquiryAnswerThread.Abort();
                }

                logger.InfoFormat("inquiryAnswerThread.ThreadState is {0}", inquiryAnswerThread.ThreadState);
            }

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

        public void RefreshBiddingPage()
        {
            Action action1 = () =>
            {
                this.webBro.Refresh();
            };
            this.webBro.Dispatcher.BeginInvoke(action1);
        }


        private void SetShowUpText(string text)
        {

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
            if (collectingThread != null)
            {
                collectingThread.Abort();
                IsCollectingWork = false;
                KK.Sleep(1000);
            }

            if (inquiryAnswerThread != null)
            {
                inquiryAnswerThread.Abort();
                IsCollectingWork = false;
                KK.Sleep(1000);
            }


            IsCollectingWork = true;
            collectingThread = new Thread(loopDetectPriceAndTimeInScreen);
            collectingThread.IsBackground = true;
            collectingThread.Start();

            inquiryAnswerThread = new Thread(loodInquiryCaptchaAnswer);
            inquiryAnswerThread.IsBackground = true;
            inquiryAnswerThread.Start();
        }

        public void Reset()
        {
            biddingContext = new BiddingContext();
        }


        private void loopDetectPriceAndTimeInScreen()
        {
            logger.InfoFormat("begin loopDetectPriceAndTimeInScreen");
            int i = 0;

            PageTimePriceResult LastResultx = null;
            while (IsCollectingWork)
            {
                long ss = KK.currentTs();

                try
                {
                    long s1 = KK.currentTs();
                    PageTimePriceResult LastResult = actionManager.detectPriceAndTimeInScreen(LastResultx);

                    // 重复检测
                    if (LastResult.status == 300)
                    {
                        continue;
                    }

                    SetShowUpText(ToShowUpText(LastResult));

                    // 处理异常情况
                    if (LastResult.status != 0)
                    {
                        ProcessErrorDetect(LastResult);
                        continue;
                    }                    

                    LastResultx = LastResult;

                    PagePrice pp = LastResult.data;

                    logger.InfoFormat("detectPriceAndTimeInScreen elapsed {0}ms", KK.currentTs() - s1);

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

                        s1 = KK.currentTs();
                        AfterSuccessDetect(pp);
                        logger.DebugFormat("afterDetect elapsed {0}ms", KK.currentTs() - s1);
                    }

                }
                catch (Exception e)
                {
                    logger.Error("detect price and time error", e);
                }
                finally
                {
                    actionManager.ResetDictIndex();
                    KK.Sleep(100);
                }

                logger.DebugFormat("round {0} loopDetectPriceAndTimeInScreen elapsed {1}ms", i++, KK.currentTs() - ss);
            }

            logger.InfoFormat("END loopDetectPriceAndTimeInScreen ");
        }

        private string ToShowUpText(PageTimePriceResult LastResult)
        {
            string prefix = "本机时间：" + DateTime.Now.ToString("HH:mm:ss") + "。\n解析的内容是：";

            if (LastResult.status != 0)
            {
                return prefix + "ERROR: " + LastResult.status;
            }
            
            PagePrice pp = LastResult.data;

            return prefix + pp.pageTime.ToString("HH:mm:ss") + ".\n" + pp.low + " - " + pp.high + ".";
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

            long s1 = KK.currentTs();
            int sec = pp.pageTime.Second;
            int minute = pp.pageTime.Minute;
            DateTime now = DateTime.Now;

            var strats = new Dictionary<int, PriceSubmitOperate>(biddingContext.GetSubmitOperateMap());

            if (minute == 29)
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
                    int OfferedPrice = stra.GetOfferedPrice();
                    int PriceAt50 = biddingContext.GetPrice(50);
                    string answer = biddingContext.GetAnswer(oper.answerUuid);

                    // logger.InfoFormat("try target second#{0}, offer-price#{1}, delta#{2}. delta ", fixSec, OfferedPrice, (pp.basePrice + 300));

                    if (OfferedPrice <= (pp.basePrice + 300))
                    {
                        logger.InfoFormat("submit target second#{0}, offer-price#{1}, delta#{2}.", fixSec, OfferedPrice, stra.deltaPrice);

                        if (answer?.Length > 0)
                        {
                            SubmitOfferedPrice(fixSec, oper, answer);
                        }
                        else
                        {
                            // TODO: 到价时，依然未有验证码
                            // oper.status = 20;
                            logger.WarnFormat("target second#{0} has no captcha-answer", fixSec);
                        }
                        
                    }
                    else if (sec > 50 && PriceAt50 > 0 && OfferedPrice >= (PriceAt50 + DrawbackDeltaPrice + 100))
                    {
                        int PriceBack2 = biddingContext.GetPrice(sec - 2);
                        if (PriceBack2 > 0 && (pp.basePrice - PriceBack2) >= 200)
                        {
                            bool matched = false;
                            int delay = -1;
                            if (OfferedPrice == (pp.basePrice + 300 + 100)) // 提前 100
                            {
                                delay = KK.RandomInt(Delay1Start, Delay1End);
                                matched = true;
                            }
                            else if (OfferedPrice == (pp.basePrice + 300 + 200))
                            {
                                delay = KK.RandomInt(Delay2Start, Delay2End);
                                matched = true;
                            }
                            else if (OfferedPrice == (pp.basePrice + 300 + 300))
                            {
                                delay = KK.RandomInt(Delay3Start, Delay3End);
                                matched = true;
                            }

                            if (matched)
                            {
                                
                                KK.Sleep(delay);

                                SubmitOfferedPrice(fixSec, oper, answer);

                                logger.InfoFormat("matched second#{0}, delay#{1}, OfferedPrice#{2}, base-price#{3}.", fixSec, delay, OfferedPrice, pp.basePrice);
                            }

                        }
                    }
                    else if (sec > 50 && now.TimeOfDay > FinalTime)
                    {
                        int delay = KK.RandomInt(ForceStart, ForceEnd);
                        KK.Sleep(delay);

                        SubmitOfferedPrice(fixSec, oper, answer);

                        logger.InfoFormat("force submit second#{0}, delay#{1}, OfferedPrice#{2}, base-price#{3}.", fixSec, delay, OfferedPrice, pp.basePrice);
                    }


                }
                


            }

            logger.InfoFormat("afterDetect elapsed {0}ms", KK.currentTs() - s1);

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
                    logger.WarnFormat("target second#{0} has no captcha-answer", fixSec);
                    return -1;
                }

                logger.InfoFormat("submit target second#{0}, still not fill captcha answer#{1}", fixSec, answer);
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
                actionManager.findAndSetCoordOfCurrentTime();
            }
            else if (ret.status == -2)
            {
                actionManager.findAndSetCoordOfPriceSection();
            }
            else if (ret.status == -11)
            {
                actionManager.findAndSetCoordOfCurrentTime();
            }
            else if (ret.status == -12)
            {
                actionManager.findAndSetCoordOfPriceSection();
            }

        }


        /// <summary>
        /// 循环询问验证码图片的答案
        /// </summary>
        private void loodInquiryCaptchaAnswer()
        {
            while (IsCollectingWork)
            {
                long ss = KK.currentTs();
                try
                {
                    long s1 = KK.currentTs();
                    if (biddingContext.IsAllImagesAnswered())
                    {
                        KK.Sleep(100);
                        continue;
                    }

                    var images = new List<CaptchaAnswerImage>(biddingContext.GetImagesOfAwaitAnswer());

                    logger.InfoFormat("inquiry answer, image size is {0}. First Uuid is {1}", images.Count, images[0].Uuid);

                    foreach (var img in images)
                    {
                        if (img == null)
                        {
                            continue;
                        }
                        if (img.Answer == null || img.Answer.Length == 0)
                        {
                            var req = new CaptchaImageAnswerRequest();
                            req.from = "test";
                            req.token = "devJustTest";
                            req.uid = img.Uuid;
                            req.timestamp = KK.currentTs();

                            DataResult<CaptchaImageAnswerResponse> dr = RestClient
                                .PostAsJson<DataResult<CaptchaImageAnswerResponse>>(conf.CaptchaAddressPrefix + "/v1/biding/captcha-answer", req);

                            if (DataResults.isOk(dr) && dr.Data?.answer?.Length > 0)
                            {
                                biddingContext.PutAnswer(img.Uuid, dr.Data.answer);
                                biddingContext.RemoveAwaitImage(img.Uuid);

                                logger.InfoFormat("task#{0}'s answer is {1}", img.Uuid, dr.Data.answer);

                                // 尝试提前输入答案
                                var oper = biddingContext.GetSubmitOperateByUuid(img.Uuid);
                                if (oper != null && oper.status != 99)
                                {
                                    phase2Manager.InputCaptchForSubmit(dr.Data.answer);
                                    logger.InfoFormat("task#{0}'s answer#{1} is inputted", img.Uuid, dr.Data.answer);

                                    oper.status = 21;
                                }
                                
                            }
                        }
                        else
                        {
                            biddingContext.RemoveAwaitImage(img.Uuid);
                        }
                    }


                }
                catch (Exception e)
                {
                    logger.Error("detect price and time error", e);
                }
                finally
                {
                    KK.Sleep(50);
                }

            }

            logger.InfoFormat("END loodInquiryCaptchaAnswer ");

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

    [Component]
    public class SubmitStrategyManager : InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SubmitStrategyManager));

        private static readonly string StrategyFileName = "submitStrategy.txt";
        private static readonly string StrategyPath = "resource\\" + StrategyFileName;

        // private BiddingScreen screen;

        private FileSystemWatcher watcher = null;
        // BiddingScreen screen

        public SubmitStrategyManager()
        {
            // this.screen = screen;
        }

        public void AfterPropertiesSet()
        {
            this.WatchStragetyFile();
        }

        public List<SubmitPriceSetting> LoadStrategies()
        {
            string lines = FileUtils.readTxtFile(StrategyPath);

            List<SubmitPriceSetting> settings = new List<SubmitPriceSetting>();
            string[] lis = lines.Split('\n');
            foreach (string li in lis)
            {
                logger.InfoFormat("load submit setting {0}", li);

                if (li == null || li.Trim().StartsWith("#"))
                {
                    continue;
                }

                var sps = SubmitPriceSetting.fromLine(li.Trim());
                if (sps != null)
                {
                    settings.Add(sps);
                }
            }

            return settings;
        }

        private void WatchStragetyFile()
        {
            logger.InfoFormat("start watch directory#{0}, strategy-file#{1}", KK.ResourceDir(), StrategyFileName);

            if (watcher != null)
            {
                watcher.Dispose();
                logger.InfoFormat("Dispose previous watcher#{0}", watcher);
            }

            watcher = new FileSystemWatcher();
            watcher.Path = KK.ResourceDir();
            watcher.Filter = StrategyFileName;
            watcher.Changed += new FileSystemEventHandler(OnProcess);
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            watcher.IncludeSubdirectories = false;
        }

        private void OnProcess(object source, FileSystemEventArgs e)
        {
            logger.InfoFormat("File#{0} with change#{1}, {2}.", e.FullPath, e.ChangeType, e.Name);

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                // screen.ResetStrategyByReload();
            }
        }

    }

}

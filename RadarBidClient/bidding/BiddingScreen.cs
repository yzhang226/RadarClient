using log4net;
using RadarBidClient.common;
using RadarBidClient.ioc;
using RadarBidClient.model;
using RadarBidClient.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RadarBidClient.bidding
{
    [Component]
    public class BiddingScreen : InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BiddingScreen));

        private static readonly string submitStrategyPath = "resource\\submitStrategy.txt";

        private ProjectConfig conf;
        private BidActionManager actionManager;

        private BiddingContext biddingContext = new BiddingContext();
        private List<SubmitPriceSetting> settings = new List<SubmitPriceSetting>();
        private Dictionary<int, SubmitPriceSetting> strategyMap = new Dictionary<int, SubmitPriceSetting>();

        public CaptchaAnswerImage Phase2PreviewCaptcha;

        private Thread collectingThread;
        private bool IsCollectingWork = true;
        private Thread inquiryAnswerThread;

        // private CaptchaAnswerImage LastImage;

        public BiddingScreen(ProjectConfig conf, BidActionManager actionManager)
        {
            this.conf = conf;
            this.actionManager = actionManager;
        }

        public void AfterPropertiesSet()
        {
            if (conf.EnableAutoBidding)
            {
                this.StartWork();
            }
        }

        public void StartWork()
        {
            biddingContext = new BiddingContext();

            // 
            string lines = FileUtils.readTxtFile(submitStrategyPath);

            // logger.InfoFormat("load Price Submit Strategy: {0}", lines);

            string[] lis = lines.Split('\n');
            foreach (string li in lis)
            {
                logger.InfoFormat("load price setting {0}", li);
                var sps = SubmitPriceSetting.fromLine(li);
                if (sps != null)
                {
                    this.settings.Add(sps);
                }
            }

            foreach (SubmitPriceSetting sps in this.settings)
            {
                strategyMap[sps.second] = sps;
            }


            StartCollectingThread();
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

            PageTimePriceResult LastResult = null;
            while (IsCollectingWork)
            {
                long ss = KK.currentTs();

                try
                {
                    long s1 = KK.currentTs();
                    LastResult = actionManager.detectPriceAndTimeInScreen(LastResult);

                    // 重复检测
                    if (LastResult.status == 300)
                    {

                        continue;
                    }

                    // 处理异常情况
                    if (LastResult.status != 0)
                    {
                        ProcessErrorDetect(LastResult);
                        continue;
                    }

                    PagePrice pp = LastResult.data;

                    logger.InfoFormat("detectPriceAndTimeInScreen elapsed {0}ms", KK.currentTs() - s1);

                    if (pp != null)
                    {


                        s1 = KK.currentTs();
                        AfterSuccessDetect(pp);
                        logger.DebugFormat("afterDetect elapsed {0}ms", KK.currentTs() - s1);

                        var dt = pp.pageTime;
                        if ((dt.Minute == 28 && dt.Second == 59)
                            || (dt.Minute == 29 && dt.Second == 59)
                            || (dt.Minute == 28 && dt.Second == 0)
                            || (dt.Minute == 29 && dt.Second == 0)
                            )
                        {
                            // this.initDoneStatus();
                        }
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

            logger.InfoFormat("end loopDetectPriceAndTimeInScreen ");
        }


        private void AfterSuccessDetect(PagePrice pp)
        {

            // 1. 11:29:15 获取验证码 用于预览
            if (pp.pageTime.TimeOfDay >= PreviewStartTime && pp.pageTime.TimeOfDay <= PreviewEndTime)
            {
                if (Phase2PreviewCaptcha == null)
                {
                    // CaptchaAnswerImage img = 
                        PreviewPhase2Captcha(pp);
                    
                    return;
                }
            }

            long s1 = KK.currentTs();
            int sec = pp.pageTime.Second;
            int minute = pp.pageTime.Minute;
            foreach (var item in strategyMap)
            {
                int fixSec = item.Key;

                SubmitPriceSetting stra = item.Value;
                int fixMinute = stra.minute > 0 ? stra.minute : 29;

                if (fixMinute != minute)
                {
                    continue;
                }


                if (sec == fixSec)
                {
                    stra.basePrice = pp.basePrice;
                    logger.InfoFormat("set detected base-price {0} {1}, {2}", fixMinute, sec, pp.basePrice);
                }



                if (sec >= fixSec)// && !doneStatus[fixSec]
                {
                    int targetPrice = pp.basePrice + stra.deltaPrice;
                    if (pp.high >= targetPrice)
                    {
                        logger.InfoFormat("find target price {0}, {1}", sec, stra.deltaPrice, targetPrice);
                        actionManager.MockPhase022(targetPrice, biddingContext);
                        // doneStatus[fixSec] = true;
                    }
                }

            }

            logger.InfoFormat("afterDetect elapsed {0}ms", KK.currentTs() - s1);

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

        }


        private TimeSpan PreviewStartTime = new TimeSpan(11, 29, 15);

        private TimeSpan PreviewEndTime = new TimeSpan(11, 29, 29);

        public CaptchaAnswerImage PreviewPhase2Captcha(PagePrice pp)
        {

            // TODO: 这里使用异步处理，否则出现不能显示验证码。
            // TODO: 这里可以归为一类问题：模拟时，必须等到所有操作才能显示页面。需要解决。

            CaptchaAnswerImage img = null;
            Task.Factory.StartNew(() => {
                actionManager.MockPhase2AtCaptcha(pp.basePrice + 1500);
                // 需要等待一会 验证码才会出现
                // KK.Sleep(100);
                KK.Sleep(100);
                img = actionManager.CaptureCaptchaAndUploadTask();

                KK.Sleep(500);
                actionManager.MockCancelPhase2AtCaptcha();

                biddingContext.PutAwaitImage(img);
                Phase2PreviewCaptcha = img;
            });
            

            return img;
        }


    }
}

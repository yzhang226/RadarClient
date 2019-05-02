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

namespace RadarBidClient.bidding
{
    [Component]
    public class BiddingScreen : InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BiddingScreen));

        private static readonly string submitStrategyPath = "submitStrategy.txt";

        private ProjectConfig conf;
        private BidActionManager actionManager;

        private BiddingContext biddingContext = null;
        private List<SubmitPriceSetting> settings = new List<SubmitPriceSetting>();
        private Dictionary<int, SubmitPriceSetting> strategyMap = new Dictionary<int, SubmitPriceSetting>();

        private Thread collectingThread;
        private bool IsCollectingWork = true;
        private Thread inquiryAnswerThread;

        private CaptchaAnswerImage LastImage;

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

        public void SetLastCaptchaAnswerImage(CaptchaAnswerImage LastImage)
        {
            this.LastImage = LastImage;
        }

        public void StartWork()
        {
            biddingContext = new BiddingContext();

            StartCollectingThread();

            // 
            string lines = FileUtils.readTxtFile(submitStrategyPath);
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

            while (IsCollectingWork)
            {
                long ss = KK.currentTs();

                try
                {
                    long s1 = KK.currentTs();
                    PageTimePriceResult ret = actionManager.detectPriceAndTimeInScreen();

                    // 处理异常情况
                    if (ret.status != 0)
                    {
                        ProcessErrorDetect(ret);
                        continue;
                    }

                    PagePrice pp = ret.data;

                    logger.DebugFormat("detectPriceAndTimeInScreen elapsed {0}ms", KK.currentTs() - s1);

                    if (pp != null)
                    {
                        s1 = KK.currentTs();
                        afterDetect(pp);
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
                    KK.Sleep(100);
                }

                logger.DebugFormat("round {0} loopDetectPriceAndTimeInScreen elapsed {1}ms", i++, KK.currentTs() - ss);
            }

            logger.InfoFormat("end loopDetectPriceAndTimeInScreen ");
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
                    if (LastImage == null)
                    {
                        KK.Sleep(300);
                        continue;
                    }

                    if (LastImage.Answer?.Length > 0)
                    {
                        KK.Sleep(300);
                        continue;
                    }

                    var req = new CaptchaImageAnswerRequest();
                    req.from = "test";
                    req.token = "devJustTest";
                    req.uid = LastImage.Uuid;
                    req.timestamp = KK.currentTs();

                    int httpStatus;

                    DataResult<CaptchaImageAnswerResponse> dr = RestClient.PostAsJson<DataResult<CaptchaImageAnswerResponse>>(conf.CaptchaAddressPrefix + "/v1/biding/captcha-answer", req);

                    if (DataResults.isOk(dr) && dr.Data?.answer?.Length > 0)
                    {
                        LastImage.Answer = dr.Data.answer;
                        logger.InfoFormat("task#{0}'s answer is {1}", LastImage.Uuid, dr.Data.answer);
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

        private TimeSpan AnswerStartTime = new TimeSpan(11, 29, 15);

        private void afterDetect(PagePrice pp)
        {

            // 1. 11:29:15 获取验证码
            if (pp.pageTime.TimeOfDay >= AnswerStartTime)
            {
                if (biddingContext.Phase2Answer == null)
                {
                    CaptchaAnswerImage img = actionManager.CapturePhase2CaptchaImage();
                    // img.ImagePath;

                    // RestClient.PostWebAPI("", "");

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

                //if (fixMinute != minute)
                //{
                //    continue;
                //}


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
                        actionManager.MockPhase022(targetPrice);
                        // doneStatus[fixSec] = true;
                    }
                }

            }

            logger.InfoFormat("afterDetect elapsed {0}ms", KK.currentTs() - s1);

        }

    }
}

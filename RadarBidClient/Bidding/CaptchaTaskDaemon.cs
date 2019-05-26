using log4net;
using Radar.Common;
using Radar.IoC;
using Radar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Radar.Bidding
{

    [Component]
    public class CaptchaTaskDaemon
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CaptchaTaskDaemon));

        private ProjectConfig conf;
        private Phase2ActManager phase2ActManager;

        private static bool isInquiryWork = false;
        private static Thread inquiryThread;

        public CaptchaTaskDaemon(ProjectConfig conf, Phase2ActManager phase2ActManager)
        {
            this.conf = conf;
            this.phase2ActManager = phase2ActManager;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RestartInquiryThread()
        {
            this.StopInquiryThread();

            isInquiryWork = true;
            inquiryThread = Radar.Common.Threads.Threads.StartNewBackgroudThread(LoopInquiryCaptchaAnswer);
            logger.InfoFormat("Restart CaptchaTask to Inquiry Answer");
        }

        public void StopInquiryThread()
        {
            isInquiryWork = false;
            Radar.Common.Threads.Threads.TryStopThreadByWait(inquiryThread, 60, 30, "inquiryThread");
        }

        /// <summary>
        /// 循环询问验证码图片的答案
        /// </summary>
        private void LoopInquiryCaptchaAnswer()
        {
            while (isInquiryWork)
            {
                long ss = KK.CurrentMills();
                try
                {
                    var taskContext = Radar.Bidding.Model.CaptchaTaskContext.me;
                    long s1 = KK.CurrentMills();
                    if (Radar.Bidding.Model.CaptchaTaskContext.me.IsAllImagesAnswered())
                    {
                        KK.Sleep(30);
                        continue;
                    }

                    var images = new List<Radar.Bidding.Model.CaptchaAnswerImage>(taskContext.GetImagesOfAwaitAnswer());

                    logger.DebugFormat("inquiry answer, image size is {0}. First Uuid is {1}", images.Count, images[0].Uuid);

                    foreach (var img in images)
                    {
                        if (img == null)
                        {
                            continue;
                        }

                        if (img.Answer == null || img.Answer.Length == 0)
                        {
                            var req = KK.CreateImageAnswerRequest(img.Uuid);

                            Radar.Bidding.Model.DataResult<Radar.Bidding.Model.CaptchaImageAnswerResponse> dr = HttpClients
                                .PostAsJson<Radar.Bidding.Model.DataResult<Radar.Bidding.Model.CaptchaImageAnswerResponse>>(conf.CaptchaAddressPrefix + "/v1/biding/captcha-answer", req);

                            if (DataResults.IsOK(dr) && dr.Data?.answer?.Length > 0)
                            {
                                taskContext.PutAnswer(img.Uuid, dr.Data.answer);
                                taskContext.RemoveAwaitImage(img.Uuid);

                                logger.InfoFormat("GET task#{0}'s answer is {1}", img.Uuid, dr.Data.answer);

                                // TODO：这段应该剥离出去
                                TryInputAnswerAhead(img.Uuid, dr.Data.answer);

                            }
                        }
                        else
                        {
                            taskContext.RemoveAwaitImage(img.Uuid);
                        }
                    }

                }
                catch (Exception e)
                {
                    logger.Error("LoopInquiryCaptchaAnswer error:", e);
                }
                finally
                {
                    KK.Sleep(30);
                }

            }

            logger.InfoFormat("END LoopInquiryCaptchaAnswer.");

        }

        /// <summary>
        /// 尝试提前输入答案
        /// </summary>
        /// <param name="imageUuid"></param>
        /// <param name="answer"></param>
        private void TryInputAnswerAhead(string imageUuid, string answer)
        {
            var oper = Radar.Bidding.Model.BiddingContext.GetSubmitOperateByUuid(imageUuid);
            if (oper != null && oper.status != 99)
            {
                phase2ActManager.InputCaptchForSubmit(answer);
                oper.status = 21;
            }
        }
    }
}

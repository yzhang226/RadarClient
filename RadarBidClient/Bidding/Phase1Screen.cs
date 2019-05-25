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
    public class Phase1Screen : InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Phase1Screen));

        private Phase1Manager phase1Manager;

        private bool isAwaitWork;
        private Thread awaitThread;

        public Phase1Screen(Phase1Manager phase1Manager)
        {
            this.phase1Manager = phase1Manager;
        }

        public void FirstOfferPrice(int price)
        {
            logger.InfoFormat("第一阶段出价 - 开始");

            CaptchaAnswerImage img = phase1Manager.OfferPrice(price, true);
            StartAwaitAnswerToSubmit(img.Uuid);

            logger.InfoFormat("第一阶段出价 - 等待验证码提交");
        }

        public void StartAwaitAnswerToSubmit(string imageUuid)
        {
            StopAwaitThread();

            isAwaitWork = true;
            awaitThread = Threads.StartNewBackgroudThread(() =>
            {
                LoopAwaitAnswer(imageUuid);
            });
            logger.InfoFormat("Restart CaptchaTask to Inquiry Answer");
        }

        private void LoopAwaitAnswer(string imageUuid)
        {
            logger.InfoFormat("Phase1 - begin LoopAwaitAnswer");

            while (isAwaitWork)
            {
                long ss = KK.CurrentMills();
                try
                {
                    var taskContext = CaptchaTaskContext.me;
                    var answer = taskContext.GetAnswer(imageUuid);
                    if (answer?.Length > 0)
                    {
                        phase1Manager.SubmitOfferedPrice(answer);
                        break;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("LoopInquiryCaptchaAnswer error:", e);
                }
                finally
                {
                    KK.Sleep(100);
                }

            }

            logger.InfoFormat("Phase1 - end LoopAwaitAnswer");
        }

        public void StopAwaitThread()
        {
            isAwaitWork = false;
            Threads.TryStopThreadByWait(awaitThread, 100, 100, "phase1-awaitThread");
        }

        public void AfterPropertiesSet()
        {
            
        }
    }
}

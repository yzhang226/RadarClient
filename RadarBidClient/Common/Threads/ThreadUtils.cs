using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Radar.Common.Threads
{

    public class ThreadUtils
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ThreadUtils));

        /// <summary>
        /// 开启一个背景线程
        /// </summary>
        /// <param name="tStart"></param>
        /// <returns></returns>
        public static Thread StartNewBackgroudThread(ThreadStart tStart)
        {
            Thread thr = new Thread(tStart) { IsBackground = true };
            thr.Start();

            return thr;
        }

        /// <summary>
        /// 开启一个背景线程, 带线程参数
        /// </summary>
        /// <param name="ptStart"></param>
        /// <returns></returns>
        public static Thread StartNewBackgroundThread(ParameterizedThreadStart ptStart, object param)
        {
            Thread thr = new Thread(ptStart) { IsBackground = true };
            thr.Start(param);

            return thr;
        }

        public static void TryStopThread(Thread th)
        {
            if ((th.ThreadState & ThreadState.Running) == ThreadState.Running)
            {
                th.Abort();
            }
        }

        public static void TryStopThreadByWait(Thread th, int mills1, int mills2, string memo)
        {
            if (th != null)
            {
                KK.Sleep(mills1);

                ThreadUtils.TryStopThread(th);
                KK.Sleep(mills2);
                logger.InfoFormat("NOW {0}#State is {1}", memo, th.ThreadState);
            }
        }

    }

}

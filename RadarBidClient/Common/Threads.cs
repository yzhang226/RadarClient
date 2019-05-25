using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Radar.Common
{
    public class SingleThread
    {
        // 
        private static readonly ILog logger = LogManager.GetLogger(typeof(SingleThread));

        static SingleThread()
        {
            // ThreadPool.SetMinThreads(1, 1);
            // ThreadPool.SetMaxThreads(1, 1);
            // logger.InfoFormat("set ThreadPool min and max to {0}", 1);
        }

        private SingleThread()
        {
            
        }

        public static bool Execute(UserTask task)
        {
            ClientHandle ch = CustomThreadPool.Instance.QueueUserTask(task);
            
            return true;
        }

        public static bool Execute(UserTask task, object state, Action<TaskStatus> callback)
        {
            ClientHandle ch = CustomThreadPool.Instance.QueueUserTask(task, state, callback);

            return true;
        }

    }

    public class Threads
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Threads));

        /// <summary>
        /// 开启一个背景线程
        /// </summary>
        /// <param name="tStart"></param>
        /// <returns></returns>
        public static Thread StartNewBackgroudThread(ThreadStart tStart)
        {
            Thread thr = new Thread(tStart);
            thr.IsBackground = true;
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
            Thread thr = new Thread(ptStart);
            thr.IsBackground = true;
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

                Threads.TryStopThread(th);
                KK.Sleep(mills2);
                logger.InfoFormat("NOW {0}#State is {1}", memo, th.ThreadState);
            }
        }

    }

}

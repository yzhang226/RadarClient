using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RadarBidClient.common
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
}

using log4net;
using Radar.Bidding.Service;
using Radar.Common;
using Radar.Common.Raw;
using Radar.Common.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Radar.Bidding.Net
{
    public class KeepAliveManager
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(KeepAliveManager));

        public static readonly KeepAliveManager ME = new KeepAliveManager();

        private Thread socketWatchThrad = null;

        private bool watching = true;

        /// <summary>
        /// 通知重启中 
        /// </summary>
        private bool notifyRestarting = false;

        /// <summary>
        /// 连续 未连接 次数
        /// </summary>
        private int continuousDisconnectedCount = 0;
        
        private static readonly int DETECT_INTERVAL_MILLS = 2300;
        
        private SocketClient socketClient;

        private PingPongCounter counter;

        public KeepAliveManager()
        {
            this.counter = PingPongCounter.ME;
        }

        public void RestartGuard(SocketClient socketClient)
        {
            // shutdown previous guard
            this.ShutdownGuard();

            counter.Reset();


            logger.InfoFormat("begin StartSocketClientGuard，continuousDisconnectedCount is {0}", continuousDisconnectedCount);
            this.socketClient = socketClient;
            watching = true;
            notifyRestarting = false;
            socketWatchThrad = ThreadUtils.StartNewBackgroudThread(WatchSocketClient);
            logger.InfoFormat("end StartSocketClientGuard");
        }

        private void ShutdownGuard()
        {
            logger.InfoFormat("begin ShutdownGuard");

            watching = false;

            if (socketWatchThrad != null)
            {
                ThreadUtils.TryStopThreadByWait(socketWatchThrad, DETECT_INTERVAL_MILLS + 500, 300, "SocketWatchThrad");
            }

            this.socketClient = null;

            logger.InfoFormat("end ShutdownGuard");
        }


        private void WatchSocketClient()
        {
            KK.Sleep(3 * 1000);

            while (watching)
            {
                KK.Sleep(DETECT_INTERVAL_MILLS);

                if (IsCurrentInDisabledTimeRange())
                {
                    continue;
                }

                try
                {
                    if (notifyRestarting)
                    {
                        logger.InfoFormat("already tell socket client to reconnect");
                        continue;
                    }

                    bool connected = socketClient.IsConnected();
                    // 如果已未连接，则通知重新连接
                    if (!connected)
                    {
                        TellToReconnect("already disconnected");
                        continue;
                    }
                    
                    RawMessage msg = MessageUtils.BuildPingMessage(ClientService.AssignedClientNo);
                    socketClient.Send(msg);
                    continuousDisconnectedCount = 0;
                    counter.IncrementPing();

                    long pingCnt = counter.GetPingCount();
                    long pongCnt = counter.GetPongCount();

                    DateTime dt = DateTime.Now;
                    if (dt.Minute > 57 && dt.Second % 8 == 0)
                    {
                        logger.InfoFormat("dice socket connected#{0}, ping#{1}, pong#{2}", connected, pingCnt, pongCnt);
                    }

                    // 如果ping和pong相差超过三次，则通知重新连接
                    long delta = pingCnt - pongCnt;
                    if (delta > 3)
                    {
                        TellToReconnect("already over " + delta +  " no pong response from server");
                    }

                }
                catch (Exception e)
                {
                    logger.Error("WatchSocketClient error", e);
                }
                finally
                {

                }
            }

        }

        private void TellToReconnect(string reason)
        {
            continuousDisconnectedCount++;
            // 每3次 未连接，则多停一会
            if (continuousDisconnectedCount % 3 == 0)
            {
                logger.InfoFormat("continuous disconnected count over 3 times, disconnect count is {0}", continuousDisconnectedCount);
                KK.Sleep(DETECT_INTERVAL_MILLS * 6);
            }

            if (notifyRestarting)
            {
                logger.InfoFormat("already tell socket client to reconnect");
            }
            else
            {
                logger.WarnFormat("tell try to reconnect, reason is {0}", reason);
                ThreadUtils.StartNewBackgroudThread(() => {
                    socketClient.RestartClient();
                });
                notifyRestarting = true;
            }
        }

        /// <summary>
        /// 当前是否属于 禁用时间段: 11:27 - 11:31
        /// </summary>
        /// <returns></returns>
        public bool IsCurrentInDisabledTimeRange()
        {
            DateTime dt = DateTime.Now;
            return dt.Hour == 11 && dt.Minute >= 27 && dt.Minute <= 31;
        }

    }
}

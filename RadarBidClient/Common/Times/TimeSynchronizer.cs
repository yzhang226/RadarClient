using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common.Times
{
    public class TimeSynchronizer
    {
        /// <summary>
        /// 从 时间服务器 中，同步时间
        /// </summary>
        /// <returns></returns>
        public static bool SyncFromNtpServer()
        {
            DateTime serverDt = NtpTime.GetNetworkTimeFromList();
            return SystemTimeUpdater.SetDate(serverDt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool SyncDateTime(DateTime dt)
        {
            return SystemTimeUpdater.SetDate(dt);
        }

    }
}

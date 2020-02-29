using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common.Times
{
    public class TimeSynchronizer
    {
        /// <summary>
        /// �� ʱ������� �У�ͬ��ʱ��
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

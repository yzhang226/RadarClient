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
            DateTime serverDt = Radar.Common.Times.NtpTime.GetNetworkTimeFromList();
            return SystemTimeUpdater.SetDate(serverDt);
        }

    }
}

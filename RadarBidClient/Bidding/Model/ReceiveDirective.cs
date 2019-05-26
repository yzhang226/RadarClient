using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    /// <summary>
    /// ����ָ�� - ����ָ��
    /// </summary>
    public enum ReceiveDirective
    {
        /// <summary>
        /// ͬ��NTP������ʱ��
        /// </summary>
        SYNC_SYSTEM_TIME = 90100,

        /// <summary>
        /// ��ͼ���ϴ�flash��Ļ
        /// </summary>
        CAPTURE_BID_SCREEN = 90201,

        /// <summary>
        /// ��ͼ���ϴ�flash��Ļ
        /// </summary>
        UPLOAD_BID_SCREEN = 90202,

        /// <summary>
        /// ��ͼ���ϴ�flash��Ļ
        /// </summary>
        CAPTURE_UPLOAD_BID_SCREEN = 90203,

    }
}

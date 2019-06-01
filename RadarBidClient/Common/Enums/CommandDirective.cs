using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common.Enums
{
    /// <summary>
    /// ����ָ�� 
    /// </summary>
    public enum CommandDirective
    {
        /// <summary>
        /// �ͻ��˵�¼ - ����
        /// </summary>
        CLIENT_LOGIN = 60060,

        /// <summary>
        /// �ͻ��˵�¼ - ��Ӧ
        /// </summary>
        RESP_CLIENT_LOGIN = 60061,


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

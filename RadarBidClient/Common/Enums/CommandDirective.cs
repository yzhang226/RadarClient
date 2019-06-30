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
        /// ��ʾ��
        /// </summary>
        NONE = 0,

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

        /// <summary>
        /// �ƶ����
        /// </summary>
        MOVE_CURSOR = 90300,

        /// <summary>
        /// ������
        /// </summary>
        LEFT_CLICK = 90301,

        /// <summary>
        /// �����ı�
        /// </summary>
        INPUT_TEXT = 90302,

        /// <summary>
        /// ָ��ű������ָ��ļ��ϣ�
        /// </summary>
        DIRECTIVE_SCRIPT = 90400,

        /// <summary>
        /// ֪ͨ�ͻ��˼۸�
        /// </summary>
        CLIENT_PRICE_TELL = 90500,

    }
}
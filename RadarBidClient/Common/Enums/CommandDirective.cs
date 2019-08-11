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
        /// �ͻ���ע�� - ����
        /// </summary>
        CLIENT_REGISTER = 60060,

        /// <summary>
        /// �ͻ���ע�� - ��Ӧ
        /// </summary>
        RESP_REGISTER_LOGIN = 60061,


        /// <summary>
        /// ͬ��NTP������ʱ��
        /// </summary>
        SYNC_SYSTEM_TIME = 90100,

        /// <summary>
        /// ��ͼflash��Ļ
        /// </summary>
        CAPTURE_BID_SCREEN = 90201,

        /// <summary>
        /// �ϴ�flash��Ļ
        /// </summary>
        UPLOAD_BID_SCREEN = 90202,

        /// <summary>
        /// ��ͼ���ϴ�flash��Ļ
        /// </summary>
        CAPTURE_UPLOAD_BID_SCREEN = 90203,

        /// <summary>
        /// ��ͼ���ϴ�ȫ��Ļ
        /// </summary>
        CAPTURE_UPLOAD_FULL_SCREEN = 90204,

        /// <summary>
        /// �ϴ�ѹ����־�ļ�
        /// </summary>
        UPLOAD_ZIPPED_LOG_FILE = 90210,

        /// <summary>
        /// �ƶ����
        /// </summary>
        MOVE_CURSOR = 90300,

        /// <summary>
        /// �ƶ���� �� ������
        /// </summary>
        MOVE_LEFT_CLICK = 90301,

        /// <summary>
        /// �ƶ���� �� ������ �� �����ı�
        /// </summary>
        MOVE_CLICK_INPUT_TEXT = 90302,

        /// <summary>
        /// ָ��ű������ָ��ļ��ϣ�
        /// </summary>
        DIRECTIVE_SCRIPT = 90400,

        /// <summary>
        /// ֪ͨ�ͻ��˼۸�
        /// </summary>
        CLIENT_PRICE_TELL = 90500,


        /// <summary>
        /// �����˺ŵ�¼
        /// </summary>
        BID_ACCOUNT_LOGIN = 90501,


        /// <summary>
        /// ��һ�׶μ۸����
        /// </summary>
        PHASE1_PRICE_OFFER = 90601,

        /// <summary>
        /// ��һ�׶μ۸��ύ
        /// </summary>
        PHASE1_PRICE_SUBMIT = 90602,

        /// <summary>
        /// ���þ��Ĳ���
        /// </summary>
        BID_STRATEGIES_SET = 90603,



    }
}

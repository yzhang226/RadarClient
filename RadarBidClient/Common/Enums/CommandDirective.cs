using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common.Enums
{
    /// <summary>
    /// 控制指定 
    /// </summary>
    public enum CommandDirective
    {
        /// <summary>
        /// 表示空
        /// </summary>
        NONE = 0,

        /// <summary>
        /// 客户端登录 - 请求
        /// </summary>
        CLIENT_LOGIN = 60060,

        /// <summary>
        /// 客户端登录 - 响应
        /// </summary>
        RESP_CLIENT_LOGIN = 60061,


        /// <summary>
        /// 同步NTP服务器时间
        /// </summary>
        SYNC_SYSTEM_TIME = 90100,

        /// <summary>
        /// 截图且上传flash屏幕
        /// </summary>
        CAPTURE_BID_SCREEN = 90201,

        /// <summary>
        /// 截图且上传flash屏幕
        /// </summary>
        UPLOAD_BID_SCREEN = 90202,

        /// <summary>
        /// 截图且上传flash屏幕
        /// </summary>
        CAPTURE_UPLOAD_BID_SCREEN = 90203,

        /// <summary>
        /// 移动鼠标
        /// </summary>
        MOVE_CURSOR = 90300,

        /// <summary>
        /// 左键点击
        /// </summary>
        LEFT_CLICK = 90301,

        /// <summary>
        /// 输入文本
        /// </summary>
        INPUT_TEXT = 90302,

        /// <summary>
        /// 指令脚本（多个指令的集合）
        /// </summary>
        DIRECTIVE_SCRIPT = 90400,

        /// <summary>
        /// 通知客户端价格
        /// </summary>
        CLIENT_PRICE_TELL = 90500,

    }
}

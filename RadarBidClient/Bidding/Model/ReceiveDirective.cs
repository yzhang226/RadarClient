using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    /// <summary>
    /// 控制指定 - 接收指令
    /// </summary>
    public enum ReceiveDirective
    {
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

    }
}

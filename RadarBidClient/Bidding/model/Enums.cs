using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model
{
    public class Enums
    {
    }

    /// <summary>
    /// 控制指定
    /// </summary>
    public enum ControlDirective
    {

        /// <summary>
        /// 账号登录
        /// </summary>
        ACCOUNT_LOGIN = 100,

        /// <summary>
        /// 第一阶段出价
        /// </summary>
        PHASE_ONE_OFFER_PRICE = 200,

        /// <summary>
        /// 第二阶段出价
        /// </summary>
        PHASE_TWO_OFFER_PRICE = 210,

        /// <summary>
        /// 同步NTP服务器时间
        /// </summary>
        SYNC_SYSTEM_TIME = 90100,

        /// <summary>
        /// 截图且上传flash屏幕
        /// </summary>
        CAPTURE_UPLOAD_BID_SCREEN = 90200,

    }




}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Common.Enums
{
    /// <summary>
    /// 策略操作状态
    /// </summary>
    public enum StrategyOperateStatus
    {

        /// <summary>
        /// 需要出价 但是还未出价. 初始化，未执行
        /// </summary>
        NEED_OFFER_PRICE = 0,

        /// <summary>
        /// 出价完成
        /// </summary>
        PRICE_OFFERED = 1,

        /// <summary>
        /// 提交完成
        /// </summary>
        PRICE_SUBMITTED = 2,

        /// <summary>
        /// 等待验证码. 包含 至少 出价完成 
        /// </summary>
        CAPTCHA_AWAIT = 20,

        /// <summary>
        /// 验证码输入完成
        /// </summary>
        CAPTCHA_INPUTTED = 21,

        /// <summary>
        /// 已取消
        /// </summary>
        CANCELLED = 99,



    }
}

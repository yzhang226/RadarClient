using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Common.Enums
{
    public enum PriceAction
    {

        /// <summary>
        ///  价格 - 显示
        /// </summary>
        PRICE_SHOW = 1,

        /// <summary>
        /// 价格 - 出价
        /// </summary>
        PRICE_OFFER = 5,

        /// <summary>
        /// 价格 - 提交
        /// </summary>
        PRICE_SUBMIT = 6,

        /// <summary>
        /// 价格 - 重新提交
        /// </summary>
        CANCEL_AND_REOFFER = 1000,

    }
}

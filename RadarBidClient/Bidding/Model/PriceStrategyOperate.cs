using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class PriceStrategyOperate
    {
        public SubmitPriceSetting setting;

        public string answerUuid;

        /// <summary>
        /// 0 - 出价完成
        /// 1 - 提交完成
        /// 20 - 等待验证码
        /// 21 - 验证码输入完成
        /// 99 - Cancelled
        /// -1 - 未执行
        /// </summary>
        public int status = -1;

    }
}

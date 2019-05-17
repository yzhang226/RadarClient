using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.bidding.model
{
    public class Enums
    {
    }

    public enum CommandEnum
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

    }

}

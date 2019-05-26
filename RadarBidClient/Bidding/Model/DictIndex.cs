using log4net;
using Radar.Bidding.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model
{

    public enum DictIndex
    {
        /// <summary>
        /// 字库 - 当前时间
        /// </summary>
        INDEX_CURRENT_TIME = 1,

        /// <summary>
        /// 字库 - 价格区间
        /// </summary>
        INDEX_PRICE_SECTION = 2,

        /// <summary>
        /// 字库 - 数字[0, 9]
        /// </summary>
        INDEX_NUMBER = 3,

    }

}

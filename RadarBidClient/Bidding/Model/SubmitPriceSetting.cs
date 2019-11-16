using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    /// <summary>
    /// 策略格式如下：
    /// 检测秒数,匹配价格区间,加价
    /// </summary>
    public class SubmitPriceSetting
    {
        /// <summary>
        /// 检测秒数
        /// </summary>
        public int second { get; set; }

        /// <summary>
        /// 价格匹配区间 开始
        /// </summary>
        public int RangeStartDelta { get; set; }

        /// <summary>
        /// 价格匹配区间 结束
        /// </summary>
        public int RangeEndDelta { get; set; }

        /// <summary>
        /// 加价
        /// </summary>
        public int deltaPrice { get; set; }

        // 延迟提交毫秒数
        public int delayMills { get; set; }

        /// <summary>
        /// 是否未区间检测
        /// </summary>
        public bool IsRange { get; set; }

        // --------------------------------------------

        // 指定 分钟
        public int minute;

        // 计算好 到价
        public int basePrice;

        // 格式: 秒数,加价,延迟提交毫秒数,
        public string toLine()
        {
            return second + "," + deltaPrice + "," + delayMills;
        }

        /// <summary>
        /// 当时出价
        /// </summary>
        /// <returns></returns>
        public int GetOfferedPrice()
        {
            return basePrice + deltaPrice;
        }

        public static SubmitPriceSetting fromLine(string line)
        {
            if (line == null || line.Trim().Length == 0)
            {
                return null;
            }
            string[] arr = line.Trim().Split(',');
            if (arr.Length < 3)
            {
                return null;
            }

            // 检测秒数,匹配价格区间,加价
            var sps = new SubmitPriceSetting();
            sps.second = int.Parse(arr[0].Trim());
            var range = arr[1];
            if (range.Contains("-"))
            {
                sps.IsRange = true;
                var a2 = range.Trim().Split('-');
                sps.RangeStartDelta = int.Parse(a2[0]);
                sps.RangeEndDelta = int.Parse(a2[1]);
            }
            else
            {
                sps.IsRange = false;
            }

            sps.deltaPrice = int.Parse(arr[2].Trim());
            

            return sps;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class SubmitPriceSetting
    {
        // 检测秒数
        public int second { get; set; }

        // 加价
        public int deltaPrice { get; set; }

        // 延迟提交毫秒数
        public int delayMills { get; set; }

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
            // 秒数,加价,延迟毫秒数(未使用)
            SubmitPriceSetting sps = new SubmitPriceSetting();
            sps.second = int.Parse(arr[0]);
            sps.deltaPrice = int.Parse(arr[1]);
            sps.delayMills = int.Parse(arr[2]);

            return sps;
        }

    }
}

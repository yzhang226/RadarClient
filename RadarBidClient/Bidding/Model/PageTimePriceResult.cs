using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class PageTimePriceResult
    {
        /// <summary>
        /// 0 - 正常
        /// -1 - 没有识别到时间
        /// -2 - 仅识别到时间, 没有价格
        /// -11 - 没有找到时间坐标，即通过OCR未找到 目前时间 文字
        /// -12 - 没有找到时间坐标，即通过OCR未找到 价格区间 文字
        /// -100 - 未知错误 
        /// 300 - 重复检测, 不需要处理
        /// </summary>
        public int status { get; set; }

        public PagePrice data;

        public PageTimePriceResult(int status)
        {
            this.status = status;
        }

        public PageTimePriceResult(PagePrice data)
        {
            this.data = data;
        }

        public static PageTimePriceResult Ok(PagePrice data)
        {
            return new PageTimePriceResult(data);
        }

        public static PageTimePriceResult Error(int status)
        {
            return new PageTimePriceResult(status);
        }

        public static PageTimePriceResult ErrorTime()
        {
            return new PageTimePriceResult(-1);
        }

        public static PageTimePriceResult ErrorPrice()
        {
            return new PageTimePriceResult(-2);
        }

        public static PageTimePriceResult ErrorCoordTime()
        {
            return new PageTimePriceResult(-11);
        }

        public static PageTimePriceResult ErrorCoordPrice()
        {
            return new PageTimePriceResult(-12);
        }

        public static PageTimePriceResult RepeatedTime()
        {
            return new PageTimePriceResult(300);
        }

    }
}

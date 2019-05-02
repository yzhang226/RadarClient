using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.model
{

    public class BiddingContext
    {

        /// <summary>
        /// 
        /// </summary>
        private List<PagePrice> prices = new List<PagePrice>();
    
        private Dictionary<DateTime, PagePrice> priceMap = new Dictionary<DateTime, PagePrice>();

        /// <summary>
        /// 最后一次 触发 时间
        /// </summary>
        private DateTime LastTickTime;

        public List<CaptchaAnswerImage> Answers = new List<CaptchaAnswerImage>();

        public CaptchaAnswerImage Phase2Answer;

        public void addPagePrice(PagePrice price)
        {
            prices.Add(price);
            priceMap[price.pageTime] = price;
        }

    }

    public class SimplePoint
    {
        public int x { get; set; }

        public int y { get; set; }

        public SimplePoint()
        {

        }

        public SimplePoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // 增加增量
        public SimplePoint AddDelta(int x1, int y1)
        {
            return new SimplePoint(this.x + x1, this.y + y1);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

    }

    public class PageTimePriceResult
    {
        /// <summary>
        /// 0 - 正常
        /// -1 - 没有识别到时间
        /// -2 - 仅识别到时间, 没有价格
        /// -11 - 没有找到时间坐标，即通过OCR未找到 目前时间 文字
        /// -12 - 没有找到时间坐标，即通过OCR未找到 价格区间 文字
        /// -100 - 未知错误 
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

    }

    public class PagePrice
    {
        /// <summary>
        /// 页面显示的时间
        /// </summary>
        public DateTime pageTime { get; set; }

        /// <summary>
        /// 基础价格
        /// </summary>
        public int basePrice { get; set; }



        public int low { get; set; }

        public int high { get; set; }

        public PagePrice()
        {

        }


        public PagePrice(DateTime occur, int currentPrice)
        {
            this.pageTime = occur;
            this.basePrice = currentPrice;
        }

        public override bool Equals(object objx)
        {
            PagePrice obj = (PagePrice) objx;
            return obj != null && pageTime == obj.pageTime && basePrice == obj.basePrice;
        }

        public override int GetHashCode()
        {
            return (pageTime + "-" + basePrice).GetHashCode();
        }


        public override string ToString()
        {
            return "(" + pageTime + ", " + basePrice + ")";
        }

    }

    public class CommandRequest
    {
        public int messageType;

        public string action;

        public string[] args;
    }

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

        public static SubmitPriceSetting fromLine(string line)
        {
            if (line?.Trim().Length == 0)
            {
                return null;
            }
            string[] arr = line.Trim().Split(',');
            if (arr.Length < 3)
            {
                return null;
            }
            SubmitPriceSetting sps = new SubmitPriceSetting();
            sps.second = int.Parse(arr[0]);
            sps.deltaPrice = int.Parse(arr[1]);
            sps.delayMills = int.Parse(arr[2]);

            return sps;
        }

    }

    public class CaptchaAnswerImage
    {

        public DateTime PageTime;

        public DateTime CaptureTime;

        /// <summary>
        /// 图片分配的uuid
        /// </summary>
        public string Uuid;

        public string ImagePath1;

        public string ImagePath2;

        /// <summary>
        /// 答案 
        /// </summary>
        public string Answer;

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.model
{
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

    public class PagePrice
    {
        public DateTime occur { get; set; }

        public int basePrice { get; set; }

        public int status { get; set; }

        public int low { get; set; }

        public int high { get; set; }

        public PagePrice()
        {

        }

        public PagePrice(DateTime occur, int currentPrice)
        {
            this.occur = occur;
            this.basePrice = currentPrice;
        }

        public override bool Equals(object objx)
        {
            PagePrice obj = (PagePrice) objx;
            return obj != null && occur == obj.occur && basePrice == obj.basePrice;
        }

        public override int GetHashCode()
        {
            return (occur + "-" + basePrice).GetHashCode();
        }


        public override string ToString()
        {
            return occur + ", " + basePrice;
        }

    }

    /**
     *         public int timing021 { get; set; }

        public int deltaPrice021 { get; set; }

        public int delayMills021 { get; set; }
     * */
    public class BiddingStrategy
    {

        public int minute { get; set; }

        public int second { get; set; }

        public int deltaPrice { get; set; }

        public int delayMills { get; set; }

        public bool done { get; set; }

        public bool active { get; set; }

        public int basePrice { get; set; }

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

}

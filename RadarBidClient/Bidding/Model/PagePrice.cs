using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class PagePrice
    {
        /// <summary>
        /// ҳ����ʾ��ʱ��
        /// </summary>
        public DateTime pageTime { get; set; }

        /// <summary>
        /// �����۸�
        /// </summary>
        public int basePrice { get; set; }



        public int low { get; set; }

        public int high { get; set; }

        public PagePrice()
        {

        }


        public PagePrice(DateTime occur, int basePrice)
        {
            this.pageTime = occur;
            this.basePrice = basePrice;
        }

        public override bool Equals(object objx)
        {
            PagePrice obj = (PagePrice)objx;
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
}

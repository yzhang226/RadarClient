using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model
{
    public class MinutePriceDetail
    {
        private int minute;

        private int[] priceSegment;
    
        public MinutePriceDetail(int minute)
        {
            this.minute = minute;

            priceSegment = new int[60];
            for (int i=0; i<60; i++)
            {
                priceSegment[i] = 0;
            }
        }

        public bool IsPriceReady(int sec)
        {
            return priceSegment[sec] > 0;
        }

        public int GetPrice(int sec)
        {
            return priceSegment[sec];
        }

        public void SetPrice(int sec, int price)
        {
            priceSegment[sec] = price;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class PageTimePriceResult
    {
        /// <summary>
        /// 0 - ����
        /// -1 - û��ʶ��ʱ��
        /// -2 - ��ʶ��ʱ��, û�м۸�
        /// -11 - û���ҵ�ʱ�����꣬��ͨ��OCRδ�ҵ� Ŀǰʱ�� ����
        /// -12 - û���ҵ�ʱ�����꣬��ͨ��OCRδ�ҵ� �۸����� ����
        /// -100 - δ֪���� 
        /// 300 - �ظ����, ����Ҫ����
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

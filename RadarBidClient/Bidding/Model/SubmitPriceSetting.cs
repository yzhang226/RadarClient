using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class SubmitPriceSetting
    {
        // �������
        public int second { get; set; }

        // �Ӽ�
        public int deltaPrice { get; set; }

        // �ӳ��ύ������
        public int delayMills { get; set; }

        // --------------------------------------------

        // ָ�� ����
        public int minute;

        // ����� ����
        public int basePrice;

        // ��ʽ: ����,�Ӽ�,�ӳ��ύ������,
        public string toLine()
        {
            return second + "," + deltaPrice + "," + delayMills;
        }

        /// <summary>
        /// ��ʱ����
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
            // ����,�Ӽ�,�ӳٺ�����(δʹ��)
            SubmitPriceSetting sps = new SubmitPriceSetting();
            sps.second = int.Parse(arr[0]);
            sps.deltaPrice = int.Parse(arr[1]);
            sps.delayMills = int.Parse(arr[2]);

            return sps;
        }

    }
}

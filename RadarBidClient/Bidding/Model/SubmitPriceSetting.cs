using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    /// <summary>
    /// ���Ը�ʽ���£�
    /// �������,ƥ��۸�����,�Ӽ�
    /// </summary>
    public class SubmitPriceSetting
    {
        /// <summary>
        /// �������
        /// </summary>
        public int second { get; set; }

        /// <summary>
        /// �۸�ƥ������ ��ʼ
        /// </summary>
        public int RangeStartDelta { get; set; }

        /// <summary>
        /// �۸�ƥ������ ����
        /// </summary>
        public int RangeEndDelta { get; set; }

        /// <summary>
        /// �Ӽ�
        /// </summary>
        public int deltaPrice { get; set; }

        // �ӳ��ύ������
        public int delayMills { get; set; }

        /// <summary>
        /// �Ƿ�δ������
        /// </summary>
        public bool IsRange { get; set; }

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

            // �������,ƥ��۸�����,�Ӽ�
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

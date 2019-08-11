using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    /// <summary>
    /// ���� - ��
    /// </summary>
    public class CoordPoint
    {
        public int x { get; set; }

        public int y { get; set; }

        public CoordPoint()
        {

        }

        public CoordPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // ��������
        public CoordPoint AddDelta(int dx, int dy)
        {
            return new CoordPoint(this.x + dx, this.y + dy);
        }

        public static CoordPoint From(string text)
        {
            var arr = text.Split(',');
            int x = int.Parse(arr[0]);
            int y = int.Parse(arr[1]);
            return new CoordPoint(x, y);
        }

        /// <summary>
        /// Զ�˷��͵ĶԱ���������ƫ��ģ�����������(-14, -2)��У׼
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static CoordPoint FromAndAdjustRemote(string text)
        {
            CoordPoint cp = From(text);
            cp.x = cp.x - 14;
            cp.y = cp.y - 2;

            return cp;
        }

        /// <summary>
        /// Զ�˷��͵ĶԱ���������ƫ��ģ�����������(-14, -2)��У׼
        /// </summary>
        /// <returns></returns>
        public CoordPoint DeltaRemote()
        {
            return new CoordPoint(this.x - 14, this.y - 2);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

    }
}

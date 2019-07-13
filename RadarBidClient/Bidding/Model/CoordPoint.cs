using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    /// <summary>
    /// 坐标 - 点
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

        // 增加增量
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

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

    }
}

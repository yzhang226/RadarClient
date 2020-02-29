using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    /// <summary>
    /// ×ø±ê - ¾ØÐÎ
    /// </summary>
    public class CoordRectangle
    {
        public int x1 { get; set; }

        public int y1 { get; set; }

        public int x2 { get; set; }

        public int y2 { get; set; }

        public CoordRectangle()
        {

        }

        public CoordRectangle(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

        public static CoordRectangle From(int x1, int y1, int length, int width)
        {
            CoordRectangle rect = new CoordRectangle(x1, y1, x1 + length, y1 + width);
            return rect;
        }

        public static CoordRectangle From(CoordPoint p, int length, int width)
        {
            CoordRectangle rect = new CoordRectangle(p.x, p.y, p.x + length, p.y + width);
            return rect;
        }


        public int GetLength()
        {
            return x2 - x1;
        }

        public int GetWidth()
        {
            return y2 - y1;
        }

        public override string ToString()
        {
            return string.Format("{{({0}, {1}), ({2}, {3})}}", x1, y1, x2, y2);
        }


    }
}

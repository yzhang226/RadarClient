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

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

    }
}

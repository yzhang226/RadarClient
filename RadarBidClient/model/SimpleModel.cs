using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.model
{
    class SimplePoint
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
}

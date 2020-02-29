using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Radar.Common.Utils
{

    public class RequestSequence
    {

        public static readonly RequestSequence me = new RequestSequence();

        static int seq = 200000;

        public long Next()
        {
            Interlocked.Increment(ref seq);

            return seq;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Net
{   
    /// <summary>
    /// ping/pong计数器
    /// </summary>
    public class PingPongCounter
    {

        public static readonly PingPongCounter ME = new PingPongCounter();

        public long pingCount;

        private long pongCount;
        
        /// <summary>
        /// 客户端ping一次
        /// </summary>
        public void IncrementPing()
        {
            this.pingCount++;
        }

        /// <summary>
        /// 服务端响应ping一次pong
        /// </summary>
        public void IncrementPong()
        {
            this.pongCount++;
        }

        public long GetPingCount()
        {
            return this.pingCount;
        }

        public long GetPongCount()
        {
            return this.pongCount;
        }

        public void Reset()
        {
            this.pingCount = 0;
            this.pongCount = 0;
        }

    }
}

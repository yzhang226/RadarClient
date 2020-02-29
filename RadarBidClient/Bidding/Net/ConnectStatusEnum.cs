using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Net
{
    public enum ConnectStatusEnum
    {

        /// <summary>
        /// 表示空
        /// </summary>
        NONE = 0,

        /// <summary>
        /// 连接中，仅在 准备开始连接时设置
        /// </summary>
        CONNECTING = 1,

        /// <summary>
        /// 已连接成功，仅能在 ConnectCallback 之后会设置连接为成功状态
        /// </summary>
        CONNECTED = 10,



        /// <summary>
        /// 连接失败 
        /// </summary>
        CONNECT_FAILED = 100,

    }
}

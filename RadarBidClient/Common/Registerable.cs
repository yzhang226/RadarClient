using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.common
{
    // 标识 可注册的
    public interface Registerable
    {
        // 执行
        void execute();

    }

    // 标识 IoC 容器的 组件
    public abstract class BaseComponent
    {

        public virtual void afterProperties()
        {
            // default nothing
        }

    }

}

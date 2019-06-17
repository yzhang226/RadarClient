using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.IoC
{
    public interface InitializingBean
    {
        /// <summary>
        /// 初始化 Component 之后执行
        /// </summary>
        void AfterPropertiesSet();

    }
}

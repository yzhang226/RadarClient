using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.IoC
{
    public interface InitializingBean
    {
        /// <summary>
        /// ��ʼ�� Component ֮��ִ��
        /// </summary>
        void AfterPropertiesSet();

    }
}

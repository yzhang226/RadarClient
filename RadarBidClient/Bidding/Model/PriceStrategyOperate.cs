using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class PriceStrategyOperate
    {
        public SubmitPriceSetting setting;

        public string answerUuid;

        /// <summary>
        /// 0 - �������
        /// 1 - �ύ���
        /// 20 - �ȴ���֤��
        /// 21 - ��֤���������
        /// 99 - Cancelled
        /// -1 - δִ��
        /// </summary>
        public int status = -1;

    }
}

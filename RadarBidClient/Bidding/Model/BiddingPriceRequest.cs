using Radar.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model
{
    public class BiddingPriceRequest
    {
        
        /// <summary>
        /// 出价时 - 检测屏幕上的显示时间
        /// </summary>
        public DateTime OfferedScreenTime { get; set; }

        /// <summary>
        /// 出价时 - 检测屏幕上的显示价格
        /// </summary>
        public int OfferedScreenPrice { get; set; }

        /// <summary>
        /// 提交时 - 检测屏幕上的显示时间
        /// </summary>
        public DateTime SubmittedScreenTime { get; set; }

        /// <summary>
        /// 提交时 - 检测屏幕上的显示价格
        /// </summary>
        public int SubmittedScreenPrice { get; set; }

        /// <summary>
        /// 操作状态
        /// </summary>
        public StrategyOperateStatus OperateStatus { get; set; }

        /// <summary>
        /// 计算出的延迟秒数
        /// </summary>
        public int ComputedDelayMills { get; set; }

        /// <summary>
        /// 策略秒数
        /// </summary>
        public int StrategySecond { get; set; }

        /// <summary>
        /// 目标价格
        /// </summary>
        public int TargetPrice { get; set; }

        /// <summary>
        /// 验证码uuid
        /// </summary>
        public string CaptchaUuid { get; set; }

        /// <summary>
        /// 是否能提交
        /// </summary>
        public bool CanSubmit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SubmitMemo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CanncelMemo { get; set; }

        /// <summary>
        /// 是否为 区间检测 触发的
        /// </summary>
        public bool IsRangeTriggered { get; set; }


    }
}

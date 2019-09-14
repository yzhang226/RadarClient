using Radar.Common;
using Radar.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Model.Dto
{
    public class PriceActionRequest
    {
        /// <summary>
        /// 机器码
        /// </summary>
        public string MachineCode { get; set; }

        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime OccurTime { get; set; }

        /// <summary>
        /// 采集的屏幕上的时间
        /// </summary>
        public DateTime ScreenTime { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        public PriceAction Action { get; set; }

        /// <summary>
        /// 当时的价格
        /// </summary>
        public int ScreenPrice { get; set; }

        /// <summary>
        /// 目标价格价格
        /// </summary>
        public int TargetPrice { get; set; }

        /// <summary>
        /// 使用的延时(毫秒)
        /// </summary>
        public int UsedDelayMills { get; set; }

        //public int target

        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }

        public string ToLine()
        {
            return MachineCode + ";" + KK.ToMills(OccurTime) + ";" + KK.ToMills(ScreenTime) + ";" + Action + ";" + ScreenPrice + ";" + TargetPrice + ";" + UsedDelayMills + ";" + Memo;
        }

        public PriceActionRequest FromLine(string line)
        {
            string[] arr = line.Split(';');
            var req = new PriceActionRequest();
            req.MachineCode = arr[0];
            req.OccurTime = KK.ToDateTime(long.Parse(arr[1]));
            req.ScreenTime = KK.ToDateTime(long.Parse(arr[2]));
            // req.Action = (PriceAction) Enum.ToObject(typeof(PriceAction), int.Parse(arr[3]));
            req.Action = (PriceAction)Enum.Parse(typeof(PriceAction), arr[3]);
            req.ScreenPrice = int.Parse(arr[4]);
            req.TargetPrice = int.Parse(arr[5]);
            req.UsedDelayMills = int.Parse(arr[6]);
            req.Memo = arr.Length > 7 ? arr[7] : null;

            return req;
        }

    }
}

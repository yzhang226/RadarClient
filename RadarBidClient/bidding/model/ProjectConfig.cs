﻿using RadarBidClient.ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.model
{
    [Component]
    public class ProjectConfig
    {
        // 竞拍网站地址 - 包含 http://
        public string BidAddressPrefix
        {
            get
            {
                return Properties.Settings.Default.BidAddressPrefix;
            }
        }

        public string CaptchaAddressPrefix
        {
            get
            {
                return Properties.Settings.Default.CaptchaAddressPrefix;
            }
        }

        public string BidLoginUrl
        {
            get
            {
                return this.BidAddressPrefix + "/login.htm";
            }
        }

        public bool EnableAutoBidding
        {
            get
            {
                return Properties.Settings.Default.EnableAutoBidding;
            }
        }

    }
}
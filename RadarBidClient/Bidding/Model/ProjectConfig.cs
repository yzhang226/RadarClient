using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Model
{
    [Component]
    public class ProjectConfig
    {
        /// <summary>
        /// 竞拍网站地址 - 包含 http://
        /// </summary>
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

        public string UploadCaptchaTaskUrl
        {
            get
            {
                return this.CaptchaAddressPrefix + "/v1/biding/captcha-task"; ;
            }
        }

        public string BidLoginUrl
        {
            get
            {
                return this.BidAddressPrefix + Properties.Settings.Default.BidLoginUrl;
            }
        }

        public string BidBidingUrl
        {
            get
            {
                return this.BidAddressPrefix + Properties.Settings.Default.BidBiddingUrl;
            }
        }

        public bool EnableAutoBidding
        {
            get
            {
                return Properties.Settings.Default.EnableAutoBidding;
            }
        }

        public bool EnableAutoUpdate
        {
            get
            {
                return Properties.Settings.Default.EnableAutoUpdate;
            }
        }

        public string RunEnv
        {
            get
            {
                return Properties.Settings.Default.RunEnv;
            }
        }

        public bool EnvIsProd
        {
            get
            {
                return this.RunEnv == "prod";
            }
        }

        public string SaberServerAddress
        {
            get
            {
                return Properties.Settings.Default.SaberServerAddress;
            }
        }

        public bool EnableSaberRobot
        {
            get
            {
                return Properties.Settings.Default.EnableSaberRobot;
            }
        }

    }
}

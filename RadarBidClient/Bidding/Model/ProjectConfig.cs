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

        public bool EnableAutoInputAccount
        {
            get
            {
                return Properties.Settings.Default.EnableAutoInputAccount;
            }
        }

        public bool LoginAccountAfterAutoInput
        {
            get
            {
                return Properties.Settings.Default.LoginAccountAfterAutoInput;
            }
        }

        public string SaberWebAddressPrefix
        {
            get
            {
                return Properties.Settings.Default.SaberWebAddressPrefix;
            }
        }

        public string UploadRobotScreenUrl
        {
            get
            {
                return this.SaberWebAddressPrefix + "/v1/screen/upload"; ;
            }
        }

        /// <summary>
        /// 是否开启 校准系统时间
        /// </summary>
        public bool EnableCorrectNetTime
        {
            get
            {
                return Properties.Settings.Default.EnableCorrectNetTime;
            }
        }

        /// <summary>
        /// 确定按钮点击模式
        /// 1 - 普通点击 ;
        /// 2 - 栅栏模式 ;
        /// 3 - 混合模式 先普通点击 再 栅栏模式
        /// </summary>
        public int ConfirmClickMode
        {
            get
            {
                return Properties.Settings.Default.ConfirmClickMode;
            }
        }


        public bool IsConfirmModeNormal()
        {
            return Properties.Settings.Default.ConfirmClickMode == 1;
        }

        public bool IsConfirmModeFence()
        {
            return Properties.Settings.Default.ConfirmClickMode == 2;
        }

        public bool IsConfirmModeMixed()
        {
            return Properties.Settings.Default.ConfirmClickMode == 3;
        }

    }
}

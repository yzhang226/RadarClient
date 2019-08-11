using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class CaptchaImageUploadRequest
    {
        public string machineCode;

        public string uid;

        public string from;

        public string token;

        public long timestamp;


        /**
         * 上传类型
         */
        public int uploadType;

    }
}

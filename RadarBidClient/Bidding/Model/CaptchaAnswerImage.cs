using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class CaptchaAnswerImage
    {

        public DateTime PageTime;

        public DateTime CaptureTime;

        /// <summary>
        /// ͼƬ�����uuid
        /// </summary>
        public string Uuid;

        public string ImagePath1;

        public string ImagePath2;

        /// <summary>
        /// �� 
        /// </summary>
        public string Answer;

    }
}

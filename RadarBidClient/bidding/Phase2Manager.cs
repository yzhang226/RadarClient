using log4net;
using RadarBidClient.common;
using RadarBidClient.ioc;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.bidding
{
    [Component]
    public class Phase2Manager
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Phase2Manager));

        private ProjectConfig conf;

        private BidActionManager ActionManager;

        
        public Phase2Manager(ProjectConfig conf, BidActionManager ActionManager)
        {
            this.conf = conf;
            this.ActionManager = ActionManager;
        }

        /// <summary>
        /// 出价 - 
        /// 1. 输入价格 - 移动到输入框位置，点击，清空历史数据，输入新价格
        /// 2. 点击出价按钮 - 移动到按钮位置，点击
        /// 3. 对验证码区域截图 且 上传
        /// </summary>
        public CaptchaAnswerImage OfferPrice(int targetPrice, bool EnableCancelFirst)
        {
            var Datum = ActionManager.GetDatum();

            // 0. 出价前，先尝试取消，防止上一步的可能的遮罩
            if (EnableCancelFirst)
            {
                // this.CancelOfferedPrice();
                ActionManager.ClickBtnUseFenceFromRightToLeft(Datum.AddDelta(742, 502));
            }

            // 1. 输入价格 且 出价
            // TODO: 坐标方法 - 应该抽取出来单独管理 
            ActionManager.InputPriceAtPoint(Datum.AddDelta(676, 417), targetPrice);
            ActionManager.ClickOfferBtn(Datum.AddDelta(800, 415));

            // 2. 对验证码区域截屏且上传 
            KK.Sleep(500);
            CaptchaAnswerImage img = CaptureCaptchaImage();
            UploadCaptchaImage(img);

            return img;
        }

        private CaptchaAnswerImage CaptureCaptchaImage()
        {
            var Datum = ActionManager.GetDatum();

            DateTime dt = DateTime.Now;
            var uuid = KK.uuid();
            
            // 1. 验证码 - 提示语
            CoordRectangle rect1 = CoordRectangle.From(Datum.AddDelta(442, 338), 380, 53);
            var img01Path = KK.CapturesDir() + "\\" + uuid + "-" + dt.ToString("HHmmss") + "-p21.jpg";
            ActionManager.CaptureImage(rect1, img01Path);

            // 2. 验证码 - 图形区域
            CoordRectangle rect2 = CoordRectangle.From(Datum.AddDelta(445, 390), 230, 90);
            var img02Path = KK.CapturesDir() + "\\" + uuid + "-" + dt.ToString("HHmmss") + "-p22.jpg";
            ActionManager.CaptureImage(rect2, img02Path);

            CaptchaAnswerImage img = new CaptchaAnswerImage();
            img.Uuid = uuid;
            img.CaptureTime = dt;
            img.ImagePath1 = img01Path;
            img.ImagePath2 = img02Path;

            return img;
        }

        private void UploadCaptchaImage(CaptchaAnswerImage img)
        {
            string url = conf.UploadCaptchaTaskUrl;
            CaptchaImageUploadRequest req = new CaptchaImageUploadRequest();
            req.token = "devJustTest";
            req.uid = img.Uuid;
            req.timestamp = KK.currentTs();
            req.from = "test";

            int httpStatus;
            DataResult<CaptchaImageUploadResponse> dr = HttpClients
                .PostWithFiles<DataResult<CaptchaImageUploadResponse>>(url, req, new List<string> { img.ImagePath1, img.ImagePath2 }, out httpStatus);

            logger.InfoFormat("upload catpcha task#{0}, result is {1}", img.Uuid, Jsons.ToJson(dr));
        }

        /// <summary>
        /// 提交之前输入验证码, 然后再提交已出的价格
        /// </summary>
        /// <param name="answer"></param>
        public void SubmitOfferedPrice(string answer)
        {
            this.InputCaptchForSubmit(answer);
            this.SubmitOfferedPrice();
        }

        /// <summary>
        /// 提交之前输入验证码
        /// </summary>
        /// <param name="answer"></param>
        public void InputCaptchForSubmit(string answer)
        {
            logger.InfoFormat("submit offered-price with answer#{0}", answer);

            var Datum = ActionManager.GetDatum();

            ActionManager.InputCaptchAtPoint(Datum.AddDelta(734, 416), answer);

        }

        /// <summary>
        /// 提交已出的价格
        /// </summary>
        public void SubmitOfferedPrice()
        {
            var Datum = ActionManager.GetDatum();

            ActionManager.ClickBtnUseFenceFromLeftToRight(Datum.AddDelta(553, 500));

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            // TODO: 应该检测 区域 是否有 出价有效
            KK.Sleep(600);
            ActionManager.ClickBtnOnceAtPoint(Datum.AddDelta(661, 478));

            // 清除以前输入的价格
            ActionManager.CleanPriceAtPoint(Datum.AddDelta(676, 417), true);
        }

        /// <summary>
        /// 取消 出价
        /// </summary>
        public void CancelOfferedPrice()
        {
            var Datum = ActionManager.GetDatum();
            
            ActionManager.ClickBtnUseFenceFromRightToLeft(Datum.AddDelta(742, 502));

            // 清除以前输入的价格
            ActionManager.CleanPriceAtPoint(Datum.AddDelta(676, 417), true);
        }


    }
}

using log4net;
using Radar.Bidding.Model;
using Radar.Common;
using Radar.Common.Model;
using Radar.Common.Threads;
using Radar.IoC;
using Radar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding
{
    [Component]
    public class Phase2ActManager
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Phase2ActManager));

        private ProjectConfig conf;

        private BidActionManager actionManager;

        
        public Phase2ActManager(ProjectConfig conf, BidActionManager actionManager)
        {
            this.conf = conf;
            this.actionManager = actionManager;
        }

        /// <summary>
        /// 出价 - 
        /// 1. 输入价格 - 移动到输入框位置，点击，清空历史数据，输入新价格
        /// 2. 点击出价按钮 - 移动到按钮位置，点击
        /// 3. 对验证码区域截图 且 上传
        /// </summary>
        public CaptchaAnswerImage OfferPrice(int targetPrice, bool enableCancelFirst)
        {
            var Datum = actionManager.Datum;

            // 0. 出价前，先尝试取消，防止上一步的可能的遮罩
            if (enableCancelFirst)
            {
                // this.CancelOfferedPrice();
                actionManager.ClickButtonByFenceWayRToL(Datum.AddDelta(742, 502));
            }

            // 1. 输入价格 且 出价
            // TODO: 坐标方法 - 应该抽取出来单独管理 
            actionManager.InputTextAtPoint(Datum.AddDelta(676, 417), targetPrice.ToString(), true, "第二阶段出价");
            actionManager.ClickButtonAtPoint(Datum.AddDelta(800, 415), true, "第二阶段出价");

            // 2. 对验证码区域截屏且上传 
            KK.Sleep(500);
            CaptchaAnswerImage img = CaptureCaptchaImage();
            UploadCaptchaImage(img);

            return img;
        }

        private CaptchaAnswerImage CaptureCaptchaImage()
        {
            var Datum = actionManager.Datum;

            DateTime dt = DateTime.Now;
            var uuid = KK.uuid();
            
            // 1. 验证码 - 提示语
            CoordRectangle rect1 = CoordRectangle.From(Datum.AddDelta(442, 338), 380, 53);
            var img01Path = string.Format("{0}\\{1}-{2:HHmmss}-p21.jpg", KK.CapturesDir(), uuid, dt);
            actionManager.CaptureImage(rect1, img01Path);

            // 2. 验证码 - 图形区域
            CoordRectangle rect2 = CoordRectangle.From(Datum.AddDelta(445, 390), 230, 90);
            var img02Path = string.Format("{0}\\{1}-{2:HHmmss}-p22.jpg", KK.CapturesDir(), uuid, dt);
            actionManager.CaptureImage(rect2, img02Path);

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
            req.timestamp = KK.CurrentMills();
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

            var Datum = actionManager.Datum;

            actionManager.InputTextAtPoint(Datum.AddDelta(734, 416), answer, true, "第二阶段提交#输入验证码");
        }

        /// <summary>
        /// 提交已出的价格
        /// </summary>
        public void SubmitOfferedPrice()
        {
            var Datum = actionManager.Datum;

            // TODO: 为了尽快的点击到确定按钮，提供3种模式
            if (conf.IsConfirmModeNormal())
            {
                actionManager.ClickButtonAtPoint(Datum.AddDelta(554, 506), false, "第二阶段提交#1");
            }
            else if (conf.IsConfirmModeMixed())
            {
                actionManager.ClickButtonAtPoint(Datum.AddDelta(554, 506), false, "第二阶段提交#1");
                actionManager.ClickButtonByFenceWayLToR(Datum.AddDelta(553, 500));
            }
            else if (conf.IsConfirmModeFence())
            {
                actionManager.ClickButtonByFenceWayLToR(Datum.AddDelta(553, 500));
            }
            else
            {
                actionManager.ClickButtonByFenceWayLToR(Datum.AddDelta(553, 500));
            }

            // TODO: 等待, 点击完成验证码确认按钮, 会弹出 出价有效
            // TODO: 应该检测 区域 是否有 出价有效
            KK.Sleep(600);

            // 尝试得到提交的结果截图, 0.6s 1.6s 2.6s 
            ThreadUtils.StartNewBackgroudThread(() => {
                for (int i=0; i<3; i++)
                {
                    try
                    {
                        string imgPath = actionManager.CaptureFlashScreen();
                        logger.InfoFormat("result of phase2-act is {0}", imgPath);
                        KK.Sleep((i + 1) * 1000);
                    }
                    catch (Exception e)
                    {
                        logger.Error("act2 background CaptureFlashScreen error", e);
                    }
                }
            });
            

            actionManager.ClickButtonAtPoint(Datum.AddDelta(661, 478), false, "第二阶段提交#确定");

            // 清除以前输入的价格
            actionManager.CleanTextAtPoint(Datum.AddDelta(676, 417), 6, true, "第二阶段提交");
        }

        /// <summary>
        /// 取消 出价
        /// </summary>
        public void CancelOfferedPrice()
        {
            var Datum = actionManager.Datum;
            
            actionManager.ClickButtonByFenceWayRToL(Datum.AddDelta(742, 502));

            // 清除以前输入的价格
            actionManager.CleanTextAtPoint(Datum.AddDelta(676, 417), 6, true, "第二阶段出价#取消");
        }

    }
}

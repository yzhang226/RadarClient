using log4net;
using RadarBidClient.bidding.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.model
{

    public class BiddingContext
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BiddingContext));
        /// <summary>
        /// 
        /// </summary>
        private List<PagePrice> prices = new List<PagePrice>();
    
        private Dictionary<DateTime, PagePrice> priceMap = new Dictionary<DateTime, PagePrice>();

        /// <summary>
        /// 最后一次 触发 时间
        /// </summary>
        // private DateTime LastTickTime;

        public List<CaptchaAnswerImage> Answers = new List<CaptchaAnswerImage>();

        // public CaptchaAnswerImage Phase2PreviewCaptcha;

        // public CaptchaAnswerImage LastAnswer;

        private List<CaptchaAnswerImage> ImagesOfAwaitAnswer = new List<CaptchaAnswerImage>();

        private Dictionary<string, string> answerMap = new Dictionary<string, string>();

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<int, int> PriceAfter29 = new Dictionary<int, int>();

        private Dictionary<int, PriceSubmitOperate> submitOperateMap = new Dictionary<int, PriceSubmitOperate>();

        private Dictionary<string, PriceSubmitOperate> uuidOfsubmitOperateMap = new Dictionary<string, PriceSubmitOperate>();

        public void AddPrice(int sec, int basePrice)
        {
            PriceAfter29[sec] = basePrice;
        }

        public int GetPrice(int sec)
        {
            if (!PriceAfter29.ContainsKey(sec))
            {
                return -1;
            }

            return PriceAfter29[sec];
        }

        public PriceSubmitOperate AddPriceSetting(SubmitPriceSetting settting)
        {
            var oper = new PriceSubmitOperate();
            oper.setting = settting;
            oper.status = -1;

            submitOperateMap[settting.second] = oper;

            return oper;
        }

        public Dictionary<int, PriceSubmitOperate> GetSubmitOperateMap()
        {
            return submitOperateMap;
        }



        public bool RemoveSubmitOperate(int second)
        {
            if (submitOperateMap.ContainsKey(second))
            {
                return false;
            }

            return submitOperateMap.Remove(second);
        }

        public void CleanSubmitOperate()
        {
            submitOperateMap.Clear();
        }

        public void PutAnswer(string uuid, string answer)
        {
            answerMap[uuid] = answer;
        }

        public string GetAnswer(string uuid)
        {
            if (!answerMap.ContainsKey(uuid))
            {
                return string.Empty;
            }

            return answerMap[uuid];
        }

        public void PutAwaitImage(CaptchaAnswerImage image, PriceSubmitOperate oper)
        {
            ImagesOfAwaitAnswer.Add(image);
            if (oper != null)
            {
                uuidOfsubmitOperateMap[image.Uuid] = oper;
            }
            logger.InfoFormat("add task#{0} to await list", image.Uuid);
        }

        public PriceSubmitOperate GetSubmitOperateByUuid(string uuid)
        {
            if (!uuidOfsubmitOperateMap.ContainsKey(uuid))
            {
                return null;
            }
            return uuidOfsubmitOperateMap[uuid];
        }

        public void RemoveAwaitImage(string uuid)
        {
            foreach (var img in ImagesOfAwaitAnswer)
            {
                if (img.Uuid == uuid)
                {
                    ImagesOfAwaitAnswer.Remove(img);
                    logger.InfoFormat("remove task#{0} from await list", uuid);
                    break;
                }
            }
        }

        public List<CaptchaAnswerImage> GetImagesOfAwaitAnswer()
        {
            return ImagesOfAwaitAnswer;
        }

        public bool IsAllImagesAnswered()
        {
            if (ImagesOfAwaitAnswer == null || ImagesOfAwaitAnswer.Count == 0)
            {
                return true;
            }

            foreach (var img in ImagesOfAwaitAnswer)
            {
                if (img != null && (img.Answer == null || img.Answer.Length == 0))
                {
                    return false;
                }
            }

            return true;
        }

        public void addPagePrice(PagePrice price)
        {
            prices.Add(price);
            priceMap[price.pageTime] = price;
        }

    }

    /// <summary>
    /// 坐标 - 点
    /// </summary>
    public class CoordPoint
    {
        public int x { get; set; }

        public int y { get; set; }

        public CoordPoint()
        {

        }

        public CoordPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // 增加增量
        public CoordPoint AddDelta(int dx, int dy)
        {
            return new CoordPoint(this.x + dx, this.y + dy);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

    }

    /// <summary>
    /// 坐标 - 矩形
    /// </summary>
    public class CoordRectangle
    {
        public int x1 { get; set; }

        public int y1 { get; set; }

        public int x2 { get; set; }

        public int y2 { get; set; }

        public CoordRectangle()
        {

        }

        public CoordRectangle(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

        public static CoordRectangle From(int x1, int y1, int length, int width)
        {
            CoordRectangle rect = new CoordRectangle(x1, y1, x1+length, y1+width);
            return rect;
        }

        public static CoordRectangle From(CoordPoint p, int length, int width)
        {
            CoordRectangle rect = new CoordRectangle(p.x, p.y, p.x + length, p.y + width);
            return rect;
        }


        public int GetLength()
        {
            return x2 - x1;
        }

        public int GetWidth()
        {
            return y2 - y1;
        }

        public override string ToString()
        {
            return "{(" + x1 + ", " + y1 + "), (" + x2 + ", " + y2 + ")}";
        }


    }

    public class PageTimePriceResult
    {
        /// <summary>
        /// 0 - 正常
        /// -1 - 没有识别到时间
        /// -2 - 仅识别到时间, 没有价格
        /// -11 - 没有找到时间坐标，即通过OCR未找到 目前时间 文字
        /// -12 - 没有找到时间坐标，即通过OCR未找到 价格区间 文字
        /// -100 - 未知错误 
        /// 300 - 重复检测, 不需要处理
        /// </summary>
        public int status { get; set; }

        public PagePrice data;

        public PageTimePriceResult(int status)
        {
            this.status = status;
        }

        public PageTimePriceResult(PagePrice data)
        {
            this.data = data;
        }

        public static PageTimePriceResult Ok(PagePrice data)
        {
            return new PageTimePriceResult(data);
        }

        public static PageTimePriceResult Error(int status)
        {
            return new PageTimePriceResult(status);
        }

        public static PageTimePriceResult ErrorTime()
        {
            return new PageTimePriceResult(-1);
        }

        public static PageTimePriceResult ErrorPrice()
        {
            return new PageTimePriceResult(-2);
        }

        public static PageTimePriceResult ErrorCoordTime()
        {
            return new PageTimePriceResult(-11);
        }

        public static PageTimePriceResult ErrorCoordPrice()
        {
            return new PageTimePriceResult(-12);
        }

        public static PageTimePriceResult RepeatedTime()
        {
            return new PageTimePriceResult(300);
        }

    }

    public class PagePrice
    {
        /// <summary>
        /// 页面显示的时间
        /// </summary>
        public DateTime pageTime { get; set; }

        /// <summary>
        /// 基础价格
        /// </summary>
        public int basePrice { get; set; }



        public int low { get; set; }

        public int high { get; set; }

        public PagePrice()
        {

        }


        public PagePrice(DateTime occur, int currentPrice)
        {
            this.pageTime = occur;
            this.basePrice = currentPrice;
        }

        public override bool Equals(object objx)
        {
            PagePrice obj = (PagePrice) objx;
            return obj != null && pageTime == obj.pageTime && basePrice == obj.basePrice;
        }

        public override int GetHashCode()
        {
            return (pageTime + "-" + basePrice).GetHashCode();
        }


        public override string ToString()
        {
            return "(" + pageTime + ", " + basePrice + ")";
        }

    }

    public class CommandRequest
    {
        public int messageType;

        public CommandEnum CommandName;

        public string action;

        public string[] args;
    }

    public class PriceSubmitOperate
    {
        public SubmitPriceSetting setting;

        public string answerUuid;

        /// <summary>
        /// 0 - 出价完成
        /// 1 - 提交完成
        /// 20 - 等待验证码
        /// 21 - 验证码输入完成
        /// 99 - Cancelled
        /// -1 - 未执行
        /// </summary>
        public int status = -1;

    }

    public class SubmitPriceSetting
    {
        // 检测秒数
        public int second { get; set; }

        // 加价
        public int deltaPrice { get; set; }

        // 延迟提交毫秒数
        public int delayMills { get; set; }

        // --------------------------------------------

        // 指定 分钟
        public int minute;

        // 计算好 到价
        public int basePrice;

        // 格式: 秒数,加价,延迟提交毫秒数,
        public string toLine()
        {
            return second + "," + deltaPrice + "," + delayMills;
        }

        /// <summary>
        /// 当时出价
        /// </summary>
        /// <returns></returns>
        public int GetOfferedPrice()
        {
            return basePrice + deltaPrice;
        }

        public static SubmitPriceSetting fromLine(string line)
        {
            if (line == null || line.Trim().Length == 0)
            {
                return null;
            }
            string[] arr = line.Trim().Split(',');
            if (arr.Length < 3)
            {
                return null;
            }
            // 秒数,加价,延迟毫秒数(未使用)
            SubmitPriceSetting sps = new SubmitPriceSetting();
            sps.second = int.Parse(arr[0]);
            sps.deltaPrice = int.Parse(arr[1]);
            sps.delayMills = int.Parse(arr[2]);

            return sps;
        }

    }

    public class CaptchaAnswerImage
    {

        public DateTime PageTime;

        public DateTime CaptureTime;

        /// <summary>
        /// 图片分配的uuid
        /// </summary>
        public string Uuid;

        public string ImagePath1;

        public string ImagePath2;

        /// <summary>
        /// 答案 
        /// </summary>
        public string Answer;

    }

    public class DataResult<T>
    {
        public int Status;

        public int HttpStatus;

        public string Message;

        public T Data;

        public DataResult()
        {

        }

        public DataResult(int Status, T Data, string Message)
        {
            this.Status = Status;
            this.Data = Data;
            this.Message = Message;
        }

    }

    public class CaptchaImageUploadRequest
    {
        public string uid;

        public string from;

        public string token;

        public long timestamp;

    }

    public class CaptchaImageUploadResponse
    {
        public string uid;

        public string from;

        public long timestamp;

    }

    public class CaptchaImageAnswerRequest
    {
        public string uid;

        public string from;

        public string token;

        public long timestamp;
    }

    public class CaptchaImageAnswerResponse
    {

        public string uid;

        public string from;

        public string answer;

        public long serverTimestamp;

    }

    public enum DictIndex
    {
        /// <summary>
        /// 字库 - 当前时间
        /// </summary>
        INDEX_CURRENT_TIME = 1,

        /// <summary>
        /// 字库 - 价格区间
        /// </summary>
        INDEX_PRICE_SECTION = 2,

        /// <summary>
        /// 字库 - 数字[0, 9]
        /// </summary>
        INDEX_NUMBER = 3,

    }

}

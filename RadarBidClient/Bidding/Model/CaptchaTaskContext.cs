using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class CaptchaTaskContext
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CaptchaTaskContext));

        public static readonly CaptchaTaskContext me = new CaptchaTaskContext();

        private CaptchaTaskContext()
        {

        }

        public List<CaptchaAnswerImage> Answers = new List<CaptchaAnswerImage>();

        private List<CaptchaAnswerImage> ImagesOfAwaitAnswer = new List<CaptchaAnswerImage>();

        private Dictionary<string, string> answerMap = new Dictionary<string, string>();


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

        public void PutAwaitImage(CaptchaAnswerImage image)
        {
            ImagesOfAwaitAnswer.Add(image);
            logger.InfoFormat("add task#{0} to await list", image.Uuid);
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

    }
}

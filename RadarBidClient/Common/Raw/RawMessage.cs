using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common
{
    public class RawMessage
    {

        /**
         * 总长度
         */
        public int totalLength;// 4 - bytes

        /**
         * 魔数
         */
        public int magic;// 4 - bytes

        /**
         * 发生时间毫秒数
         */
        public long occurMills;// 8 - bytes

        /**
         * 客户端编号
         */
        public int clientNo { get; set; }// 4

        /**
         * 消息类型
         */
        public int messageType;// 4

        /**
         *
         */
        public int bodyLength;// 4

        /**
         * 消息体 - 字符串格式
         */
        public String bodyText;


        public int getTotalLength()
        {
            return totalLength;
        }

        public void setTotalLength(int totalLength)
        {
            this.totalLength = totalLength;
        }

        public int getMagic()
        {
            return magic;
        }

        public void setMagic(int magic)
        {
            this.magic = magic;
        }

        public long getOccurMills()
        {
            return occurMills;
        }

        public void setOccurMills(long occurMills)
        {
            this.occurMills = occurMills;
        }

        public int getClientNo()
        {
            return clientNo;
        }

        public void setClientNo(int clientNo)
        {
            this.clientNo = clientNo;
        }

        public int getMessageType()
        {
            return messageType;
        }

        public void setMessageType(int messageType)
        {
            this.messageType = messageType;
        }

        public int getBodyLength()
        {
            return bodyLength;
        }

        public void setBodyLength(int bodyLength)
        {
            this.bodyLength = bodyLength;
        }

        public String getBodyText()
        {
            return bodyText;
        }

        public void setBodyText(String bodyText)
        {
            this.bodyText = bodyText;
        }

    }
}

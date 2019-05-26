using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common
{
    public class RawMessage
    {

        /**
         * �ܳ���
         */
        public int totalLength;// 4 - bytes

        /**
         * ħ��
         */
        public int magic;// 4 - bytes

        /**
         * ����ʱ�������
         */
        public long occurMills;// 8 - bytes

        /**
         * �ͻ��˱��
         */
        public int clientNo { get; set; }// 4

        /**
         * ��Ϣ����
         */
        public int messageType;// 4

        /**
         *
         */
        public int bodyLength;// 4

        /**
         * ��Ϣ�� - �ַ�����ʽ
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

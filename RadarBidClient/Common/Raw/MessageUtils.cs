using Radar.Bidding.Model;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Common.Raw
{
    public class MessageUtils
    {

        // 
        public static RawMessage BuildJsonMessage(int clientNo, JsonCommand body)
        {
            return From(10002, clientNo, Jsons.ToJson(body));
        }

        public static RawMessage BuildJsonMessage(int clientNo, string body)
        {
            return From(10002, clientNo, body);
        }

        public static RawMessage From(int messageType, int clientNo, string body)
        {
            RawMessage msg = new RawMessage();

            int bLen = body.Length;
            int tLen = 4 + 4 + 8 + 4 + 4 + 4 + bLen;

            msg.setTotalLength(tLen);
            msg.setMagic(ByteUtils.MAGIC_NUMBER);
            msg.setOccurMills(KK.CurrentMills());

            msg.setClientNo(clientNo);
            msg.setMessageType(messageType);
            msg.setBodyLength(bLen);

            msg.setBodyText(body);

            return msg;
        } 


        public static JsonCommand ParseAsCommandRequest(int clientNo, string command)
        {
            JsonCommand req = Jsons.FromJson<JsonCommand>(command);
            if (clientNo > -1)
            {
                req.clientNo = clientNo;
            }
            

            return req;
        }


    }
}

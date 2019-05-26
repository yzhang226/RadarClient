using Radar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Common
{
    public class MessageUtils
    {

        public static RawMessage from(int messageType, int clientNo, String body)
        {
            RawMessage msg = new RawMessage();

            int bLen = body.Length;
            int tLen = 4 + 4 + 8 + 4 + 4 + 4 + bLen;

            msg.setTotalLength(tLen);
            msg.setMagic(Radar.Common.ByteUtils.MAGIC_NUMBER);
            msg.setOccurMills(Radar.Common.KK.CurrentMills());

            msg.setClientNo(clientNo);
            msg.setMessageType(messageType);
            msg.setBodyLength(bLen);

            msg.setBodyText(body);

            return msg;
        }


        public static Radar.Bidding.Model.CommandRequest ParseAsCommandRequest(string command)
        {
            Radar.Bidding.Model.CommandRequest req = new Radar.Bidding.Model.CommandRequest();
            string comm = command.Trim();
            int len = comm.Length;

            int idx = comm.IndexOf("(");
            int idx2 = comm.IndexOf(")", idx + 1);

            if (idx > -1 && idx2 > -1)
            {
                req.action = comm.Substring(0, idx);

                if (idx2 > idx + 1)
                {
                    string[] arr = comm.Substring(idx + 1, idx2 - idx - 1).Split(',');

                    List<string> lis = new List<string>();
                    foreach (string a in arr)
                    {
                        string tr = a.Trim();
                        if (tr.Length == 0)
                        {
                            continue;
                        }
                        lis.Add(tr);
                    }

                    req.args = lis.ToArray();
                }
                else
                {
                    req.args = new string[0];
                }

            }
            else
            {
                req.action = comm;
                req.args = new string[0];
            }

            req.CommandName = (Radar.Bidding.Model.ReceiveDirective)Enum.ToObject(typeof(Radar.Bidding.Model.ReceiveDirective), int.Parse(req.action));

            return req;
        }


    }
}

using Microsoft.Win32;
using Radar.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Radar.Common.Raw
{


    public class RawMessageEncoder
    {
        public static readonly RawMessageEncoder me = new RawMessageEncoder();

        private RawMessageEncoder()
        {

        }

        public RawMessage decode(byte[] data)
        {
            RawMessage msg = new RawMessage();
            msg.setTotalLength(ByteUtils.readInt(data, 0));
            msg.setMagic(ByteUtils.readInt(data, 4));
            msg.setOccurMills(ByteUtils.readLong(data, 8));

            msg.setClientNo(ByteUtils.readInt(data, 16));
            msg.setMessageType(ByteUtils.readInt(data, 20));
            msg.setBodyLength(ByteUtils.readInt(data, 24));

            string body = Encoding.Default.GetString(ByteUtils.readFixLength(data, 28, msg.getBodyLength()));

            msg.setBodyText(body);

            return msg;
        }

        public byte[] encode(RawMessage msg)
        {
            byte[] data = new byte[msg.getTotalLength()];

            ByteUtils.writeInt(data, 0, msg.getTotalLength());
            ByteUtils.writeInt(data, 4, msg.getMagic());
            ByteUtils.writeLong(data, 8, msg.getOccurMills());

            ByteUtils.writeInt(data, 16, msg.getClientNo());
            ByteUtils.writeInt(data, 20, msg.getMessageType());
            ByteUtils.writeInt(data, 24, msg.getBodyLength());

            byte[] bodyBytes = Encoding.Default.GetBytes(msg.getBodyText());

            ByteUtils.writeBytes(data, 28, bodyBytes);

            return data;
        }
    }

}

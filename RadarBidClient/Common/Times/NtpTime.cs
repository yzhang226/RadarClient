﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Radar.Common.Times
{


    /// <summary>  
    /// 网络时间  
    /// </summary>  
    public class NtpTime
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(NtpTime));

        private static readonly List<string> NtpServerList = new List<string>
        {
            "ntp1.aliyun.com", "ntp2.aliyun.com", "ntp3.aliyun.com",
            "ntp4.aliyun.com", "timentp5.aliyun.com", "ntp6.aliyun.com",
            "ntp7.aliyun.com"
        };
        
        public static DateTime GetNetworkTimeFromList()
        {  
            foreach (string ntpServer in NtpServerList)
            {
                try
                {
                    DateTime dt = GetNetworkTime(ntpServer);
                    if (dt != null)
                    {
                        return dt;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get network-time from server#" + ntpServer + " error. ", e);
                }
            }

            return DateTime.Now;
        }

        public static DateTime GetNetworkTime(string ntpServer)
        {
            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //NTP uses UDP

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            DateTime dt = networkDateTime.ToLocalTime();

            logger.InfoFormat("GetNetworkTime from NTPServer#{0}, time is {1}", ntpServer, dt);

            return dt;
        }

        // stackoverflow.com/a/3294698/162671
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
        
    }

}

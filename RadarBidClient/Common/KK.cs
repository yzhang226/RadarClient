using log4net;
using Radar.Bidding.Model;
using Radar.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Radar.Common
{
    public class KK
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(KK));

        private const string Windows2000 = "5.0";
        private const string WindowsXP = "5.1";
        private const string Windows2003 = "5.2";
        private const string Windows2008 = "6.0";
        private const string Windows7 = "6.1";
        private const string Windows8OrWindows81 = "6.2";
        private const string Windows10 = "10.0";

        // 
        public static long CurrentMills()
        {
            return ToMills(DateTime.Now);
        }

        public static DateTime ToDateTime(long mills)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return startTime.AddMilliseconds(mills);
        }

        public static long ToMills(DateTime dt)
        {
            // 这里要考虑时区
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long timeStamp = (long)(dt - startTime).TotalMilliseconds;
            return timeStamp;
        }

        // 
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        public static void Sleep(int mills)
        {
            Thread.Sleep(mills);
        }

        public static string uuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        //public static TValue GetIfPresent<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        //{
        //    if (dictionary.ContainsKey(key))
        //    {
        //        return dictionary[key];
        //    }

        //    return defaultValue;
        //}

        public static int timeToInt(DateTime dt)
        {
            string nowTime = dt.ToString("HHmmss");
            return int.Parse(nowTime);
        }

        public static DateTime intToTime(int intValue)
        {
            // 999999,  99999
            string text = intValue.ToString();
            if (intValue < 100000)
            {
                text = "0" + text;
            }
            return DateTime.Parse("HHmmss");
        }

        public static bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        public static string ToText(object obj)
        {
            string data = "";
            if (obj is string)
            {
                data = (string)obj;
            }
            else if (obj is byte[])
            {
                data = Encoding.UTF8.GetString((byte[])obj);
            }
            else
            {
                data = Jsons.ToJson(obj);
            }

            return data;
        }

        public static int RandomInt(int n, int m)
        {
            Random r = new Random();
            return r.Next(n, m);
        }

        public static string WorkingBaseDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string ResourceDir()
        {
            return WorkingBaseDir() + "\\Resource";
        }

        public static string CapturesDir()
        {
            return WorkingBaseDir() + "\\Captures";
        }

        public static string FlashScreenDir()
        {
            return CapturesDir() + "\\Screen";
        }

        /// <summary>
        /// Operating system			Version number
        /// Windows 10					10.0
        /// Windows Server 2016			10.0
        /// Windows 8.1					6.3
        /// Windows Server 2012 R2		6.3
        /// Windows 8					6.2
        /// Windows Server 2012			6.2
        /// Windows 7					6.1
        /// Windows Server 2008 R2		6.1
        /// Windows Server 2008			6.0
        /// Windows Vista				6.0
        /// Windows Server 2003 R2		5.2
        /// Windows Server 2003			5.2
        /// Windows XP 64-Bit Edition	5.2
        /// Windows XP					5.1
        /// Windows 2000				5.0
        /// 参考 http://csharphelper.com/blog/2017/10/get-the-computers-operating-system-in-c/
        /// </summary>
        /// <returns></returns>
        public static string GetFitOSName()
        {
            string osName = "";
            var osVersion = string.Format("{0}.{1}", Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor);
            switch (osVersion)
            {
                case Windows2000:
                    osName = "win2000";
                    break;
                case WindowsXP:
                    osName = "winxp";
                    break;
                case Windows2003:
                    osName = "win2003";
                    break;
                case Windows2008:
                    osName = "win2008";
                    break;
                case Windows7:
                    // 
                    osName = "win7";
                    break;
                case Windows8OrWindows81:
                    osName = "win10";
                    break;
                case Windows10:
                    osName = "win10";
                    break;
            }

            return osName;
        }

        public static string ExtractDigits(string text)
        {
            char[] cs2 = text.ToCharArray();
            string numberStr = "";

            foreach (char c in cs2)
            {
                if (c >= '0' && c <= '9')
                {
                    numberStr += c.ToString();
                }
            }
            return numberStr;
        }

        public static CaptchaImageAnswerRequest CreateImageAnswerRequest(string uuid)
        {
            var req = new CaptchaImageAnswerRequest();
            req.from = "test";
            req.token = "devJustTest";
            req.uid = uuid;
            req.timestamp = KK.CurrentMills();
            return req;
        }

        public static bool IsNotSecond(int sec)
        {
            if (sec < 0 || sec > 59)
            {
                return true;
            }

            return false;
        }

        private static string localIp = null;

        public static string GetLocalIP()
        {
            if (localIp?.Length > 0)
            {
                return localIp;
            }

            try
            {
                localIp = GetLocalIPv4(NetworkInterfaceType.Wireless80211);

                if (localIp == null || localIp.Length == 0)
                {
                    localIp = GetLocalIPv4(NetworkInterfaceType.Ethernet);
                }

                if (localIp == null || localIp.Length == 0)
                {
                    localIp = "127.0.0.1";
                }

                return localIp;
            }
            catch (Exception ex)
            {
                logger.Error("GetLocalIP error", ex);
                return "127.0.0.1";
            }
        }


        public static string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType != _type || item.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    if (IPAddress.IsLoopback(ip.Address))
                    {
                        continue;
                    }

                    if (ip.Address.ToString().EndsWith(".1"))
                    {
                        continue;
                    }

                    output = ip.Address.ToString();
                    logger.InfoFormat("detect ip#{0}", ip.Address);
                }
            }
            return output;
        }

        public static BidAccountInfo LoadResourceAccount()
        {
            string path = ResourceDir() + "/account.txt";
            string text = FileUtils.ReadTxtFile(path).Replace('\r', ' ');
            string[] lines = text.Split('\n');

            if (lines == null || lines.Length == 0 || lines[0] == null || lines[0].Length == 0)
            {
                return null;
            }

            string[] arr = lines[0].Split(',');
            if (arr == null || arr.Length < 2)
            {
                return null;
            }

            var acc = new BidAccountInfo();
            acc.BidNo = arr[0].Trim();
            acc.Password = arr[1].Trim();
            acc.IdCardNo = arr[2].Trim();
            return acc;
        }

        public static string GetFileNameNoSuffix(string path)
        {
            FileInfo fi = new FileInfo(path);
            string fname = fi.Name;
            int idx = fname.LastIndexOf(".");

            
            return idx > 0 ? fname.Substring(0, idx) : fname;
        }

        /// <summary>
        /// 读取seat-no文件, 获取座位号
        /// </summary>
        /// <returns></returns>
        public static string ReadClientSeatNo()
        {
            string seatNo = "-2";
            try
            {
                string path = KK.ResourceDir() + "\\seat-no";
                string text = FileUtils.ReadTxtFile(path);
                string[] arr = text.Split('\n');
                seatNo = arr[0];
            }
            catch (Exception e)
            {
                logger.Error("GetClientSeatNo", e);
                seatNo = "ERR";
            }
            return seatNo;
        }

        public static bool IsDigits(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }

        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }


    }
}

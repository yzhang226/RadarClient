using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace RadarBidClient
{
    class KK
    {
        // 
        public static long currentTs()
        {
            long currentTicks = DateTime.Now.Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (currentTicks - dtFrom.Ticks) / 10000;
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
            if (intValue <100000)
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
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

    }
}

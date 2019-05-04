using RadarBidClient.common;
using RadarBidClient.model;
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
            return WorkingBaseDir() + "\\resource";
        }

        public static string CapturesDir()
        {
            return WorkingBaseDir() + "\\Captures";
        }

    }

    public class DataResults
    {

        // private

    /**
     * 判断 <code>DataResult</code> 是否成功, null表示失败
     * @param dr
     * @return
     */
        public static bool isOk<T>(DataResult<T> dr)
        {
            return dr != null && dr.Status == 0;
        }

        /**
         * 判断 <code>DataResult</code> 是否失败, null表示失败
         * @param dr
         * @return
         */
        public static bool isFail<T>(DataResult<T> dr)
        {
            return !isOk(dr);
        }

        /**
         *
         * @param data
         * @param <T>
         * @return
         */
        public static DataResult<T> ok<T>(T data)
        {
            return new DataResult<T>(0, data, "");
        }

        /**
         *
         * @param message
         * @return
         */
        public static DataResult<T> fail<T>(string message)
        {
            // System.Nullable<T>
            return new DataResult<T>(-1, default(T), message);
        }

        /**
         *
         * @param status
         * @param message
         * @return
         */
        public static DataResult<T> fail<T>(int status, String message)
        {
            return new DataResult<T>(status, default(T), message);
        }

    }

}

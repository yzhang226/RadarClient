using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Radar.Common
{
    /// <summary>
    /// 更新系统时间
    /// </summary>
    public class SystemTimeUpdater
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(SystemTimeUpdater));

        //设置系统时间的API函数
        [DllImport("kernel32.dll")]
        private static extern bool SetLocalTime(ref SYSTEMTIME time);

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public short year;
            public short month;
            public short dayOfWeek;
            public short day;
            public short hour;
            public short minute;
            public short second;
            public short milliseconds;
        }

        /// <summary>
        /// 设置系统时间
        /// </summary>
        /// <param name="dt">需要设置的时间</param>
        /// <returns>返回系统时间设置状态，true为成功，false为失败</returns>
        public static bool SetDate(DateTime dt)
        {
            SYSTEMTIME st;

            st.year = (short)dt.Year;
            st.month = (short)dt.Month;
            st.dayOfWeek = (short)dt.DayOfWeek;
            st.day = (short)dt.Day;
            st.hour = (short)dt.Hour;
            st.minute = (short)dt.Minute;
            st.second = (short)dt.Second;
            st.milliseconds = (short)dt.Millisecond;
            bool rt = SetLocalTime(ref st);

            logger.InfoFormat("Set SystemTime to {0}", dt);
            

            return rt;
        }
    }
}

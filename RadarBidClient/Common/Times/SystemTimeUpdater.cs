using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Radar.Common
{
    /// <summary>
    /// ����ϵͳʱ��
    /// </summary>
    public class SystemTimeUpdater
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(SystemTimeUpdater));

        //����ϵͳʱ���API����
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
        /// ����ϵͳʱ��
        /// </summary>
        /// <param name="dt">��Ҫ���õ�ʱ��</param>
        /// <returns>����ϵͳʱ������״̬��trueΪ�ɹ���falseΪʧ��</returns>
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common
{
    public class DataResults
    {

        // private

        /**
         * �ж� <code>DataResult</code> �Ƿ�ɹ�, null��ʾʧ��
         * @param dr
         * @return
         */
        public static bool IsOK<T>(Radar.Common.Model.DataResult<T> dr)
        {
            return dr != null && dr.Status == 0;
        }

        /**
         * �ж� <code>DataResult</code> �Ƿ�ʧ��, null��ʾʧ��
         * @param dr
         * @return
         */
        public static bool IsFail<T>(Radar.Common.Model.DataResult<T> dr)
        {
            return !IsOK(dr);
        }

        /**
         *
         * @param data
         * @param <T>
         * @return
         */
        public static Radar.Common.Model.DataResult<T> OK<T>(T data)
        {
            return new Radar.Common.Model.DataResult<T>(0, data, "");
        }

        /**
         *
         * @param message
         * @return
         */
        public static Radar.Common.Model.DataResult<T> Fail<T>(string message)
        {
            // System.Nullable<T>
            return new Radar.Common.Model.DataResult<T>(-1, default(T), message);
        }

        /**
         *
         * @param status
         * @param message
         * @return
         */
        public static Radar.Common.Model.DataResult<T> Fail<T>(int status, String message)
        {
            return new Radar.Common.Model.DataResult<T>(status, default(T), message);
        }

    }
}

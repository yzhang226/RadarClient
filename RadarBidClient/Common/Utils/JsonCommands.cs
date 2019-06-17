using Radar.Common.Enums;
using Radar.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Common.Utils
{
    public class JsonCommands
    {


        /**
         * 判断 <code>JsonCommand</code> 是否成功, null表示失败
         * @param dr
         * @return
         */
        public static bool IsOK(JsonCommand dr)
        {
            return dr != null && dr.status == 0;
        }

        /**
         * 判断 <code>JsonCommand</code> 是否失败, null表示失败
         * @param dr
         * @return
         */
        public static bool IsFail(JsonCommand dr)
        {
            return !IsOK(dr);
        }

        /**
         *
         * @param data
         * @return
         */
        public static JsonCommand OK(CommandDirective directive, object data)
        {
            string json = null;
            if (data != null)
            {
                if (typeof(string).IsAssignableFrom(data.GetType()))
                {
                    json = (string)data;
                }
                else
                {
                    json = Jsons.ToJson(data);
                }
            }
            return new JsonCommand(directive, 0, json, "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JsonCommand Fail(string message)
        {
            return new JsonCommand(CommandDirective.NONE, -1, null, message);
        }

        /**
         *
         * @param message
         * @return
         */
        public static JsonCommand Fail(CommandDirective directive, string message)
        {
            return new JsonCommand(directive, -1, null, message);
        }

        /**
         *
         * @param status
         * @param message
         * @return
         */
        public static JsonCommand Fail(CommandDirective directive, int status, string message)
        {
            return new JsonCommand(directive, status, null, message);
        }


    }
}

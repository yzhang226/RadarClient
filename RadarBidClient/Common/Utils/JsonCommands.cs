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
        public static bool isOk(JsonCommand dr)
        {
            return dr != null && dr.status == 0;
        }

        /**
         * 判断 <code>JsonCommand</code> 是否失败, null表示失败
         * @param dr
         * @return
         */
        public static bool isFail(JsonCommand dr)
        {
            return !isOk(dr);
        }

        /**
         *
         * @param data
         * @return
         */
        public static JsonCommand ok(CommandDirective directive, object data)
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

        /**
         *
         * @param message
         * @return
         */
        public static JsonCommand fail(CommandDirective directive, String message)
        {
            return new JsonCommand(directive, -1, null, message);
        }

        /**
         *
         * @param status
         * @param message
         * @return
         */
        public static JsonCommand fail(CommandDirective directive, int status, String message)
        {
            return new JsonCommand(directive, status, null, message);
        }


    }
}

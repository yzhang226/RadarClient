using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Common
{
    public class Jsons
    {
        private static JsonSerializerSettings jsonSettings;
        static Jsons() {
            // 2019-07-17T00:22:04.401	
            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fff";

        }

        /// <summary> 
        /// 对象转JSON 
        /// </summary> 
        /// <param name="obj">对象</param> 
        /// <returns>JSON格式的字符串</returns> 
        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, jsonSettings);
        }

        /// <summary>
        /// 将Json字符串转换为对像  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, jsonSettings);
        }

    }
}

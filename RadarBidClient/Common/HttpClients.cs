using log4net;
using Newtonsoft.Json;
using Radar.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Radar.Common
{
    public class HttpClients
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(HttpClients));

        private static HttpClient httpClient;

        private static HttpClient uploadClient;

        static HttpClients() {
            // AutomaticDecompression = DecompressionMethods.GZip
            HttpClientHandler httpHandler = new HttpClientHandler();
            httpClient = new HttpClient(httpHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(10 * 1000);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 上传超时不能太长 不然会导致收到的验证码无效（已过期）
            uploadClient = new HttpClient();
            uploadClient.Timeout = TimeSpan.FromSeconds(30 * 1000);

        }

        private HttpClients()
        {

        }

        #region HttpClient

        /// <summary>
        /// Get请求指定的URL地址
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns></returns>
        public static string GetAsJson(string url)
        {
            string result = "";
            int httpStatus;
            result = GetAsJson(url, out httpStatus);
            return result;
        }

        /// <summary>
        /// Get请求指定的URL地址
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="statusCode">Response返回的状态</param>
        /// <returns></returns>
        public static string GetAsJson(string url, out int httpStatus)
        {
            string result = string.Empty;
            try
            {

                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                httpStatus = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }

                return result;
            }
            catch (Exception e)
            {
                httpStatus = -1;
                logger.Error("", e);
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpStatus"></param>
        /// <returns></returns>
        public static byte[] GetAsBytes(string url, out int httpStatus)
        {
            byte[] result = new byte[0];
            try
            {

                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                httpStatus = (int) response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsByteArrayAsync().Result;
                }

                return result;
            }
            catch (Exception e)
            {
                httpStatus = -1;
                logger.Error("GetAsBytes for url#" + url + " error.", e);
                return new byte[0];
            }
        }


        /// <summary>
        /// Get请求指定的URL地址
        /// </summary>
        /// <typeparam name="T">返回的json转换成指定实体对象</typeparam>
        /// <param name="url">URL地址</param>
        /// <returns></returns>
        public static T GetAsJson<T>(string url) where T : class, new()
        {
            int httpStatus;
            string json = GetAsJson(url, out httpStatus);
            
            return json?.Length > 0 ? Jsons.FromJson<T>(json) : null;
        }

        /// <summary>
        /// Post请求指定的URL地址
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="postData">提交到Web的Json格式的数据：如:{"ErrCode":"FileErr"}</param>
        /// <returns></returns>
        public static string PostAsJson(string url, object requestData)
        {
            string result = "";
            int httpStatus;
            result = PostAsJson(url, requestData, out httpStatus);
            return result;

        }

        /// <summary>
        /// Post请求指定的URL地址
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="postData">提交到Web的Json格式的数据：如:{"ErrCode":"FileErr"}</param>
        /// <param name="statusCode">Response返回的状态</param>
        /// <returns></returns>
        public static string PostAsJson(string url, object postData, out int statusCode)
        {
            string result = string.Empty;

            var httpContent = BuildStringJsonContent(postData);

            try
            {
                //异步Post
                HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;
                //输出Http响应状态码
                statusCode = (int)response.StatusCode;
                //确保Http响应成功
                if (response.IsSuccessStatusCode)
                {
                    //异步读取json
                    result = response.Content.ReadAsStringAsync().Result;
                }

                return result;
            }
            catch (Exception e)
            {
                statusCode = -1;
                
                logger.Error("", e);
                return string.Empty;
            }
        }

        private static HttpContent BuildStringJsonContent(object obj)
        {
            string data = KK.ToText(obj);

            HttpContent httpContent = new StringContent(data);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpContent.Headers.ContentType.CharSet = "utf-8";

            return httpContent;
        }

        /// <summary>
        /// 上传文件 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="filePaths"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static T PostWithFiles<T>(string url, object postData, List<string> filePaths, out int statusCode) where T : class, new()
        {
            // WebKitFormBoundaryafkSRjSyccnJC6ED
            string boundary = "------WebKitFormBoundary" + KK.CurrentMills();


            using (var httpContent = new MultipartFormDataContent(boundary))
            {

                // 1. form-data 
                // Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryqgCWFrzZQTH8Ubnp

                if (postData != null)
                {
                    Dictionary<string, object> dict = Jsons.FromJson<Dictionary<string, object>>(Jsons.ToJson(postData));
                    foreach (var item in dict)
                    {
                        //Content-Disposition: form-data; name="json"
                        var stringContent = new StringContent(KK.ToText(item.Value));
                        stringContent.Headers.Add("Content-Disposition", "form-data; name=\"" + item.Key + "\"");
                        httpContent.Add(stringContent, item.Key);
                    }

                }

                // 2. file-data
                foreach (string path in filePaths)
                {
                    FileInfo fi = new FileInfo(path);
                    string fname = fi.Name;
                    int idx = fname.LastIndexOf(".");

                    ByteArrayContent bac = new ByteArrayContent(File.ReadAllBytes(path));
                    httpContent.Add(bac, idx > 0 ? fname.Substring(0, idx) : fname, fname);
                }

                // httpContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");// boundary=" + boundary
                // httpContent.Headers.ContentType.CharSet = "utf-8";

                string result = string.Empty;
                //异步Post
                try
                {
                    HttpResponseMessage response = uploadClient.PostAsync(url, httpContent).Result;
                    //输出Http响应状态码
                    statusCode = (int)response.StatusCode;
                    //确保Http响应成功
                    if (response.IsSuccessStatusCode)
                    {
                        //异步读取json
                        result = response.Content.ReadAsStringAsync().Result;
                    }

                    return result?.Length > 0 ? Jsons.FromJson<T>(result) : null;
                } 
                catch (Exception e)
                {
                    logger.Error("", e);
                    statusCode = -1;
                    return default(T);
                }
                
            }

        }

        /// <summary>
        /// Post请求指定的URL地址
        /// </summary>
        /// <typeparam name="T">返回的json转换成指定实体对象</typeparam>
        /// <param name="url">URL地址</param>
        /// <param name="postData">提交到Web的Json格式的数据：如:{"ErrCode":"FileErr"}</param>
        /// <returns></returns>
        public static T PostAsJson<T>(string url, object postData) where T : class, new()
        {
            int httpStatus;
            string json = PostAsJson(url, postData, out httpStatus);

            return json?.Length > 0 ? Jsons.FromJson<T>(json) : null;
        }

        /// <summary>
        /// Put请求指定的URL地址
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="putData">提交到Web的Json格式的数据：如:{"ErrCode":"FileErr"}</param>
        /// <returns></returns>
        public static string PutAsJson(string url, object putData)
        {
            string result = "";
            int httpStatus;
            result = PutAsJson(url, putData, out httpStatus);
            return result;
        }

        /// <summary>
        /// Put请求指定的URL地址
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="putData">提交到Web的Json格式的数据：如:{"ErrCode":"FileErr"}</param>
        /// <param name="statusCode">Response返回的状态</param>
        /// <returns></returns>
        public static string PutAsJson(string url, object putData, out int statusCode)
        {
            string result = string.Empty;
            HttpContent httpContent = BuildStringJsonContent(putData);

            try
            {

                HttpResponseMessage response = httpClient.PutAsync(url, httpContent).Result;
                statusCode = (int)response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }
            } 
            catch (Exception e)
            {
                statusCode = -1;
                logger.Error("", e);
                return string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Put请求指定的URL地址
        /// </summary>
        /// <typeparam name="T">返回的json转换成指定实体对象</typeparam>
        /// <param name="url">URL地址</param>
        /// <param name="putData">提交到Web的Json格式的数据：如:{"ErrCode":"FileErr"}</param>
        /// <returns></returns>
        public static T PutAsJson<T>(string url, object putData) where T : class, new()
        {
            int httpStatus;
            string json = PutAsJson(url, putData, out httpStatus);

            return json?.Length > 0 ? Jsons.FromJson<T>(json) : null;
        }


        #endregion

    }
}

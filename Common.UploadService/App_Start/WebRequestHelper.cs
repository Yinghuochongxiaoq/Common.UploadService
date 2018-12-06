using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Common.UploadService
{
    /// <summary>
    /// WebRequest extend
    /// </summary>
    public static class WebRequestHelper
    {
        #region [1、获取请求参数]
        /// <summary>
        /// 验证字符串是否为数字（正则表达式）（true = 是数字, false = 不是数字）
        /// </summary>
        /// <param name="validatedString">被验证的字符串</param>
        /// <returns>true = 是数字, false = 不是数字</returns>
        private static bool IsNumeric(string validatedString)
        {
            const string numericPattern = @"^[-]?\d+[.]?\d*$";
            return Regex.IsMatch(validatedString, numericPattern);
        }

        /// <summary>
        /// Get Querystring or Request.From params,you also can define params in method param use [FromBody]Type params string.
        /// </summary>
        /// <param name="context">request context</param>
        /// <param name="key">params key</param>
        /// <returns></returns>
        public static string GetStringFromParameters(this HttpContext context, string key)
        {
            var value = string.Empty;
            if (string.IsNullOrEmpty(key) || context == null)
            {
                return value;
            }
            value = context.Request.QueryString[key];
            if (string.IsNullOrEmpty(value)) value = context.Request.Form[key];
            return value;
        }

        /// <summary>
        /// Get Querystring or Request.From params,you also can define params in method param use [FromBody]Type params int.
        /// </summary>
        /// <param name="context">request context</param>
        /// <param name="key">params key</param>
        /// <returns></returns>
        public static int GetIntFromParameters(this HttpContext context, string key)
        {
            var value = default(int);
            if (!string.IsNullOrEmpty(key) && context != null)
            {
                var stringValue = context.Request.QueryString[key];
                if (string.IsNullOrEmpty(stringValue)) stringValue = context.Request.Form[key];
                value = (!string.IsNullOrEmpty(stringValue)
                         && IsNumeric(stringValue))
                            ? int.Parse(stringValue)
                            : default(int);
            }
            return value;
        }

        /// <summary>
        /// Get Querystring params of DateTime type.
        /// </summary>
        /// <param name="context">request context</param>
        /// <param name="key">params key</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromParameters(this HttpContext context, string key)
        {
            var value = new DateTime(1900, 01, 01);
            if (string.IsNullOrEmpty(key) || context == null)
            {
                return value;
            }
            var stringValue = context.Request.QueryString[key];
            if (string.IsNullOrEmpty(stringValue)) stringValue = context.Request.Form[key];
            return !DateTime.TryParse(stringValue, out value) ? new DateTime(1900, 1, 1) : value;
        }

        /// <summary>
        /// Get collection of int.
        /// </summary>
        /// <param name="context">request context</param>
        /// <param name="key">params key</param>
        /// <param name="separator">split char</param>
        /// <returns></returns>
        public static List<int> GetListIntFromParameters(this HttpContext context, string key, char separator)
        {
            var strList = context.GetStringFromParameters(key);
            if (string.IsNullOrEmpty(strList))
            {
                return null;
            }
            var list = new List<int>();
            foreach (var item in strList.Split(separator))
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                if (Int32.TryParse(item, out var id))
                {
                    if (list.Contains(id))
                    {
                        continue;
                    }
                    list.Add(id);
                }
            }
            return list;
        }
        #endregion

        #region [2、请求远程接口]

        /// <summary>
        /// Async get http data.
        /// </summary>
        /// <param name="url">request url.</param>
        /// <param name="encoding">encoding type.</param>
        /// <returns>Get url data result is string</returns>
        public static async Task<string> HttpGetAsync(string url, Encoding encoding = null)
        {
            HttpClient httpClient = new HttpClient();
            var data = await httpClient.GetByteArrayAsync(url);
            encoding = encoding ?? Encoding.UTF8;
            var ret = encoding.GetString(data);
            return ret;
        }

        /// <summary>
        /// Get http data.
        /// </summary>
        /// <param name="url">request url.</param>
        /// <param name="encoding">encoding type.</param>
        /// <param name="timeOut">超时时间秒</param>
        /// <returns>Get url data result is string</returns>
        public static string HttpGet(string url, Encoding encoding = null, int timeOut = 3)
        {
            try
            {
                timeOut = timeOut < 1 ? 3 : timeOut;
                encoding = encoding ?? Encoding.UTF8;
                HttpWebRequest httpwebreq;
                httpwebreq = (HttpWebRequest)WebRequest.Create(url);
                httpwebreq.Timeout = timeOut * 60 * 1000;
                var httpwebrsp = (HttpWebResponse)httpwebreq.GetResponse();
                var streamReader = httpwebrsp.GetResponseStream();
                if (streamReader != null)
                {
                    var sr = new StreamReader(streamReader, encoding);
                    var resContent = sr.ReadToEnd();
                    sr.Close();
                    httpwebrsp.Close();
                    return resContent;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Async post data to url.
        /// </summary>
        /// <param name="url">request url</param>
        /// <param name="formData">formData,the key is string ,value is object,but the best choose if int or string</param>
        /// <param name="encoding">encoding default(UTF8)</param>
        /// <param name="timeOut">http request timeout.</param>
        /// <returns>response data of string</returns>
        public static async Task<string> HttpPostAsync(string url, Dictionary<string, object> formData = null,
            Encoding encoding = null, int timeOut = 3000)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            MemoryStream ms = new MemoryStream();
            //填充formData数据
            formData.FillFormDataStream(ms);
            HttpContent hc = new StreamContent(ms);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
            hc.Headers.Add("UserAgent",
                "Mozilla/5.0(Window NT 6.1;WOW64) AppleWebKit/573.36 (KHTML,like Gecko) Chrome/31.0.1650.57 Safari/537.36");
            hc.Headers.Add("TimeOut", timeOut.ToString());
            hc.Headers.Add("KeepAlive", "true");

            var r = await client.PostAsync(url, hc);
            byte[] tmp = await r.Content.ReadAsByteArrayAsync();
            return (encoding ?? Encoding.UTF8).GetString(tmp);
        }

        /// <summary>
        /// Post data to url.
        /// </summary>
        /// <param name="url">request url</param>
        /// <param name="formData">formData,the key is string ,value is object,but the best choose if int or string</param>
        /// <param name="encoding">encoding default(UTF8)</param>
        /// <param name="timeOut">http request timeout.</param>
        /// <returns>response data of string</returns>
        public static string HttpPost(string url, Dictionary<string, object> formData = null,
            Encoding encoding = null, int timeOut = 3000)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            MemoryStream ms = new MemoryStream();
            //填充formData数据
            formData.FillFormDataStream(ms);
            HttpContent hc = new StreamContent(ms);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
            hc.Headers.Add("UserAgent",
                "Mozilla/5.0(Window NT 6.1;WOW64) AppleWebKit/573.36 (KHTML,like Gecko) Chrome/31.0.1650.57 Safari/537.36");
            hc.Headers.Add("TimeOut", timeOut.ToString());
            hc.Headers.Add("KeepAlive", "true");

            var r = client.PostAsync(url, hc);
            r.Wait();
            var tmp = r.Result.Content.ReadAsByteArrayAsync();
            return (encoding ?? Encoding.UTF8).GetString(tmp.Result);
        }

        /// <summary>
        /// <para>组装QueryString的方法</para>
        /// <para>参数之间用and连接，首位没有符号，如：a=1 and b=2 and c=3</para>
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        private static string GetQueryString(this Dictionary<string, object> formData)
        {
            if (formData == null || formData.Count == 0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            var i = 0;
            foreach (var kv in formData)
            {
                i++;
                sb.AppendFormat("{0}={1}", kv.Key, kv.Value);
                if (i < formData.Count)
                {
                    sb.Append("&");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 填充表单信息的Stream
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="stream"></param>
        private static void FillFormDataStream(this Dictionary<string, object> formData, Stream stream)
        {
            string dataString = GetQueryString(formData);
            var formDataBytes = formData == null ? new byte[0] : Encoding.UTF8.GetBytes(dataString);
            stream.Write(formDataBytes, 0, formDataBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);//设置指针读取位置
        }

        /// <summary>
        /// Post https request
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="postData">post data</param>
        /// <param name="cookies">cookies</param>
        /// <returns></returns>
        public static string HttpsPost(string url, string postData, Dictionary<string, Cookie> cookies = null)
        {
            HttpWebRequest request;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                request = WebRequest.Create(url) as HttpWebRequest;
                if (request == null) return string.Empty;
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                request.ProtocolVersion = HttpVersion.Version11;
                // 这里设置了协议类型。
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2; 
                request.KeepAlive = false;
                ServicePointManager.CheckCertificateRevocationList = true;
                ServicePointManager.DefaultConnectionLimit = 100;
                ServicePointManager.Expect100Continue = false;
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(url);
            }

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Referer = null;
            request.AllowAutoRedirect = true;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            request.Accept = "*/*";
            if (cookies != null && cookies.Any())
            {
                CookieContainer cc = new CookieContainer();
                foreach (var cookie in cookies)
                {
                    cc.Add(new Uri(cookie.Key), cookie.Value);
                }
                request.CookieContainer = cc;
            }

            byte[] data = Encoding.UTF8.GetBytes(postData);
            Stream newStream = request.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            //获取网页响应结果
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            if (stream == Stream.Null || stream == null) return string.Empty;
            string result;
            using (StreamReader sr = new StreamReader(stream))
            {
                result = sr.ReadToEnd();
            }
            return result;
        }

        /// <summary>
        /// Validation check method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        /// <summary>
        /// 将本地文件上传到指定的地址(HttpWebRequest方法)
        /// </summary>
        /// <param name="address">文件上传到的服务器</param>
        /// <param name="fileNamePath">要上传的本地文件（全路径）</param>
        /// <param name="saveName">文件上传后的名称</param>
        /// <param name="attachName">附加值名称</param>
        /// <param name="attachValue">附加值的值</param>
        /// <returns>成功返回1，失败返回0</returns>
        public static string PostFileAndValue(string address, string fileNamePath, string saveName, string attachName, string attachValue)
        {
            if (string.IsNullOrEmpty(address)) return null;
            if (!File.Exists(fileNamePath))
            {
                return null;
            }
            // 要上传的文件
            FileStream fs = new FileStream(fileNamePath, FileMode.Open, FileAccess.Read);
            long size = fs.Length;
            byte[] array = new byte[size];
            fs.Read(array, 0, array.Length);
            fs.Close();
            var filelist = new List<UploadFile>
            {
                new UploadFile
                {
                    Data = array,
                    Filename = saveName,
                    Name = "upfile"
                }
            };
            NameValueCollection valuesCollection = new NameValueCollection
            {
                {attachName, attachValue}
            };
            var result = PostFileAndValue(address, filelist, valuesCollection);
            return result;
        }

        /// <summary>
        /// 以Post 形式提交数据到 uri，一次提交全部数据
        /// </summary>  
        /// <param name="uri">请求地址</param>
        /// <param name="files">文件列表</param>
        /// <param name="values">附加值</param>
        /// <returns></returns>
        public static string PostFileAndValue(string uri, IEnumerable<UploadFile> files, NameValueCollection values)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;

            MemoryStream stream = new MemoryStream();
            byte[] line = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            //提交文本字段  
            if (values != null)
            {
                string format = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
                foreach (string key in values.Keys)
                {
                    string s = string.Format(format, key, values[key]);
                    byte[] data = Encoding.UTF8.GetBytes(s);
                    stream.Write(data, 0, data.Length);
                }
                stream.Write(line, 0, line.Length);
            }
            //提交文件
            var uploadFiles = files as IList<UploadFile> ?? files.ToList();
            if (uploadFiles.Any())
            {
                string fformat = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n Content-Type: application/octet-stream\r\n\r\n";
                foreach (UploadFile file in uploadFiles)
                {
                    string s = string.Format(fformat, file.Name, file.Filename);
                    byte[] data = Encoding.UTF8.GetBytes(s);
                    stream.Write(data, 0, data.Length);
                    stream.Write(file.Data, 0, file.Data.Length);
                    stream.Write(line, 0, line.Length);
                }
            }
            request.ContentLength = stream.Length;
            request.Timeout = 1000 * 60 * 10;
            Stream requestStream = request.GetRequestStream();
            stream.Position = 0L;
            stream.CopyTo(requestStream);
            stream.Close();
            requestStream.Close();
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) return null;
                    //读取服务器端返回的消息
                    StreamReader sr = new StreamReader(responseStream);
                    String sReturnString = sr.ReadLine();
                    return sReturnString;
                }
            }
        }

        /// <summary>
        /// 文件分片上传文件
        /// </summary>
        /// <param name="uploadHost">接口地址</param>
        /// <param name="filePath">文件全路径</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static UploadResult UploadFileToServer(string uploadHost, string filePath, string fileName)
        {
            var resultModel = new UploadResult { State = UploadState.Unknown };
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return resultModel;
            }
            if (string.IsNullOrEmpty(uploadHost))
            {
                return resultModel;
            }
            var fileGuId = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Path.GetFileName(filePath);
            }

            // 要上传的文件
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            //每次读取4M字节大小的数据
            var fileBuffer = new byte[1024 * 1024 * 4];
            long totalLength = fs.Length;
            var sendTime = totalLength / fileBuffer.Length + (totalLength % fileBuffer.Length > 0 ? 1 : 0);
            var partIndex = 0;
            while (totalLength > 0)
            {
                partIndex++;
                var length = fs.Read(fileBuffer, 0, fileBuffer.Length);
                totalLength = totalLength - length;
                NameValueCollection valuesCollection = new NameValueCollection
                {
                    {"fun", "003"},
                    {"guId", fileGuId},
                    {"partCount", sendTime.ToString()},
                    {"partIndex", partIndex.ToString()},
                    {"fileName", fileName}
                };

                string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uploadHost);
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                request.Method = "POST";
                request.KeepAlive = true;
                request.Credentials = CredentialCache.DefaultCredentials;

                Stream requestStream = request.GetRequestStream();

                //MemoryStream stream = new MemoryStream();
                byte[] line = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                //提交文本字段
                string format = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
                foreach (string key in valuesCollection.Keys)
                {
                    string s = string.Format(format, key, valuesCollection[key]);
                    byte[] data = Encoding.UTF8.GetBytes(s);
                    requestStream.Write(data, 0, data.Length);
                }
                requestStream.Write(line, 0, line.Length);

                //写文件信息
                string fformat = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n Content-Type: application/octet-stream\r\n\r\n";
                string fileInfo = string.Format(fformat, fileName, fileName);
                byte[] fileNameData = Encoding.UTF8.GetBytes(fileInfo);
                requestStream.Write(fileNameData, 0, fileNameData.Length);

                //数据传输
                requestStream.Write(fileBuffer, 0, length);
                requestStream.Write(line, 0, line.Length);

                request.Timeout = 1000 * 60 * 100;
                requestStream.Close();
                try
                {
                    using (var response = request.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            if (responseStream == null) return null;
                            //读取服务器端返回的消息
                            StreamReader sr = new StreamReader(responseStream);
                            string sReturnString = sr.ReadLine();
                            if (string.IsNullOrEmpty(sReturnString))
                            {
                                resultModel.State = UploadState.NetworkError;
                                resultModel.ErrorMessage = "服务器返回为空";
                            }
                            else
                            {
                                resultModel = JsonConvert.DeserializeObject<UploadResult>(sReturnString);
                            }
                            if (resultModel.State != UploadState.Success)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    resultModel.State = UploadState.Unknown;
                    resultModel.ErrorMessage = e.Message;
                    break;
                }
            }
            fs.Close();

            if (resultModel.State != UploadState.Success)
            {
                return resultModel;
            }

            uploadHost += $"?fun=004&guId={fileGuId}&partCount={sendTime}&fileName={fileName}";

            UploadResult httpResultModel = new UploadResult();
            try
            {
                var finishStr = HttpGet(uploadHost);
                httpResultModel = JsonConvert.DeserializeObject<UploadResult>(finishStr);
            }
            catch (Exception e)
            {
                httpResultModel.ErrorMessage = e.Message;
            }
            resultModel = httpResultModel;
            return resultModel;
        }
        #endregion
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    public class UploadFile
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public UploadFile()
        {
            ContentType = "application/octet-stream";
        }
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// 请求文件类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 文件流
        /// </summary>
        public byte[] Data { get; set; }
    }
}
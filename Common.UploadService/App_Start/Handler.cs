using System;
using System.Web;
using Newtonsoft.Json;

namespace Common.UploadService
{
    /// <summary>
    /// Handler 的摘要说明
    /// </summary>
    public abstract class Handler
    {
        /// <summary>
        /// 上传配置
        /// </summary>
        public UploadConfig UploadConfig { get; set; }

        /// <summary>
        /// 上传结果
        /// </summary>
        public UploadResult Result { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
        protected Handler(HttpContext context)
        {
            Request = context.Request;
            Response = context.Response;
            Context = context;
            Server = context.Server;
        }

        public abstract void Process();

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="response"></param>
        protected void WriteJson(object response)
        {
            string jsonpCallback = Request["callback"],
                json = JsonConvert.SerializeObject(response);
            if (String.IsNullOrWhiteSpace(jsonpCallback))
            {
                Response.AddHeader("Content-Type", "text/plain");
                Response.Write(json);
            }
            else
            {
                Response.AddHeader("Content-Type", "application/javascript");
                Response.Write(String.Format("{0}({1});", jsonpCallback, json));
            }
            Response.End();
        }

        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }
        public HttpContext Context { get; private set; }
        public HttpServerUtility Server { get; private set; }
    }
}
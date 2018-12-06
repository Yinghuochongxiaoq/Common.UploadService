using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace Common.UploadService.HandlerFile
{
    /// <summary>
    /// HandlerFile 的摘要说明
    /// </summary>
    public class HandlerFile : IHttpHandler
    {
        /// <summary>
        /// 文件处理模块
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Cache-Control", "private, no-cache");
            context.Response.Charset = "utf-8";
            context.Response.ContentEncoding = Encoding.GetEncoding("utf-8");
            Handler action = null;

            var fun = context.GetStringFromParameters("fun");
            switch (fun)
            {
                case "001":
                    action = new UploadHandler(context, new UploadConfig
                    {
                        AllowExtensions = Config.GetStringList("imageAllowFiles"),
                        PathFormat = Config.GetString("imagePathFormat"),
                        SizeLimit = Config.GetInt("imageMaxSize"),
                        UploadFieldName = Config.GetString("imageFieldName"),
                        PreUrl = Config.GetString("imageUrlPrefix")
                    });
                    break;
                //获取文件记录
                case "002":
                    action = new GetFileHandler(context);
                    break;
                //接收文件
                case "003":
                    action = new UploadPartHandler(context, new UploadConfig
                    {
                        AllowExtensions = Config.GetStringList("imageAllowFiles"),
                        PathFormat = Config.GetString("imagePathFormat"),
                        SizeLimit = Config.GetInt("imageMaxSize"),
                        UploadFieldName = Config.GetString("imageFieldName"),
                        PreUrl = Config.GetString("imageUrlPrefix")
                    });
                    break;
                //通知文件传输结束
                case "004":
                    action = new FinishuploadHandler(context, new UploadConfig
                    {
                        AllowExtensions = Config.GetStringList("imageAllowFiles"),
                        PathFormat = Config.GetString("imagePathFormat"),
                        SizeLimit = Config.GetInt("imageMaxSize"),
                        UploadFieldName = Config.GetString("imageFieldName"),
                        PreUrl = Config.GetString("imageUrlPrefix")
                    });
                    break;
                //测试通信
                default:
                    var result = new UploadResult { State = UploadState.Success, Url = "测试通信" };
                    var returnValue = JsonConvert.SerializeObject(result);
                    context.Response.Write(returnValue);
                    context.Response.End();
                    break;
            }
            action?.Process();

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }
}
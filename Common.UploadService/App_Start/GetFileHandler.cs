using System;
using System.IO;
using System.Text;
using System.Web;

namespace Common.UploadService
{
    /// <summary>
    /// 获取文件
    /// </summary>
    public class GetFileHandler : Handler
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
        public GetFileHandler(HttpContext context) : base(context)
        {
            Result = new UploadResult { State = UploadState.Unknown };
        }

        /// <summary>
        /// 处理文件
        /// </summary>
        public override void Process()
        {
            string outFilePath = Context.GetStringFromParameters("filePath");
            outFilePath = AppDomain.CurrentDomain.BaseDirectory + "/" + outFilePath;
            SendStreamInfoToWeb(Context, outFilePath);
        }

        /// <summary>
        /// 发送文件流到请求中
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名称</param>
        private void SendStreamInfoToWeb(HttpContext context, string filePath, string fileName = null)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = Path.GetFileName(filePath);
                }
                var response = context.Response;
                var fileBuffer = new byte[1024];//每次读取1024字节大小的数据
                long totalLength = fs.Length;

                response.ContentType = "application/octet-stream";
                response.AddHeader("Content-Disposition", "attachment;  filename=" + HttpUtility.UrlEncode(fileName, Encoding.UTF8));

                while (totalLength > 0 && response.IsClientConnected)
                {
                    int length = fs.Read(fileBuffer, 0, fileBuffer.Length);//每次读取1024个字节长度的内容
                    fs.Flush();
                    response.OutputStream.Write(fileBuffer, 0, length);//写入到响应的输出流
                    response.Flush();//刷新响应
                    totalLength = totalLength - length;
                }

                fs.Close();
                response.End();
            }
        }
    }
}
using System;
using System.IO;
using System.Web;

namespace Common.UploadService
{
    /// <summary>
    /// 上传部分文件
    /// </summary>
    public class UploadPartHandler : Handler
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        public UploadPartHandler(HttpContext context, UploadConfig config) : base(context)
        {
            UploadConfig = config;
            Result = new UploadResult { State = UploadState.Unknown };
        }

        /// <summary>
        /// 处理
        /// </summary>
        public override void Process()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var guIdFileName = Context.GetStringFromParameters("guId");
            var partIndex = Context.GetIntFromParameters("partIndex");
            if (Context.Request.Files.Count < 1)
            {
                Result.State = UploadState.FileAccessError;
                WriteJson(Result);
            }
            HttpPostedFile postedFile = Context.Request.Files[0];
            if (string.IsNullOrEmpty(postedFile.FileName))
            {
                Result.State = UploadState.FileAccessError;
                WriteJson(Result);
            }
            var filePostName = postedFile.FileName;
            string partFileUpload = "/Uploadfile/PartFile";
            string path = basePath + partFileUpload;

            if (!UploadConfig.CheckFileType(filePostName))
            {
                Result.State = UploadState.TypeNotAllow;
                WriteJson(Result);
            }

            if (!UploadConfig.CheckFileSize(postedFile.ContentLength))
            {
                Result.State = UploadState.SizeLimitExceed;
                WriteJson(Result);
            }
            string fileName = guIdFileName + ".part" + partIndex;
            string fpath = path + "/" + fileName;
            //如果不存在该目录，则创建目录
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Stream reader = postedFile.InputStream;
            var buffersize = 1024;
            FileStream fStream = new FileStream(fpath, FileMode.Create, FileAccess.Write, FileShare.Read, buffersize);
            byte[] buffer = new byte[buffersize];
            int len = reader.Read(buffer, 0, buffersize);

            while (len > 0)
            {
                fStream.Write(buffer, 0, len);
                len = reader.Read(buffer, 0, buffersize);
            }
            reader.Close();
            fStream.Close();
            string rmsg = partFileUpload + "/" + fileName;
            Result.State = UploadState.Success;
            Result.Url = rmsg;
            Result.FullUrl = UploadConfig.PreUrl + rmsg;
            WriteJson(Result);
        }
    }
}
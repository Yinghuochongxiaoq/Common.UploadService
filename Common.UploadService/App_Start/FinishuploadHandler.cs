using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Common.UploadService
{
    /// <summary>
    /// 文件上传结束
    /// </summary>
    public class FinishuploadHandler : Handler
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        public FinishuploadHandler(HttpContext context, UploadConfig config) : base(context)
        {
            UploadConfig = config;
            Result = new UploadResult { State = UploadState.Unknown };
        }

        public override void Process()
        {
            var guIdFileName = Context.GetStringFromParameters("guId");
            var partCount = Context.GetIntFromParameters("partCount");
            var filePostName = Context.GetStringFromParameters("fileName");

            string partFileUpload = "/Uploadfile/PartFile";
            var savePath = PathFormatter.Format(filePostName, UploadConfig.PathFormat);
            var localPath = Server.MapPath(savePath);
            //分片文件列表
            var partFileList = new List<string>();
            if (!UploadConfig.CheckFileType(filePostName))
            {
                Result.State = UploadState.TypeNotAllow;
                WriteJson(Result);
            }
            if (string.IsNullOrEmpty(localPath))
            {
                Result.State = UploadState.Unknown;
                WriteJson(Result);
                return;
            }
            //如果不存在该目录，则创建目录
            if (!Directory.Exists(Path.GetDirectoryName(localPath)))
            {
                Result.State = UploadState.FileAccessError;
                WriteJson(Result);
            }
            for (int i = 1; i <= partCount; i++)
            {
                string fileName = guIdFileName + ".part" + i;
                string fpath = partFileUpload + "/" + fileName;
                fpath = Server.MapPath(fpath);
                if (!File.Exists(fpath))
                {
                    Result.State = UploadState.FileAccessError;
                    WriteJson(Result);
                }
                partFileList.Add(fpath);
            }
            if (partFileList.Count < 1)
            {
                Result.State = UploadState.FileAccessError;
                WriteJson(Result);
            }


            var buffersize = 1024;
            //最终文件
            FileStream fStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.Read, buffersize);
            byte[] buffer = new byte[buffersize];

            partFileList.ForEach(f =>
            {
                // 要上传的文件
                FileStream fs = new FileStream(f, FileMode.Open, FileAccess.Read);
                long totalLength = fs.Length;
                while (totalLength > 0)
                {
                    var length = fs.Read(buffer, 0, buffer.Length);
                    fStream.Write(buffer, 0, length);
                    totalLength = totalLength - length;
                }
                fs.Close();
            });
            fStream.Close();
            Result.State = UploadState.Success;
            Result.Url = savePath;
            Result.FullUrl = UploadConfig.PreUrl + savePath;
            if (partFileList.Count > 0)
            {
                try
                {
                    partFileList.ForEach(r =>
                    {
                        if (File.Exists(r))
                        {
                            File.Delete(r);
                        }
                    });
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            WriteJson(Result);
        }
    }
}
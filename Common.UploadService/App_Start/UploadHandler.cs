using System;
using System.IO;
using System.Web;

namespace Common.UploadService
{
    /// <summary>
    /// 文件上传
    /// </summary>
    public class UploadHandler : Handler
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        public UploadHandler(HttpContext context, UploadConfig config) : base(context)
        {
            UploadConfig = config;
            Result = new UploadResult { State = UploadState.Unknown, Others = config.Others };
        }

        /// <summary>
        /// 执行文件上传
        /// </summary>
        public override void Process()
        {
            byte[] uploadFileBytes;
            string uploadFileName;

            if (UploadConfig.Base64)
            {
                uploadFileName = UploadConfig.Base64Filename;
                uploadFileBytes = Convert.FromBase64String(Request[UploadConfig.UploadFieldName]);
            }
            else
            {
                var file = Request.Files[UploadConfig.UploadFieldName];
                if (file == null)
                {
                    Result.State = UploadState.FileAccessError;
                    WriteJson(Result);
                    return;
                }
                uploadFileName = file.FileName;

                if (!UploadConfig.CheckFileType(uploadFileName))
                {
                    Result.State = UploadState.TypeNotAllow;
                    WriteJson(Result);
                    return;
                }
                if (!UploadConfig.CheckFileSize(file.ContentLength))
                {
                    Result.State = UploadState.SizeLimitExceed;
                    WriteJson(Result);
                    return;
                }

                uploadFileBytes = new byte[file.ContentLength];
                try
                {
                    file.InputStream.Read(uploadFileBytes, 0, file.ContentLength);
                }
                catch (Exception)
                {
                    Result.State = UploadState.NetworkError;
                    WriteJson(Result);
                    return;
                }
            }

            Result.OriginFileName = uploadFileName;

            var savePath = PathFormatter.Format(uploadFileName, UploadConfig.PathFormat);
            var localPath = Server.MapPath(savePath);
            try
            {
                if (string.IsNullOrEmpty(localPath))
                {
                    Result.State = UploadState.Unknown;
                    return;
                }
                if (!Directory.Exists(Path.GetDirectoryName(localPath)))
                {
                    var diskPath = Path.GetDirectoryName(localPath);
                    if (string.IsNullOrEmpty(diskPath))
                    {
                        Result.State = UploadState.Unknown;
                        return;
                    }
                    Directory.CreateDirectory(diskPath);
                }
                File.WriteAllBytes(localPath, uploadFileBytes);
                Result.Url = savePath;
                Result.FullUrl = UploadConfig.PreUrl + savePath;
                Result.State = UploadState.Success;
            }
            catch (Exception e)
            {
                Result.State = UploadState.FileAccessError;
                Result.ErrorMessage = e.Message;
            }
            finally
            {
                WriteJson(Result);
            }
        }
    }
}
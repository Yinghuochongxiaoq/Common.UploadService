using System.IO;
using System.Linq;

namespace Common.UploadService
{
    public class UploadConfig
    {
        /// <summary>
        /// 文件命名规则
        /// </summary>
        public string PathFormat { get; set; }

        /// <summary>
        /// 上传表单域名称
        /// </summary>
        public string UploadFieldName { get; set; }

        /// <summary>
        /// 上传大小限制
        /// </summary>
        public int SizeLimit { get; set; }

        /// <summary>
        /// 上传允许的文件格式
        /// </summary>
        public string[] AllowExtensions { get; set; }

        /// <summary>
        /// 文件是否以 Base64 的形式上传
        /// </summary>
        public bool Base64 { get; set; }

        /// <summary>
        /// Base64 字符串所表示的文件名
        /// </summary>
        public string Base64Filename { get; set; }

        /// <summary>
        /// 返回图片前缀
        /// </summary>
        public string PreUrl { get; set; }

        /// <summary>
        /// 检测文件类型
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool CheckFileType(string filename)
        {
            var fileExtension = Path.GetExtension(filename)?.ToLower();
            if (fileExtension == null) return false;
            return AllowExtensions.Select(x => x.ToLower()).Contains(fileExtension);
        }

        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GetFileType(string filename)
        {
            return Path.GetExtension(filename)?.ToLower();
        }

        /// <summary>
        /// 检测文件大小
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool CheckFileSize(int size)
        {
            return size < SizeLimit;
        }
    }
}
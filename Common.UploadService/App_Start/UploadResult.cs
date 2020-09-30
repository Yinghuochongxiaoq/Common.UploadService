namespace Common.UploadService
{
    /// <summary>
    /// 上传结果
    /// </summary>
    public class UploadResult
    {
        /// <summary>
        /// 结果状态
        /// </summary>
        public UploadState State { get; set; }
        /// <summary>
        /// url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 全路径
        /// </summary>
        public string FullUrl { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string OriginFileName { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 或者状态信息
        /// </summary>
        public string StateStr => GetStateMessage(State);
        /// <summary>
        /// 其他信息
        /// </summary>
        public string Others { get; set; }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private string GetStateMessage(UploadState state)
        {
            switch (state)
            {
                case UploadState.Success:
                    return "SUCCESS";
                case UploadState.FileAccessError:
                    return "文件访问出错，请检查写入权限";
                case UploadState.SizeLimitExceed:
                    return "文件大小超出服务器限制";
                case UploadState.TypeNotAllow:
                    return "不允许的文件格式";
                case UploadState.NetworkError:
                    return "网络错误";
            }
            return "未知错误";
        }
    }
}
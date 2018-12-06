namespace Common.UploadService
{
    /// <summary>
    /// 上传状态
    /// </summary>
    public enum UploadState
    {
        Success = 0,
        SizeLimitExceed = -1,
        TypeNotAllow = -2,
        FileAccessError = -3,
        NetworkError = -4,
        Unknown = 1,
    }
}
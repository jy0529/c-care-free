namespace CDriveCleanupMaster.Domain.Services;

/// <summary>
/// 统一控制大文件筛选阈值。
/// </summary>
public static class LargeFileThresholdPolicy
{
    public static bool ShouldInclude(long bytes, long thresholdBytes)
    {
        return bytes >= thresholdBytes;
    }
}

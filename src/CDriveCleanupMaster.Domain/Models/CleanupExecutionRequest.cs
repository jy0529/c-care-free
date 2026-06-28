namespace CDriveCleanupMaster.Domain.Models;

/// <summary>
/// 清理执行请求。
/// </summary>
public sealed class CleanupExecutionRequest
{
    public required string DriveRoot { get; init; }
    public required IReadOnlyList<CleanupCategoryResult> Categories { get; init; }
}

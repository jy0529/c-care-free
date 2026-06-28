using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Application.Abstractions;

/// <summary>
/// 垃圾清理执行接口。
/// </summary>
public interface ICleanupExecutor
{
    Task<CleanupExecutionResult> ExecuteAsync(CleanupExecutionRequest request, CancellationToken cancellationToken);
}

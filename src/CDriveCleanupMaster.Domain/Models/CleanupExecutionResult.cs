namespace CDriveCleanupMaster.Domain.Models;

public sealed class CleanupExecutionItemResult
{
    public required string CategoryId { get; init; }
    public required string DisplayName { get; init; }
    public required bool Success { get; init; }
    public required long BytesFreed { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// 清理执行结果。
/// </summary>
public sealed class CleanupExecutionResult
{
    public required long TotalBytesFreed { get; init; }
    public required IReadOnlyList<CleanupExecutionItemResult> Items { get; init; }
}

namespace CDriveCleanupMaster.Domain.Models;

/// <summary>
/// 单个大文件信息。
/// </summary>
public sealed class LargeFileItem
{
    public required string Path { get; init; }
    public required string Name { get; init; }
    public required string Extension { get; init; }
    public required long Bytes { get; init; }
    public required DateTimeOffset ModifiedAt { get; init; }
    public string? RelocationHint { get; init; }
}

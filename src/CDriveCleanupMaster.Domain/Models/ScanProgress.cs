namespace CDriveCleanupMaster.Domain.Models;

public sealed class ScanProgress
{
    public required string Phase { get; init; }
    public required int Percent { get; init; }
    public int? LargeFileCount { get; init; }
    public long? LargestBytesFound { get; init; }
    public IReadOnlyList<LargeFileItem>? LargeFilePreview { get; init; }
}

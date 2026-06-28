namespace CDriveCleanupMaster.Domain.Models;

public sealed class LargeFileScanOptions
{
    public required long ThresholdBytes { get; init; }
    public int MaxResultCount { get; init; } = 500;
}

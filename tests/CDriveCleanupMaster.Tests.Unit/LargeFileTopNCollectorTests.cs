using CDriveCleanupMaster.Domain.Models;
using CDriveCleanupMaster.Domain.Services;

namespace CDriveCleanupMaster.Tests.Unit;

public sealed class LargeFileTopNCollectorTests
{
    [Fact]
    public void TryAdd_ShouldKeepOnlyTopNBySize()
    {
        var collector = new LargeFileTopNCollector(3);

        collector.TryAdd(CreateItem("small.bin", 100));
        collector.TryAdd(CreateItem("medium.bin", 500));
        collector.TryAdd(CreateItem("large.bin", 900));
        collector.TryAdd(CreateItem("tiny.bin", 50));
        collector.TryAdd(CreateItem("huge.bin", 1000));

        var result = collector.ToOrderedArray();

        Assert.Equal(3, result.Length);
        Assert.Equal("huge.bin", result[0].Name);
        Assert.Equal("large.bin", result[1].Name);
        Assert.Equal("medium.bin", result[2].Name);
        Assert.Equal(5, collector.TotalMatched);
    }

    [Fact]
    public void TryAdd_ShouldBeThreadSafe()
    {
        var collector = new LargeFileTopNCollector(10);
        const int fileCount = 500;

        Parallel.For(0, fileCount, index =>
        {
            collector.TryAdd(CreateItem($"file-{index}.bin", index));
        });

        Assert.Equal(fileCount, collector.TotalMatched);
        Assert.Equal(10, collector.Count);
        Assert.Equal(10, collector.ToOrderedArray().Length);
        Assert.Equal("file-499.bin", collector.ToOrderedArray()[0].Name);
    }

    private static LargeFileItem CreateItem(string name, long bytes) =>
        new()
        {
            Path = $@"C:\temp\{name}",
            Name = name,
            Extension = ".bin",
            Bytes = bytes,
            ModifiedAt = DateTimeOffset.UtcNow
        };
}

using CDriveCleanupMaster.Domain.Models;
using CDriveCleanupMaster.Domain.Services;

namespace CDriveCleanupMaster.Tests.Unit;

public sealed class LargeFileRelocationMatcherTests
{
    [Theory]
    [InlineData(@"C:\Users\Jy\AppData\Local\Docker\wsl\disk\docker_data.vhdx", true)]
    [InlineData(@"C:\Users\Jy\AppData\Local\Packages\CanonicalGroupLimited...\LocalState\ext4.vhdx", true)]
    [InlineData(@"C:\Windows\hiberfil.sys", true)]
    [InlineData(@"C:\random\file.txt", false)]
    public void GetHint_ShouldMatchKnownRelocationTargets(string path, bool shouldHaveHint)
    {
        string? hint = LargeFileRelocationMatcher.GetHint(path);

        if (shouldHaveHint)
        {
            Assert.False(string.IsNullOrWhiteSpace(hint));
        }
        else
        {
            Assert.Null(hint);
        }
    }

    [Fact]
    public void WithRelocationHint_ShouldPreserveFileMetadata()
    {
        var item = new LargeFileItem
        {
            Path = @"C:\Docker\wsl\data\ext4.vhdx",
            Name = "ext4.vhdx",
            Extension = ".vhdx",
            Bytes = 1024,
            ModifiedAt = DateTimeOffset.UtcNow
        };

        LargeFileItem result = LargeFileRelocationMatcher.WithRelocationHint(item);

        Assert.Equal(item.Path, result.Path);
        Assert.Equal(item.Bytes, result.Bytes);
        Assert.NotNull(result.RelocationHint);
    }
}

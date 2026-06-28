using CDriveCleanupMaster.Domain.Services;

namespace CDriveCleanupMaster.Tests.Unit;

public sealed class ScanPathSkipPolicyTests
{
    [Theory]
    [InlineData(@"C:\$Recycle.Bin", true)]
    [InlineData(@"C:\System Volume Information", true)]
    [InlineData(@"C:\Users\Jy\Downloads", false)]
    public void ShouldSkipDirectory_ShouldMatchKnownSystemFolders(string path, bool expected)
    {
        Assert.Equal(expected, ScanPathSkipPolicy.ShouldSkipDirectory(path));
    }
}

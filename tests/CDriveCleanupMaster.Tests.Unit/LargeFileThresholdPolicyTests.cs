using CDriveCleanupMaster.Domain.Services;

namespace CDriveCleanupMaster.Tests.Unit;

public sealed class LargeFileThresholdPolicyTests
{
    [Fact]
    public void ShouldInclude_ShouldReturnTrue_WhenThresholdIsMet()
    {
        Assert.True(LargeFileThresholdPolicy.ShouldInclude(1024, 1024));
    }

    [Fact]
    public void ShouldInclude_ShouldReturnFalse_WhenThresholdIsNotMet()
    {
        Assert.False(LargeFileThresholdPolicy.ShouldInclude(1023, 1024));
    }
}

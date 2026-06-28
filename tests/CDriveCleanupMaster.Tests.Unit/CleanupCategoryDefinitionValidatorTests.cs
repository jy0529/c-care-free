using CDriveCleanupMaster.Domain.Enums;
using CDriveCleanupMaster.Domain.Models;
using CDriveCleanupMaster.Domain.Services;

namespace CDriveCleanupMaster.Tests.Unit;

public sealed class CleanupCategoryDefinitionValidatorTests
{
    [Fact]
    public void Validate_ShouldThrow_WhenDefaultSelectionIsNotLowRisk()
    {
        var definition = new CleanupCategoryDefinition
        {
            Id = "downloads",
            DisplayName = "Downloads",
            Description = "Downloads",
            GroupName = "Review",
            IconKey = "Download",
            RiskLevel = RiskLevel.Medium,
            IsExecutable = false,
            IsSelectedByDefault = true,
            Paths = ["C:\\Downloads"]
        };

        Assert.Throws<ArgumentException>(() => CleanupCategoryDefinitionValidator.Validate(definition));
    }
}

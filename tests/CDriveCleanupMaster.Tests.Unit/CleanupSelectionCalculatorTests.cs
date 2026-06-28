using CDriveCleanupMaster.Domain.Enums;
using CDriveCleanupMaster.Domain.Models;
using CDriveCleanupMaster.Domain.Services;

namespace CDriveCleanupMaster.Tests.Unit;

public sealed class CleanupSelectionCalculatorTests
{
    [Fact]
    public void CalculateSelectedBytes_ShouldSumOnlySelectedItems()
    {
        var items = new[]
        {
            CreateItem("a", 100, true),
            CreateItem("b", 200, false),
            CreateItem("c", 300, true)
        };

        long result = CleanupSelectionCalculator.CalculateSelectedBytes(items);

        Assert.Equal(400, result);
    }

    private static CleanupCategoryResult CreateItem(string id, long bytes, bool isSelected)
    {
        return new CleanupCategoryResult
        {
            Definition = new CleanupCategoryDefinition
            {
                Id = id,
                DisplayName = id,
                Description = id,
                GroupName = "g",
                IconKey = "i",
                RiskLevel = RiskLevel.Low,
                IsExecutable = true,
                IsSelectedByDefault = isSelected,
                Paths = ["C:\\Temp"]
            },
            TotalBytes = bytes,
            FileCount = 1,
            ResolvedPaths = ["C:\\Temp"],
            IsSelected = isSelected
        };
    }
}

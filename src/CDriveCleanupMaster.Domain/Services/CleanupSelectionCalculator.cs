using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Domain.Services;

/// <summary>
/// 计算当前选中项总空间。
/// </summary>
public static class CleanupSelectionCalculator
{
    public static long CalculateSelectedBytes(IEnumerable<CleanupCategoryResult> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);
        return categories.Where(category => category.IsSelected).Sum(category => category.TotalBytes);
    }
}

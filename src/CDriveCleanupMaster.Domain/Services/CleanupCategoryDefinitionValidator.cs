using CDriveCleanupMaster.Domain.Enums;
using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Domain.Services;

/// <summary>
/// 校验规则定义是否合法。
/// </summary>
public static class CleanupCategoryDefinitionValidator
{
    public static void Validate(CleanupCategoryDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new ArgumentException("Category id is required.", nameof(definition));
        }

        if (string.IsNullOrWhiteSpace(definition.DisplayName))
        {
            throw new ArgumentException("Display name is required.", nameof(definition));
        }

        if (definition.Paths.Count == 0)
        {
            throw new ArgumentException("At least one path is required.", nameof(definition));
        }

        if (definition.IsSelectedByDefault && definition.RiskLevel != RiskLevel.Low)
        {
            throw new ArgumentException("Only low risk items may be selected by default.", nameof(definition));
        }
    }
}

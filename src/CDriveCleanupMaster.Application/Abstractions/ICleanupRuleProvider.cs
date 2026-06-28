using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Application.Abstractions;

/// <summary>
/// 规则提供器。
/// </summary>
public interface ICleanupRuleProvider
{
    Task<IReadOnlyList<CleanupCategoryDefinition>> LoadDefinitionsAsync(CancellationToken cancellationToken);
}

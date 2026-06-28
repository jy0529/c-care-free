using System.Text.Json;
using CDriveCleanupMaster.Application.Abstractions;
using CDriveCleanupMaster.Domain.Enums;
using CDriveCleanupMaster.Domain.Models;
using CDriveCleanupMaster.Domain.Services;
using CDriveCleanupMaster.Infrastructure.Configuration;

namespace CDriveCleanupMaster.Infrastructure.Services;

/// <summary>
/// JSON 规则文件读取器。
/// </summary>
public sealed class JsonCleanupRuleProvider : ICleanupRuleProvider
{
    private readonly string _filePath;

    public JsonCleanupRuleProvider(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<IReadOnlyList<CleanupCategoryDefinition>> LoadDefinitionsAsync(CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(_filePath);
        var documents = await JsonSerializer.DeserializeAsync<List<CleanupRuleDocument>>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken);

        var definitions = new List<CleanupCategoryDefinition>();
        foreach (var document in documents ?? [])
        {
            var definition = new CleanupCategoryDefinition
            {
                Id = document.Id,
                DisplayName = document.DisplayName,
                Description = document.Description,
                GroupName = document.GroupName,
                IconKey = document.IconKey,
                RiskLevel = Enum.Parse<RiskLevel>(document.RiskLevel, true),
                IsExecutable = document.IsExecutable,
                IsSelectedByDefault = document.IsSelectedByDefault,
                Paths = document.Paths.Select(Environment.ExpandEnvironmentVariables).ToArray()
            };

            CleanupCategoryDefinitionValidator.Validate(definition);
            definitions.Add(definition);
        }

        return definitions;
    }
}

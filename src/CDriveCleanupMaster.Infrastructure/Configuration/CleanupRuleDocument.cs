namespace CDriveCleanupMaster.Infrastructure.Configuration;

internal sealed class CleanupRuleDocument
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public bool IsExecutable { get; set; }
    public bool IsSelectedByDefault { get; set; }
    public List<string> Paths { get; set; } = new();
}

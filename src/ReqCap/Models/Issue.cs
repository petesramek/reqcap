
namespace ReqCap.Models;

public enum RequirementSeverity { Error, Warning }

public sealed class Issue
{
    public string Property { get; init; } = default!;
    public string Message { get; init; } = default!;
    public RequirementSeverity Severity { get; init; }
    public string? RuleName { get; init; }
    public string? RuleAlias { get; init; }
    public string? GroupName { get; set; }
    public string? GroupAlias { get; set; }
}

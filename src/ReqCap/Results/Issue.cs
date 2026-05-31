namespace ReqCap.Results;

/// <summary>
/// Describes a failed rule or warning produced by evaluation.
/// </summary>
public sealed class Issue
{
    /// <summary>
    /// Gets the property path associated with the issue.
    /// </summary>
    public string Property { get; init; } = string.Empty;

    /// <summary>
    /// Gets the issue message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the issue severity.
    /// </summary>
    public RequirementSeverity Severity { get; init; }

    /// <summary>
    /// Gets the optional rule name.
    /// </summary>
    public string? RuleName { get; init; }

    /// <summary>
    /// Gets the optional rule alias.
    /// </summary>
    public string? RuleAlias { get; init; }

    /// <summary>
    /// Gets or sets the optional group name.
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// Gets or sets the optional group alias.
    /// </summary>
    public string? GroupAlias { get; set; }
}

namespace ReqCap.Results;

/// <summary>
/// Represents a neutral rule match that can be projected into a consumer-defined result object.
/// </summary>
public sealed class RequirementMatch
{
    /// <summary>
    /// Gets the matched rule name.
    /// </summary>
    public string RuleName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional matched rule alias.
    /// </summary>
    public string? RuleAlias { get; init; }

    /// <summary>
    /// Gets the matched property path, if the match came from a property rule.
    /// </summary>
    public string? PropertyPath { get; init; }

    /// <summary>
    /// Gets the optional match message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the group name, if the match came from a group.
    /// </summary>
    public string? GroupName { get; init; }

    /// <summary>
    /// Gets the group alias, if the match came from a group.
    /// </summary>
    public string? GroupAlias { get; init; }
}

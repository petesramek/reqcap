namespace ReqCap.Results;

/// <summary>
/// Represents the result of evaluating a requirement.
/// </summary>
public sealed class EvaluationResult
{
    private static readonly EvaluationResult OkResult = new()
    {
        Allowed = true,
        Errors = Array.Empty<Issue>(),
        Warnings = Array.Empty<Issue>(),
    };

    /// <summary>
    /// Gets a value indicating whether the evaluated capability is allowed.
    /// </summary>
    public bool Allowed { get; init; }

    /// <summary>
    /// Gets the blocking issues produced by evaluation.
    /// </summary>
    public IReadOnlyList<Issue> Errors { get; init; } = Array.Empty<Issue>();

    /// <summary>
    /// Gets the non-blocking issues produced by evaluation.
    /// </summary>
    public IReadOnlyList<Issue> Warnings { get; init; } = Array.Empty<Issue>();

    /// <summary>
    /// Creates a successful evaluation result.
    /// </summary>
    /// <returns>A successful evaluation result.</returns>
    public static EvaluationResult Ok()
    {
        return OkResult;
    }

    /// <summary>
    /// Creates an evaluation result from an issue.
    /// </summary>
    /// <param name="issue">The issue to include in the result.</param>
    /// <returns>An evaluation result containing the issue.</returns>
    public static EvaluationResult FromIssue(Issue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return new EvaluationResult
        {
            Allowed = issue.Severity != RequirementSeverity.Error,
            Errors = issue.Severity == RequirementSeverity.Error
                ? new[] { issue }
                : Array.Empty<Issue>(),
            Warnings = issue.Severity == RequirementSeverity.Warning
                ? new[] { issue }
                : Array.Empty<Issue>(),
        };
    }
}

namespace ReqCap.Results;

/// <summary>
/// Represents the result of evaluating a requirement or rule.
/// </summary>
public sealed class EvaluationResult {
    /// <summary>Gets a value indicating whether evaluation passed without blocking errors.</summary>
    public bool Allowed { get; init; }
    /// <summary>Gets blocking errors produced during evaluation.</summary>
    public IReadOnlyList<Issue> Errors { get; init; } = [];
    /// <summary>Gets non-blocking warnings produced during evaluation.</summary>
    public IReadOnlyList<Issue> Warnings { get; init; } = [];
    /// <summary>Creates a successful evaluation result.</summary>
    /// <returns>A successful evaluation result.</returns>
    public static EvaluationResult Ok() => new() { Allowed = true };
    /// <summary>Creates an evaluation result from a single issue.</summary>
    /// <param name="issue">The issue to include.</param>
    /// <returns>An evaluation result containing the supplied issue.</returns>
    public static EvaluationResult FromIssue(Issue issue) {
        ArgumentNullException.ThrowIfNull(issue);
        return issue.Severity == RequirementSeverity.Error
            ? new EvaluationResult { Allowed = false, Errors = [issue] }
            : new EvaluationResult { Allowed = true, Warnings = [issue] };
    }
}

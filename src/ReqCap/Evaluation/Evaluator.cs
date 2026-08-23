namespace ReqCap.Evaluation;

using ReqCap.Abstractions;
using ReqCap.Requirements;
using ReqCap.Results;

/// <summary>
/// Evaluates capabilities against requirements.
/// </summary>
public static class Evaluator {
    /// <summary>
    /// Evaluates a capability against a requirement.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <param name="capability">The capability to evaluate.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <returns>The evaluation result.</returns>
    public static EvaluationResult Evaluate<TCapability>(
        TCapability capability,
        Requirement<TCapability> requirement)
        where TCapability : ICapability {
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(requirement);

        var errors = new List<Issue>();
        var warnings = new List<Issue>();

        foreach (var rule in requirement.Rules) {
            var result = rule.Evaluate(capability);
            errors.AddRange(result.Errors);
            warnings.AddRange(result.Warnings);
        }

        return new EvaluationResult {
            Allowed = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
        };
    }

    /// <summary>
    /// Determines whether a capability satisfies a requirement without projecting matched rules into result objects.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <param name="capability">The capability to evaluate.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <returns><see langword="true" /> when no rule matches; otherwise, <see langword="false" />.</returns>
    public static bool Satisfies<TCapability>(
        TCapability capability,
        Requirement<TCapability> requirement)
        where TCapability : ICapability {
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(requirement);

        foreach (var rule in requirement.Rules) {
            var result = rule.Evaluate(capability);
            if (result.Errors.Count > 0 || result.Warnings.Count > 0) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Evaluates a capability against a requirement and projects matched rules into consumer-defined result objects.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <typeparam name="TResult">The consumer-defined result type.</typeparam>
    /// <param name="capability">The capability to evaluate.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <param name="resultFactory">The factory used to create result objects from matched rules.</param>
    /// <returns>The projected evaluation result.</returns>
    public static EvaluationResult<TResult> Evaluate<TCapability, TResult>(
        TCapability capability,
        Requirement<TCapability> requirement,
        Func<RequirementMatch, TResult> resultFactory)
        where TCapability : ICapability {
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(requirement);
        ArgumentNullException.ThrowIfNull(resultFactory);

        List<TResult>? results = null;

        foreach (var rule in requirement.Rules) {
            var result = rule.Evaluate(capability);
            Add(results: ref results, result.Errors, resultFactory);
            Add(results: ref results, result.Warnings, resultFactory);
        }

        return results is null
            ? EvaluationResult<TResult>.SatisfiedResultInstance()
            : EvaluationResult<TResult>.FromResults(results);
    }

    private static void Add<TResult>(
        ref List<TResult>? results,
        IReadOnlyList<Issue> issues,
        Func<RequirementMatch, TResult> resultFactory) {
        foreach (var issue in issues) {
            results ??= [];
            results.Add(resultFactory(ToMatch(issue)));
        }
    }

    private static RequirementMatch ToMatch(Issue issue) {
        return new RequirementMatch {
            RuleName = issue.RuleName ?? string.Empty,
            RuleAlias = issue.RuleAlias,
            PropertyPath = string.IsNullOrEmpty(issue.Property) ? null : issue.Property,
            Message = issue.Message,
            GroupName = issue.GroupName,
            GroupAlias = issue.GroupAlias,
        };
    }
}

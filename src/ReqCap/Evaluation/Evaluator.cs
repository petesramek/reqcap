using ReqCap.Abstractions;
using ReqCap.Requirements;
using ReqCap.Results;

namespace ReqCap.Evaluation;

/// <summary>
/// Evaluates requirements against capability instances.
/// </summary>
public static class Evaluator
{
    /// <summary>
    /// Evaluates a requirement against a capability instance.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <param name="capability">The capability instance.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <returns>The evaluation result.</returns>
    public static EvaluationResult Evaluate<TCapability>(TCapability capability, Requirement<TCapability> requirement)
        where TCapability : ICapability
    {
        ArgumentNullException.ThrowIfNull(requirement);

        var errors = new List<Issue>();
        var warnings = new List<Issue>();

        foreach (var rule in requirement.Rules)
        {
            var result = rule.Evaluate(capability);
            errors.AddRange(result.Errors);
            warnings.AddRange(result.Warnings);
        }

        return new EvaluationResult
        {
            Allowed = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
        };
    }
}

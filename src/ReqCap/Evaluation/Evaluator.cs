
using ReqCap.Abstractions;
using ReqCap.Models;
using ReqCap.Builder;

namespace ReqCap.Evaluation;

public static class Evaluator
{
    public static EvaluationResult Evaluate<TCapability>(TCapability capability, RequirementModel<TCapability> requirement)
        where TCapability : ICapability
    {
        var errors = new List<Issue>();
        var warnings = new List<Issue>();

        foreach (var rule in requirement.Rules)
        {
            var result = rule.Evaluate(capability);

            if (!result.Allowed)
                errors.AddRange(result.Errors);

            warnings.AddRange(result.Warnings);
        }

        return new EvaluationResult
        {
            Allowed = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }
}

using ReqCap.Results;

namespace ReqCap.Abstractions;

/// <summary>
/// Represents an executable requirement rule for a capability type.
/// </summary>
/// <typeparam name="TCapability">The capability type evaluated by the rule.</typeparam>
public interface IRule<TCapability>
    where TCapability : ICapability {
    /// <summary>
    /// Evaluates the rule against the supplied capability instance.
    /// </summary>
    /// <param name="capability">The capability instance to evaluate.</param>
    /// <returns>The evaluation result.</returns>
    EvaluationResult Evaluate(TCapability capability);
}

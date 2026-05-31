using ReqCap.Models;

namespace ReqCap.Abstractions;

public interface IRule<TCapability>
    where TCapability : ICapability {
    EvaluationResult Evaluate(TCapability capability);
}


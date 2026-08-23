namespace ReqCap.Rules;

using ReqCap.Abstractions;
using ReqCap.Results;
using System.Linq.Expressions;

/// <summary>
/// Represents a single issue condition for a capability property.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
public sealed class ComparisonRule<TCapability, TProperty> : IRule<TCapability>
    where TCapability : ICapability {
    private readonly PropertyRuleChain<TCapability, TProperty> _chain;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComparisonRule{TCapability, TProperty}" /> class.
    /// </summary>
    /// <param name="expression">The property expression.</param>
    /// <param name="expected">The expected value used by the issue condition.</param>
    /// <param name="operator">The comparison operator that describes the issue condition.</param>
    /// <param name="severity">The severity used when the condition matches.</param>
    /// <param name="ruleName">The optional rule name.</param>
    /// <param name="ruleAlias">The optional rule alias.</param>
    /// <param name="message">The optional issue message.</param>
    public ComparisonRule(
        Expression<Func<TCapability, TProperty>> expression,
        TProperty expected,
        ComparisonOperator @operator,
        RequirementSeverity severity,
        string? ruleName = null,
        string? ruleAlias = null,
        string? message = null) {
        _chain = new PropertyRuleChain<TCapability, TProperty>(expression);
        _chain.Add(new PropertyCondition<TProperty>(
            @operator,
            expected,
            severity,
            ruleName,
            ruleAlias,
            message));
    }

    /// <inheritdoc />
    public EvaluationResult Evaluate(TCapability capability) {
        return _chain.Evaluate(capability);
    }
}

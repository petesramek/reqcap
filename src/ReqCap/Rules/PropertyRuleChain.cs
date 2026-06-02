using System.Linq.Expressions;
using ReqCap.Abstractions;
using ReqCap.Results;

namespace ReqCap.Rules;

/// <summary>
/// Represents an ordered chain of issue conditions for a single property.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
public sealed class PropertyRuleChain<TCapability, TProperty> : IRule<TCapability>
    where TCapability : ICapability
{
    private readonly Func<TCapability, TProperty> _getter;
    private readonly List<PropertyCondition<TProperty>> _conditions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRuleChain{TCapability, TProperty}" /> class.
    /// </summary>
    /// <param name="expression">The property expression.</param>
    public PropertyRuleChain(Expression<Func<TCapability, TProperty>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        PropertyPath = ExpressionPath.GetPropertyPath(expression.Body);
        _getter = expression.Compile();
    }

    /// <summary>
    /// Gets the property path evaluated by this chain.
    /// </summary>
    public string PropertyPath { get; }

    internal bool HasConditions => _conditions.Count > 0;

    internal void Add(PropertyCondition<TProperty> condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        _conditions.Add(condition);
    }

    /// <inheritdoc />
    public EvaluationResult Evaluate(TCapability capability)
    {
        if (_conditions.Count == 0)
        {
            return EvaluationResult.Ok();
        }

        TProperty actual;

        try
        {
            actual = _getter(capability);
        }
        catch (NullReferenceException)
        {
            var first = _conditions[0];
            return CreateIssueResult(
                first,
                $"{PropertyPath} could not be evaluated because part of the property path was null.");
        }

        foreach (var condition in _conditions)
        {
            if (condition.Matches(actual))
            {
                return CreateIssueResult(
                    condition,
                    condition.Message ?? GetDefaultMessage(condition));
            }
        }

        return EvaluationResult.Ok();
    }

    private string GetDefaultMessage(PropertyCondition<TProperty> condition)
    {
        return condition.Type == PropertyConditionType.Null
            ? $"{PropertyPath} matched condition Null."
            : $"{PropertyPath} matched condition {condition.Operator} {condition.Expected}.";
    }

    private EvaluationResult CreateIssueResult(PropertyCondition<TProperty> condition, string message)
    {
        var issue = new Issue
        {
            Property = PropertyPath,
            Message = message,
            Severity = condition.Severity,
            RuleName = condition.RuleName,
            RuleAlias = condition.RuleAlias,
        };

        return EvaluationResult.FromIssue(issue);
    }
}

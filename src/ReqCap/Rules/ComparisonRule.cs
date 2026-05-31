using ReqCap.Abstractions;
using ReqCap.Results;
using System.Linq.Expressions;

namespace ReqCap.Rules;

/// <summary>
/// Represents a built-in comparison rule for a comparable capability property.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
/// <typeparam name="TProperty">The comparable property type.</typeparam>
public sealed class ComparisonRule<TCapability, TProperty> : IRule<TCapability>
    where TCapability : ICapability
    where TProperty : IComparable<TProperty> {
    private readonly Func<TCapability, TProperty> _getter;
    private readonly TProperty _expected;
    private readonly ComparisonOperator _operator;
    private readonly RequirementSeverity _severity;
    private readonly string _propertyPath;
    private readonly string? _ruleName;
    private readonly string? _ruleAlias;
    private readonly string? _message;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComparisonRule{TCapability, TProperty}"/> class.
    /// </summary>
    /// <param name="expression">The property expression.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="operator">The comparison operator.</param>
    /// <param name="severity">The severity used when the rule fails.</param>
    /// <param name="ruleName">The optional rule name.</param>
    /// <param name="ruleAlias">The optional rule alias.</param>
    /// <param name="message">The optional failure message.</param>
    public ComparisonRule(Expression<Func<TCapability, TProperty>> expression, TProperty expected, ComparisonOperator @operator, RequirementSeverity severity, string? ruleName = null, string? ruleAlias = null, string? message = null) {
        ArgumentNullException.ThrowIfNull(expression);
        _propertyPath = ExpressionPath.GetPropertyPath(expression.Body);
        _getter = expression.Compile();
        _expected = expected;
        _operator = @operator;
        _severity = severity;
        _ruleName = ruleName;
        _ruleAlias = ruleAlias;
        _message = message;
    }

    /// <inheritdoc />
    public EvaluationResult Evaluate(TCapability capability) {
        TProperty actual;
        try { actual = _getter(capability); } catch (NullReferenceException) {
            return CreateIssueResult($"{_propertyPath} could not be evaluated because part of the property path was null.");
        }
        return EvaluateComparison(actual)
            ? EvaluationResult.Ok()
            : CreateIssueResult(_message ?? $"{_propertyPath} must be {_operator} {_expected}, but was {actual}.");
    }

    private bool EvaluateComparison(TProperty actual) {
        var comparison = actual.CompareTo(_expected);
        return _operator switch {
            ComparisonOperator.GreaterOrEqual => comparison >= 0,
            ComparisonOperator.GreaterThan => comparison > 0,
            ComparisonOperator.LessOrEqual => comparison <= 0,
            ComparisonOperator.LessThan => comparison < 0,
            ComparisonOperator.Equal => Equals(actual, _expected),
            ComparisonOperator.NotEqual => !Equals(actual, _expected),
            _ => false,
        };
    }

    private EvaluationResult CreateIssueResult(string message) {
        var issue = new Issue { Property = _propertyPath, Message = message, Severity = _severity, RuleName = _ruleName, RuleAlias = _ruleAlias };
        return EvaluationResult.FromIssue(issue);
    }
}

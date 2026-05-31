
using System.Linq.Expressions;
using ReqCap.Abstractions;
using ReqCap.Models;

namespace ReqCap.Rules;

public sealed class ComparisonRule<TCapability, TProperty> : IRule<TCapability>
    where TCapability : ICapability
    where TProperty : IComparable<TProperty>
{
    private readonly Func<TCapability, TProperty> _getter;
    private readonly TProperty _expected;
    private readonly ComparisonOperator _operator;
    private readonly RequirementSeverity _severity;
    private readonly string _propertyName;
    private readonly string? _ruleName;
    private readonly string? _ruleAlias;

    public ComparisonRule(
        Expression<Func<TCapability, TProperty>> expression,
        TProperty expected,
        ComparisonOperator @operator,
        RequirementSeverity severity,
        string? ruleName = null,
        string? ruleAlias = null)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (expression.Body is not MemberExpression member)
            throw new ArgumentException("Expression must be member access", nameof(expression));

        _getter = expression.Compile();
        _expected = expected;
        _operator = @operator;
        _severity = severity;
        _propertyName = member.Member.Name;
        _ruleName = ruleName;
        _ruleAlias = ruleAlias;
    }

    public EvaluationResult Evaluate(TCapability instance)
    {
        var actual = _getter(instance);

        var passed = EvaluateComparison(actual);

        if (passed)
            return EvaluationResult.Ok();

        var issue = new Issue
        {
            Property = _propertyName,
            Message = $"{_propertyName} must be {_operator} {_expected}, but was {actual}",
            Severity = _severity,
            RuleName = _ruleName,
            RuleAlias = _ruleAlias
        };

        return issue.Severity == RequirementSeverity.Error
            ? new EvaluationResult { Allowed = false, Errors = new[] { issue } }
            : new EvaluationResult { Allowed = true, Warnings = new[] { issue } };
    }

    private bool EvaluateComparison(TProperty actual)
    {
        var cmp = actual.CompareTo(_expected);

        return _operator switch
        {
            ComparisonOperator.GreaterOrEqual => cmp >= 0,
            ComparisonOperator.GreaterThan => cmp > 0,
            ComparisonOperator.LessOrEqual => cmp <= 0,
            ComparisonOperator.LessThan => cmp < 0,
            ComparisonOperator.Equal => Equals(actual, _expected),
            ComparisonOperator.NotEqual => !Equals(actual, _expected),
            _ => false
        };
    }
}

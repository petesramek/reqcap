using ReqCap.Internal;
using ReqCap.Results;

namespace ReqCap.Rules;

internal sealed class PropertyCondition<TProperty>
    where TProperty : IComparable<TProperty>
{
    public PropertyCondition(
        ComparisonOperator op,
        TProperty expected,
        RequirementSeverity severity,
        string? ruleName,
        string? ruleAlias,
        string? message)
    {
        ArgumentValidation.ThrowIfInvalidEnum(op, nameof(op));
        ArgumentValidation.ThrowIfInvalidEnum(severity, nameof(severity));
        ArgumentValidation.ThrowIfWhiteSpace(ruleName, nameof(ruleName));
        ArgumentValidation.ThrowIfWhiteSpace(ruleAlias, nameof(ruleAlias));
        ArgumentValidation.ThrowIfWhiteSpace(message, nameof(message));

        if (expected is null)
        {
            throw new ArgumentNullException(nameof(expected));
        }

        Operator = op;
        Expected = expected;
        Severity = severity;
        RuleName = ruleName;
        RuleAlias = ruleAlias;
        Message = message;
    }

    public ComparisonOperator Operator { get; }

    public TProperty Expected { get; }

    public RequirementSeverity Severity { get; }

    public string? RuleName { get; }

    public string? RuleAlias { get; }

    public string? Message { get; }

    public bool Matches(TProperty actual)
    {
        if (actual is null)
        {
            return Operator switch
            {
                ComparisonOperator.Equal => Expected is null,
                ComparisonOperator.NotEqual => Expected is not null,
                _ => false,
            };
        }

        var comparison = actual.CompareTo(Expected);

        return Operator switch
        {
            ComparisonOperator.GreaterOrEqual => comparison >= 0,
            ComparisonOperator.GreaterThan => comparison > 0,
            ComparisonOperator.LessOrEqual => comparison <= 0,
            ComparisonOperator.LessThan => comparison < 0,
            ComparisonOperator.Equal => Equals(actual, Expected),
            ComparisonOperator.NotEqual => !Equals(actual, Expected),
            _ => false,
        };
    }
}

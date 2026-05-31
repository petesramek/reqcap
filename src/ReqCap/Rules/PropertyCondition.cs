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

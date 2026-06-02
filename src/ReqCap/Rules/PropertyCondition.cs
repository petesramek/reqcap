using ReqCap.Internal;
using ReqCap.Results;

namespace ReqCap.Rules;

internal sealed class PropertyCondition<TProperty>
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
            throw new ArgumentNullException(
                nameof(expected),
                "Expected comparison value cannot be null. Use Null() to match null property values.");
        }

        ThrowIfUnsupportedComparison(op);

        Type = PropertyConditionType.Comparison;
        Operator = op;
        Expected = expected;
        Severity = severity;
        RuleName = ruleName;
        RuleAlias = ruleAlias;
        Message = message;
    }

    private PropertyCondition(
        PropertyConditionType type,
        RequirementSeverity severity,
        string? ruleName,
        string? ruleAlias,
        string? message)
    {
        ArgumentValidation.ThrowIfInvalidEnum(type, nameof(type));
        ArgumentValidation.ThrowIfInvalidEnum(severity, nameof(severity));
        ArgumentValidation.ThrowIfWhiteSpace(ruleName, nameof(ruleName));
        ArgumentValidation.ThrowIfWhiteSpace(ruleAlias, nameof(ruleAlias));
        ArgumentValidation.ThrowIfWhiteSpace(message, nameof(message));

        Type = type;
        Severity = severity;
        RuleName = ruleName;
        RuleAlias = ruleAlias;
        Message = message;
    }

    public PropertyConditionType Type { get; }

    public ComparisonOperator Operator { get; }

    public TProperty? Expected { get; }

    public RequirementSeverity Severity { get; }

    public string? RuleName { get; }

    public string? RuleAlias { get; }

    public string? Message { get; }

    public static PropertyCondition<TProperty> Null(
        RequirementSeverity severity,
        string? ruleName,
        string? ruleAlias,
        string? message)
    {
        return new PropertyCondition<TProperty>(
            PropertyConditionType.Null,
            severity,
            ruleName,
            ruleAlias,
            message);
    }

    public bool Matches(TProperty? actual)
    {
        return Type switch
        {
            PropertyConditionType.Null => actual is null,
            PropertyConditionType.Comparison => MatchesComparison(actual),
            _ => false,
        };
    }

    private bool MatchesComparison(TProperty? actual)
    {
        if (actual is null)
        {
            return false;
        }

        return Operator switch
        {
            ComparisonOperator.Equal => Equals(actual, Expected),
            ComparisonOperator.NotEqual => !Equals(actual, Expected),
            ComparisonOperator.GreaterOrEqual => Compare(actual) >= 0,
            ComparisonOperator.GreaterThan => Compare(actual) > 0,
            ComparisonOperator.LessOrEqual => Compare(actual) <= 0,
            ComparisonOperator.LessThan => Compare(actual) < 0,
            _ => false,
        };
    }

    private int Compare(TProperty actual)
    {
        return Comparer<TProperty>.Default.Compare(actual, Expected!);
    }

    private static void ThrowIfUnsupportedComparison(ComparisonOperator op)
    {
        if (op is ComparisonOperator.Equal or ComparisonOperator.NotEqual)
        {
            return;
        }

        var propertyType = typeof(TProperty);
        var nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);
        var comparableType = nullableUnderlyingType ?? propertyType;
        var genericComparableType = typeof(IComparable<>).MakeGenericType(comparableType);

        if (genericComparableType.IsAssignableFrom(comparableType) || typeof(IComparable).IsAssignableFrom(comparableType))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Property type '{propertyType.Name}' cannot be used with comparison operator '{op}' because it does not support comparison.");
    }
}

namespace ReqCap.Rules;

/// <summary>
/// Defines comparison operators used by built-in issue conditions.
/// </summary>
public enum ComparisonOperator
{
    /// <summary>
    /// Matches when the actual value is greater than or equal to the expected value.
    /// </summary>
    GreaterOrEqual,

    /// <summary>
    /// Matches when the actual value is greater than the expected value.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Matches when the actual value is less than or equal to the expected value.
    /// </summary>
    LessOrEqual,

    /// <summary>
    /// Matches when the actual value is less than the expected value.
    /// </summary>
    LessThan,

    /// <summary>
    /// Matches when the actual value is equal to the expected value.
    /// </summary>
    Equal,

    /// <summary>
    /// Matches when the actual value is different from the expected value.
    /// </summary>
    NotEqual,
}
